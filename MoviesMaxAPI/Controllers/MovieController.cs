﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesMaxAPI.DTOs;
using MoviesMaxAPI.Entities;
using MoviesMaxAPI.Helpers;

namespace MoviesMaxAPI.Controllers
{
    [Route("api/movies")]
    [ApiController]
    public class MovieController : Controller
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

        [HttpPost]
        public async Task<ActionResult> Post([FromForm] MovieCreationDTO movieCreationDTO)
        {
            var movie = mapper.Map<Movie>(movieCreationDTO);
            
            if (movieCreationDTO.Poster != null)
            {
                movie.Poster = await fileStorageService.SaveFile(folderName, movieCreationDTO.Poster);
            }

            AnnotateActorsOrder(movie);
            db.Add(movie);
            await db.SaveChangesAsync();

            return NoContent();
        }

        //Anotate the orders of the actors; modify d Order property of the MoviesActors entity so that so that we can save in DB
        // d order in which they should appear in the movie details page. Here we are saving it in d order d actors came from the frontend.
        private void AnnotateActorsOrder(Movie movie
            )
        {
            if (movie.MovieActors != null)
            {
                for (int i = 0; i < movie.MovieActors.Count; i++)
                {
                    movie.MovieActors[i].Order = i;
                }
            }
        } 
    }
}
