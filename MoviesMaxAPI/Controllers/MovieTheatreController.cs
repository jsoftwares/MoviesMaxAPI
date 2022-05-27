using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesMaxAPI.DTOs;

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
    }
}
