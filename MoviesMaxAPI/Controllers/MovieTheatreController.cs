using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesMaxAPI.DTOs;
using MoviesMaxAPI.Entities;

namespace MoviesMaxAPI.Controllers
{
    [Route("api/movietheatres")]
    [ApiController]
    public class MovieTheatreController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IMapper mapper;

        public MovieTheatreController(ApplicationDbContext db, IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }
        [HttpGet]
        public async Task<ActionResult<List<MovieTheatreDTO>>> Get()
        {
            var entities = await db.MovieTheatres.ToListAsync();
            return mapper.Map<List<MovieTheatreDTO>>(entities); //for this mapping, we need to pass d GeometryFactory into our AutoMapper configuration class                      
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<MovieTheatreDTO>> Get(int id)
        {
            var movieTheatre = await db.MovieTheatres.FirstOrDefaultAsync(x => x.Id == id);
            if (movieTheatre == null)
            {
                return NotFound();
            }

            return mapper.Map<MovieTheatreDTO>(movieTheatre);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] MovieTheatreCreationDTO movieTheatreCreationDTO)
        {
            var movieTheatre = mapper.Map<MovieTheatre>(movieTheatreCreationDTO);
            db.Add(movieTheatre);
            await db.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, [FromBody] MovieTheatreCreationDTO movieTheatreCreationDTO)
        {
            var movieTheatre = await db.MovieTheatres.FirstOrDefaultAsync(x => x.Id == id);
            if (movieTheatre == null)
            {
                return NotFound();
            }

            movieTheatre = mapper.Map(movieTheatreCreationDTO, movieTheatre);
            await db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var movieTheatre = await db.MovieTheatres.FirstOrDefaultAsync(x => x.Id == id);
            if (movieTheatre == null)
            {
                return NotFound();
            }

            db.Remove(movieTheatre);
            await db.SaveChangesAsync();

            return NoContent();
        }
    }
}
