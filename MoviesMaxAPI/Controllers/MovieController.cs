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
        private void AnnotateActorsOrder(Movie movie
            )
        {
            if (movie.MoviesActors != null)
            {
                for (int i = 0; i < movie.MoviesActors.Count; i++)
                {
                    movie.MoviesActors[i].Order = i;
                }
            }
        } 
    }
}
