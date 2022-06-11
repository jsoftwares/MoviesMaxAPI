namespace MoviesMaxAPI.DTOs
{
    public class LandingPageDTO
    {
        public List<MovieDTO> InTheatres { get; set; }
        public List<MovieDTO> UpcomingReleases { get; set; }
    }
}
