using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesMaxAPI.DTOs;
using MoviesMaxAPI.Entities;
using MoviesMaxAPI.Helpers;

namespace MoviesMaxAPI.Controllers
{
    [Route("api/movies")]
    [ApiController]
    public class MovieController : ControllerBase
    {
        private readonly ApplicationDbContext db;
        private readonly IMapper mapper;
        private readonly IFileStorageService fileStorageService;
        private readonly string folderName = "movies";

        public MovieController(ApplicationDbContext applicationDbContext, IMapper mapper, IFileStorageService fileStorageService)
        {
            this.db = applicationDbContext;
            this.mapper = mapper;
            this.fileStorageService = fileStorageService;
        }


        [HttpGet()]
        public async Task<ActionResult<LandingPageDTO>> Get()
        {
            var top = 6;
            var today = DateTime.Today;

            var upcomingReleases = await db.Movies
                .Where(x => x.ReleaseDate > today)
                .OrderBy(x => x.ReleaseDate)
                .Take(top)
                .ToListAsync();

            var intheatres = await db.Movies
                .Where(x => x.InTheatres)
                .OrderBy(x => x.ReleaseDate)
                .Take(top)
                .ToListAsync();

            var landingPageDTO = new LandingPageDTO();
            landingPageDTO.Intheatres = mapper.Map<List<MovieDTO>>(intheatres);
            landingPageDTO.UpcomingReleases = mapper.Map<List<MovieDTO>>(upcomingReleases);

            return landingPageDTO;
        }

        [HttpGet("{id:int}")]
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

            var dto = mapper.Map<MovieDTO>(movie);
            dto.Actors = dto.Actors.ToList();

            return dto;
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
                Actots = movie.Actors
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
    }
}
