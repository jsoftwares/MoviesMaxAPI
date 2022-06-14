namespace MoviesMaxAPI.DTOs
{
    public class FilterMoviesDTO
    {
        public int Page { get; set; }
        public int RecordsPerPage { get; set; }
        public PaginationDTO PaginationDTO 
        {
            //this is just a helper method to quickly construct a PaginationDTO from d parameters of this FilterMoviesDTO class
            get { return new PaginationDTO() { Page = Page, RecordsPerPage = RecordsPerPage }; }
        }
        public string Title { get; set; }
        public int GenreId { get; set; }
        public bool InTheatres { get; set; }
        public bool UpcomingReleases { get; set; }
    }
}
