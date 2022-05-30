namespace MoviesMaxAPI.DTOs
{
    public class MoviePostGetDTO
    {
        public List<GenreDTO> Genres { get; set; }
        public List<MovieTheatreDTO> MovieTheatres { get; set; }
    }
}
