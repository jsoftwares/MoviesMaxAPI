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

            /**We are ignoring mapping for Picture during Actor creation bcos during creation, we will be receiving an IForm
             * file and in DB we store string hence there is no reason to map from IForm file to a string **/
            CreateMap<ActorDTO, Actor>().ReverseMap();
            CreateMap<ActorCreationDTO, Actor>()
                .ForMember(x=> x.Picture, options=> options.Ignore());
        }
    }
}
