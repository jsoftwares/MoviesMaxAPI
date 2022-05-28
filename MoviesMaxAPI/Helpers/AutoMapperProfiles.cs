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

            CreateMap<MovieCreationDTO, Movie>()
                .ForMember(x => x.Poster, options => options.Ignore())
                .ForMember(x => x.MovieGenres, options => options.MapFrom(MapMoviesGenres))
                .ForMember(x => x.MovieTheatresMovie, options => options.MapFrom(MapMovieTheatresMovies))
                .ForMember(x => x.MovieActors, options => options.MapFrom(MapMoviesActors));
        }

        private List<MoviesGenres> MapMoviesGenres(MovieCreationDTO movieCreationDTO, Movie movie)
        {
            var result = new List<MoviesGenres>();

            if (movieCreationDTO.GenresIds == null) { return result; }

            foreach (var id in movieCreationDTO.GenresIds)
            {
                result.Add(new MoviesGenres() { GenreId = id });
            }
            return result;
        }

        private List<MovieTheatresMovies> MapMovieTheatresMovies(MovieCreationDTO movieCreationDTO, Movie movie)
        {
            var result = new List<MovieTheatresMovies>();

            if (movieCreationDTO.MovieTheatresId == null) { return result;  }

            foreach (var id in movieCreationDTO.MovieTheatresId)
            {
                result.Add(new MovieTheatresMovies() { MovieTheatreId = id });
            }
            return result;
        }

        private List<MoviesActors> MapMoviesActors(MovieCreationDTO movieCreationDTO, Movie movie)
        {
            var result = new List<MoviesActors>();

            if (movieCreationDTO.Actors == null) { return result; }

            foreach (var actor in movieCreationDTO.Actors)
            {
                result.Add(new MoviesActors() { ActorId = actor.Id, Character = actor.Character });
            }
            return result;
        }
    }
}
