namespace MoviesMaxAPI.Entities
{
    public class MovieTheatresMovies
    {
        public int MovieTheatreId { get; set; }
        public int MovieId { get; set; }
        public MovieTheatre MovieTheatre { get; set; }
        public Movie Movie { get; set; }
    }
}
