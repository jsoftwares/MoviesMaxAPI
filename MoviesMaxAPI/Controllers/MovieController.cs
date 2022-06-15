using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesMaxAPI.DTOs;
using MoviesMaxAPI.Entities;
using MoviesMaxAPI.Helpers;

namespace MoviesMaxAPI.Controllers
{
    [Route("api/movies")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class MovieController : ControllerBase
    {
        private readonly ApplicationDbContext db;
        private readonly IMapper mapper;
        private readonly IFileStorageService fileStorageService;
        private readonly UserManager<IdentityUser> userManager;
        private readonly string folderName = "movies";

        public MovieController(ApplicationDbContext applicationDbContext, IMapper mapper, IFileStorageService fileStorageService, UserManager<IdentityUser> userManager)
        {
            this.db = applicationDbContext;
            this.mapper = mapper;
            this.fileStorageService = fileStorageService;
            this.userManager = userManager;
        }


        [HttpGet()]
        [AllowAnonymous]
        public async Task<ActionResult<LandingPageDTO>> Get()
        {
            var top = 6;
            var today = DateTime.Today;

            var upcomingReleases = await db.Movies
                .Where(x => x.ReleaseDate > today)
                .OrderBy(x => x.ReleaseDate)
                .Take(top)
                .ToListAsync();

            var inTheatres = await db.Movies
                .Where(x => x.InTheatres)
                .OrderBy(x => x.ReleaseDate)
                .Take(top)
                .ToListAsync();

            var landingPageDTO = new LandingPageDTO();
            landingPageDTO.InTheatres = mapper.Map<List<MovieDTO>>(inTheatres);
            landingPageDTO.UpcomingReleases = mapper.Map<List<MovieDTO>>(upcomingReleases);

            return landingPageDTO;
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<MovieDTO>> Get(int id)
        {
            //load in d info of d intermetiate relationship table & then chain on that to load d related Genres for the movie.
            //we do same for MovieTheatres and Actors for the related movie
            var movie = await db.Movies
                .Include(x=> x.MoviesGenres).ThenInclude(x => x.Genre)      
                .Include(x => x.MovieTheatresMovies).ThenInclude(x => x.MovieTheatre)
                .Include(x => x.MoviesActors).ThenInclude(x => x.Actor)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (movie == null)
            {
                return NotFound();
            }

            var averageVote = 0.0;
            var userVote = 0;

            //check if Rating table has records corresponding to this movie ID
            if (await db.Ratings.AnyAsync(x => x.MovieId == id))
            {
                averageVote = await db.Ratings.Where(x => x.MovieId == id).AverageAsync(x => x.Rate);

                //get the vote of d user making this request. We first check that the user is Authenticated
                if (HttpContext.User.Identity.IsAuthenticated)  //this works with [Authorize]
                {
                    var email = HttpContext.User.Claims.FirstOrDefault(x => x.Type ==  "email").Value;
                    var user = await userManager.FindByEmailAsync(email);
                    var userId = user.Id;

                    var ratingDb = await db.Ratings.FirstOrDefaultAsync(x => x.MovieId == id && x.UserId == userId);
                    if (ratingDb != null)
                    {
                        userVote = ratingDb.Rate;
                    }
                }

            }

            var dto = mapper.Map<MovieDTO>(movie);
            dto.AverageVote = averageVote;
            dto.UserVote = userVote;
            dto.Actors = dto.Actors.ToList();

            return dto;
        }

        [HttpGet("filter")]
        [AllowAnonymous]
        public async Task<ActionResult<List<MovieDTO>>> Filter([FromQuery] FilterMoviesDTO filterMoviesDTO)
        {
            //we'd use defer execution in EF to do this; with this we'd build d query line-by-line & that's going allow us condit-
            //ionally build the query.
            var moviesQueryable = db.Movies.AsQueryable();

            //if Title is sent from d client in querystring & it's not empty str, we'd not apply Title as part of the Filters
            if (!string.IsNullOrEmpty(filterMoviesDTO.Title))
            {
                moviesQueryable = moviesQueryable.Where(x => x.Title.Contains(filterMoviesDTO.Title));
            }

            if (filterMoviesDTO.InTheatres)
            {
                moviesQueryable = moviesQueryable.Where(x => x.InTheatres);
            }

            if (filterMoviesDTO.UpcomingReleases)
            {
                var today = DateTime.Today;
                moviesQueryable = moviesQueryable.Where(x => x.ReleaseDate > today);
            }

            //We are querying by a realationship here
            if (filterMoviesDTO.GenreId != 0)
            {
                moviesQueryable = moviesQueryable.Where(x => x.MoviesGenres.Select(y => y.GenreId)
                .Contains(filterMoviesDTO.GenreId));
            }

            //REM we have this helper method that helps us easily Paginate
            await HttpContext.InsertParametersPaginationInHeader(moviesQueryable);
            var movies = await moviesQueryable.OrderBy(x => x.Title).Paginate(filterMoviesDTO.PaginationDTO).ToListAsync();

            return mapper.Map<List<MovieDTO>>(movies);
        }

        //endpoint to return all genres and movietheatres that we would display for selection on our movie creation page
        [HttpGet("PostGet")]
        public async Task<ActionResult<MoviePostGetDTO>> PostGet()
        {
            var movieTheatre = await db.MovieTheatres.ToListAsync();
            var genres = await db.Genres.ToListAsync();

            var movieTheatresDTO = mapper.Map<List<MovieTheatreDTO>>(movieTheatre);
            var genresDTO = mapper.Map<List<GenreDTO>>(genres);

            return new MoviePostGetDTO() { Genres = genresDTO, MovieTheatres = movieTheatresDTO };
        }

        [HttpPost]
        public async Task<ActionResult<int>> Post([FromForm] MovieCreationDTO movieCreationDTO)
        {
            var movie = mapper.Map<Movie>(movieCreationDTO);
            
            if (movieCreationDTO.Poster != null)
            {
                movie.Poster = await fileStorageService.SaveFile(folderName, movieCreationDTO.Poster);
            }

            AnnotateActorsOrder(movie);
            db.Add(movie);
            await db.SaveChangesAsync();

            return movie.Id;
            //we are done with this, now we need to go to our Automapper class to configure d necessary mapping rules for Movies
        }

        //Anotate the orders of the actors; modify d Order property of the MoviesActors entity so that so that we can save in DB
        // d order in which they should appear in the movie details page. Here we are saving it in d order d actors came from the frontend.
        private void AnnotateActorsOrder(Movie movie)
        {
            if (movie.MoviesActors != null)
            {
                for (int i = 0; i < movie.MoviesActors.Count; i++)
                {
                    movie.MoviesActors[i].Order = i;
                }
            }
        }

        //endpoint to Get & return d movie we want to edit, we can d movie by ID but also add other Actors, Genres, Theatres that
        // are not selected for this movie to dislay on d edit page so that they can be selected from if needed.
        [HttpGet("putGet/{id:int}")]
        public async Task<ActionResult<MoviePutGetDTO>> PutGet(int id)
        {
            var movieActionResult = await Get(id);  //we are reusing out Get(int id) method in this class
            //if d Result we get from the response is a NotFoundResult
            if (movieActionResult.Result is NotFoundResult) { return NotFound(); }

            var movie = movieActionResult.Value;

            //Get all the Genre IDs that has this movie ID associated with then in them as in the movie-genres table
            var genresSelectedIds = movie.Genres.Select(x => x.Id).ToList();
            // get all Genres that does not include d Genres with d IDs in the list genresSelectedIds
            var nonSelectedGenres = await db.Genres.Where(x => !genresSelectedIds.Contains(x.Id)).ToListAsync();

            var movieTheatresSelectedIds = movie.MovieTheatres.Select(x => x.Id).ToList();
            var nonSelectedMovieTheatres = await db.MovieTheatres.Where(x => !movieTheatresSelectedIds.Contains(x.Id)).ToListAsync();

            var nonSelectedGenresDTOs = mapper.Map<List<GenreDTO>>(nonSelectedGenres);
            var nonSelectedMovieTheatresDTOs = mapper.Map<List<MovieTheatreDTO>>(nonSelectedMovieTheatres);

            var response = new MoviePutGetDTO()

            {
                Movie = movie,
                SelectedGenres = movie.Genres,
                NonSelectedGenres = nonSelectedGenresDTOs,
                SelectedMovieTheatres = movie.MovieTheatres,
                NonSelectedMovieTheatres = nonSelectedMovieTheatresDTOs,
                Actors = movie.Actors
            };

            return response;
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, [FromForm] MovieCreationDTO movieCreationDTO)
        {
            // We get d movie & include all of d relationships bcos this way we will be able to update d movie & all of its relationships
            var movie = await db.Movies.Include(x => x.MoviesActors)
                .Include(x => x.MovieTheatresMovies)
                .Include(x => x.MoviesGenres)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (movie == null) { return NotFound(); }

            movie = mapper.Map(movieCreationDTO, movie);    //this carries out the fields update; map d movieCreationDTO we receive with movie we found in DB

            if (movieCreationDTO.Poster != null)
            {
                movie.Poster = await fileStorageService.EditFile(folderName, movieCreationDTO.Poster, movie.Poster);
            }

            AnnotateActorsOrder(movie);
            await db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var movie = await db.Movies.FirstOrDefaultAsync(x => x.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            db.Remove(movie);
            await db.SaveChangesAsync();
            await fileStorageService.DeleteFile(movie.Poster, folderName);

            return NoContent();
        }
    }
}
