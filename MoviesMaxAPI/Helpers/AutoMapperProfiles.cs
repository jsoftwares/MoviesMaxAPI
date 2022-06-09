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
                .ForMember(x => x.MoviesGenres, options => options.MapFrom(MapMoviesGenres))
                .ForMember(x => x.MovieTheatresMovies, options => options.MapFrom(MapMovieTheatresMovies))
                .ForMember(x => x.MoviesActors, options => options.MapFrom(MapMoviesActors));

            CreateMap<Movie, MovieDTO>()
                .ForMember(x => x.Genres, options => options.MapFrom(MapMoviesGenres))
                .ForMember(x => x.MovieTheatres, options => options.MapFrom(MapMovieTheatresMovies))
                .ForMember(x => x.Actors, options => options.MapFrom(MapMoviesActors));
        }

        private List<GenreDTO> MapMoviesGenres(Movie movie, MovieDTO movieDTO)
        {
            var result = new List<GenreDTO>();
            if (movie.MoviesGenres != null)
            {
                //foreach row in this movie MoviesGenres intermediate table, we create & add a new GenreDTO to result
                foreach (var genre in movie.MoviesGenres)
                {
                    //genre.Genre.Name is why we had to use ThenInclude(x=>x.Genre) while getting a single movie bcos only by doing
                    //so we are able to have access to the Name property of the Genre here. Same thing for MovieTheatres & Actors
                    result.Add(new GenreDTO() { Id = genre.GenreId, Name = genre.Genre.Name });
                }
            }
            return result;
        }
        private List<MovieTheatreDTO> MapMovieTheatresMovies(Movie movie, MovieDTO movieDTO)
        {
            var result = new List<MovieTheatreDTO>();

            if (movie.MovieTheatresMovies != null)
            {
                foreach (var movieTheatreMovies in movie.MovieTheatresMovies)
                {
                    result.Add(new MovieTheatreDTO() { Id = movieTheatreMovies.MovieTheatreId, 
                        Name = movieTheatreMovies.MovieTheatre.Name,
                        Latitude = movieTheatreMovies.MovieTheatre.Location.Y,
                        Longitude = movieTheatreMovies.MovieTheatre.Location.X
                    });
                }
            }

            return result;
        }
        private List<ActorsMovieDTO> MapMoviesActors(Movie movie, MovieDTO movieDTO)
        {
            var result = new List<ActorsMovieDTO>();
            if (movie.MoviesActors != null)
            {
                foreach (var moviesActors in movie.MoviesActors)
                {
                    result.Add(new ActorsMovieDTO()
                    {
                        Id = moviesActors.ActorId,
                        Name = moviesActors.Actor.Name,
                        Character = moviesActors.Character,
                        Order = moviesActors.Order,
                    });
                }
            }

            return result;
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

            if (movieCreationDTO.MovieTheatresIds == null) { return result;  }

            foreach (var id in movieCreationDTO.MovieTheatresIds)
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
