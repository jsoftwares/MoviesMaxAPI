using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesMaxAPI.DTOs;
using MoviesMaxAPI.Entities;
using MoviesMaxAPI.Services;

namespace MoviesMaxAPI.Controllers
{    
    [Route("api/genres")]
    [ApiController]
    public class GenreController : Controller
    {
        private readonly IRepository _repository;
        private readonly ILogger<GenreController> logger;
        private readonly ApplicationDbContext _context;
        private readonly IMapper mapper;

        public GenreController(IRepository repository, ILogger<GenreController> logger, ApplicationDbContext context, IMapper mapper)
        {
            this._repository = repository;
            this.logger = logger;
            this._context = context;
            this.mapper = mapper;
        }

        [HttpGet]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<List<GenreDTO>>> Get()
        {
            var genres = await _context.Genres.ToListAsync();
            //var genresDTO = new List<GenreDTO>();
            //foreach (var genre in genres)
            //{
            //    genresDTO.Add(new GenreDTO() { Id=genre.Id, Name = genre.Name});
            //}

            //return genresDTO;
            
            //we concert Genre to GenreDTO and send to our client because its not good practice to expose your model to the outside world
            //we also have to configure this mapping in Helpers/AutoMapperProfiles.cs
            return mapper.Map<List<GenreDTO>>(genres);

        }

        [HttpGet("{Id:int}")]
        public ActionResult<Genre> Get(int Id)
        {
            var genre = _repository.GetGenreById(Id);
            if (genre == null)
            {
                logger.LogWarning($"Genre will ID {Id} is not found");
                return NotFound();
            }
            return genre;
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] GenreCreationDTO genreCreationDTO)
        {
            //during creation, our DB doesn't understand what a genreCreationDTO is so, we use mapper to convert genreCreationDTO to Genre which is what our DB is expecting.
            //we also have to configure this mapping in Helpers/AutoMapperProfiles.cs
            var genre = mapper.Map<Genre>(genreCreationDTO);
            _context.Genres.Add(genre);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut]
        public ActionResult Put([FromBody] Genre genre)
        {
            throw new NotImplementedException();
        }

        [HttpDelete]
        public ActionResult Delete()
        {
            throw new NotImplementedException();
        }
    }
}
