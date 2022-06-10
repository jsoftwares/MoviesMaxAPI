namespace MoviesMaxAPI.DTOs
{
    public class MoviePutGetDTO
    {
        public MovieDTO Movie { get; set; }
        public List<GenreDTO> SelectedGenres { get; set; }
        public List<GenreDTO> NonSelectedGenres { get; set; }
        public List<MovieTheatreDTO> SelectedMovieTheatres { get; set; }
        public List<MovieTheatreDTO> NonSelectedMovieTheatres { get; set; }
        public List<ActorsMovieDTO> Actots { get; set; }
    }
    
}
