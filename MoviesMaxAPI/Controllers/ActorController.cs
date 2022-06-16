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
    [Route("api/actors")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "IsAdmin")]
    public class ActorController : ControllerBase
    {
        private readonly ApplicationDbContext db;
        private readonly IMapper mapper;
        private readonly IFileStorageService fileStorageService;
        private readonly string containerName = "actors";

        public ActorController(ApplicationDbContext db, IMapper mapper, IFileStorageService fileStorageService)
        {
            this.db = db;
            this.mapper = mapper;
            this.fileStorageService = fileStorageService;
        }
        
        [HttpGet]
        public async Task<ActionResult<List<ActorDTO>>> Get([FromQuery] PaginationDTO paginationDTO)
        {
            var queryable = db.Actors.AsQueryable();
            await HttpContext.InsertParametersPaginationInHeader(queryable);
            var actors = await queryable.OrderBy(x=>x.Name).Paginate(paginationDTO).ToListAsync();
            return mapper.Map<List<ActorDTO>>(actors);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ActorDTO>> Get(int id)
        {
            var actor = await  db.Actors.FirstOrDefaultAsync(x => x.Id == id);
            if (actor == null)
            {
                return NotFound();
            }

            return mapper.Map<ActorDTO>(actor);
        }

        [HttpGet("searchByName/{query}")]
        public async Task<ActionResult<List<ActorsMovieDTO>>> SearchByName(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) { return new List<ActorsMovieDTO>(); }  //if query is empty return an empty list, don't search

            return await db.Actors.Where(x => x.Name.Contains(query))
                .OrderBy(x => x.Name)
                .Select( x => new ActorsMovieDTO { Id = x.Id, Name = x.Name, Picture = x.Picture })
                .Take(5)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<ActorCreationDTO>> Post([FromForm] ActorCreationDTO actorCreationDTO)
        {
            var actor = mapper.Map<Actor>(actorCreationDTO);
            if(actorCreationDTO.Picture != null)
            {
                actor.Picture = await fileStorageService.SaveFile(containerName, actorCreationDTO.Picture);
            }

            db.Add(actor);
            await db.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id:int}")]
        //We are using FromForm so that we can send a form bcos we need to send our picture file since 
        public async Task<ActionResult<ActorCreationDTO>> Put(int id, [FromForm] ActorCreationDTO actorCreationDTO)
        {
            var actor = await db.Actors.FirstOrDefaultAsync(x => x.Id==id);
            if (actor == null)
            {
                return NotFound();
            }

            actor = mapper.Map(actorCreationDTO, actor);
            if(actorCreationDTO.Picture != null)
            {
                actor.Picture = await fileStorageService.EditFile(containerName, actorCreationDTO.Picture, actor.Picture);
            }

            await db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var actor = await db.Actors.FirstOrDefaultAsync(x => x.Id == id);
            if (actor == null)
            {
                return NotFound();
            }

            db.Remove(actor);
            await db.SaveChangesAsync();
            if (actor.Picture != null)
            {
                await fileStorageService.DeleteFile(actor.Picture, containerName);
            }
            return NoContent();
        }
    }
}
