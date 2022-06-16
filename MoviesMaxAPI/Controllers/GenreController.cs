using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesMaxAPI.DTOs;
using MoviesMaxAPI.Entities;
using MoviesMaxAPI.Helpers;

namespace MoviesMaxAPI.Controllers
{    
    [Route("api/genres")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "IsAdmin")]
    public class GenreController : ControllerBase
    {
        private readonly ILogger<GenreController> logger;
        private readonly ApplicationDbContext _context;
        private readonly IMapper mapper;

        public GenreController(ILogger<GenreController> logger, ApplicationDbContext context, IMapper mapper)
        {
            this.logger = logger;
            this._context = context;
            this.mapper = mapper;
        }

        [HttpGet]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        /**Since we a recieving a complex type ie Pagination, so we need to pass [FromQuery] from our Action
         * AsQueryable() allows us build step-by-step d query we are going to send to our DB provider (MSSQL Server in this case)
         * We get thte total number of records in the table we want to query. Since we want to use pagination in different places, 
         * we centralized d operation in Helpers/HttpContextExtension.cs also in IQueryableExtensions.cs
         * 
         */
        public async Task<ActionResult<List<GenreDTO>>> Get([FromQuery] PaginationDTO paginationDTO)
        {
            var queryable = _context.Genres.AsQueryable();
            await HttpContext.InsertParametersPaginationInHeader(queryable);
            var genres = await queryable.OrderBy(x=>x.Name).Paginate(paginationDTO).ToListAsync();
            //var genres = await _context.Genres.ToListAsync();

            //var genresDTO = new List<GenreDTO>();
            //foreach (var genre in genres)
            //{
            //    genresDTO.Add(new GenreDTO() { Id=genre.Id, Name = genre.Name});
            //}

            //return genresDTO;
            
            //we convert Genre to GenreDTO and send to our client because its not good practice to expose your model to the outside world
            //we also have to configure this mapping in Helpers/AutoMapperProfiles.cs
            return mapper.Map<List<GenreDTO>>(genres);

        }

        [HttpGet("all")]
        [AllowAnonymous]
        public async Task<ActionResult<List<GenreDTO>>> Get()
        {
            var genres = await _context.Genres.ToListAsync();      
            return mapper.Map<List<GenreDTO>>(genres);

        }

        [HttpGet("{Id:int}")]
        public async Task<ActionResult<GenreDTO>> Get(int Id)
        {
            var genre = await _context.Genres.FirstOrDefaultAsync(x=> x.Id == Id);
            if (genre == null)
            {
                logger.LogWarning($"Genre will ID {Id} is not found");
                return NotFound();
            }

            return mapper.Map<GenreDTO>(genre);
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

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, [FromBody] GenreCreationDTO genreCreationDTO)
        {
            var genre = await _context.Genres.FirstOrDefaultAsync(x => x.Id == id);
            if (genre == null)
            {
                return NotFound();
            }

            //here we are mapping what we received from client(genrecreationDTO) into genre we got from DB. EntityFramework   Core will handle the 
            // updating when we call SaveChangesAsync()
            genre = mapper.Map(genreCreationDTO, genre);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var exists = await _context.Genres.AnyAsync(x => x.Id == id);

            if (!exists)
            {
                return NotFound();
            }

            _context.Remove(new Genre() { Id=id});
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
