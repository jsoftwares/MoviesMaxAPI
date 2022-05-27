using AutoMapper;
using MoviesMaxAPI.DTOs;
using MoviesMaxAPI.Entities;
using NetTopologySuite.Geometries;

namespace MoviesMaxAPI.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles(GeometryFactory geometryFactory)
        {
            CreateMap<GenreDTO, Genre>().ReverseMap();  //ReverseMap() helps us have a mapping from Genre to GenreDTO also as we will need that in this app.
            CreateMap<GenreCreationDTO, Genre>();

            /**We are ignoring mapping for Picture during Actor creation bcos during creation, we will be receiving an IForm
             * file and in DB we store string hence there is no reason to map from IForm file to a string **/
            CreateMap<ActorDTO, Actor>().ReverseMap();
            CreateMap<ActorCreationDTO, Actor>()
                .ForMember(x=> x.Picture, options=> options.Ignore());
            CreateMap<MovieTheatre, MovieTheatreDTO>()
                .ForMember(x => x.Latitude, dto => dto.MapFrom(prop => prop.Location.Y))
                .ForMember(x=> x.Longitude, dto => dto.MapFrom(prop => prop.Location.X));
            CreateMap<MovieTheatreCreationDTO, MovieTheatre>()
                .ForMember(x => x.Location, x => x.MapFrom(dto => 
                geometryFactory.CreatePoint(new Coordinate(dto.Longitude, dto.Latitude))));
        }
    }
}
