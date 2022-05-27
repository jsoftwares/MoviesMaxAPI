﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesMaxAPI.DTOs;

namespace MoviesMaxAPI.Controllers
{
    [Route("api/actors")]
    [ApiController]
    public class ActorController : ControllerBase
    {
        private readonly ApplicationDbContext db;
        private readonly IMapper mapper;

        public ActorController(ApplicationDbContext db, IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }
        
        [HttpGet]
        public async Task<ActionResult<List<ActorDTO>>> Get()
        {
            var actors = await db.Actors.ToListAsync();
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

        [HttpPost]
        public async Task<ActionResult<ActorCreationDTO>> Post([FromForm] ActorCreationDTO actorCreationDTO)
        {
            return NoContent();
            throw new NotImplementedException();
        }

        [HttpPut("{id:int}")]
        //We are using FromForm so that we can send a form bcos we need to send our picture file since 
        public async Task<ActionResult<ActorCreationDTO>> Put(int id, [FromForm] ActorCreationDTO actorCreationDTO)
        {
            throw new NotImplementedException();
        }

        [HttpDelete]
        public async Task<ActionResult> Delete(int id)
        {
            var actor = await db.Actors.FirstOrDefaultAsync(x => x.Id == id);
            if (actor == null)
            {
                return NotFound();
            }

            db.Remove(actor);
            await db.SaveChangesAsync();
            return NoContent();
        }
    }
}
