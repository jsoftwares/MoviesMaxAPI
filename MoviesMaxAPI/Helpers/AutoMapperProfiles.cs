using AutoMapper;
using MoviesMaxAPI.DTOs;
using MoviesMaxAPI.Entities;

namespace MoviesMaxAPI.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<GenreDTO, Genre>().ReverseMap();  //ReverseMap() helps us have a mapping from Genre to GenreDTO also as we will need that in this app.
            CreateMap<GenreCreationDTO, Genre>();
        }
    }
}
