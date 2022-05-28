using System.ComponentModel.DataAnnotations;

namespace MoviesMaxAPI.Entities
{

    /**The Movie entity has relationship with the Genre, Movie Theatre & Actor entities and we will model these relationships
     * using Intermediate classes.
     * Though we can skip using intermediate class bcos in from Entity Framework Core 5 you can use related Entities/models without
     * needing an intermediate class, but it is a style I like to use, but you don't have to use it if you don't want to
     * We also configure the PKs of the entities that model the many-to-many relationship; we have to do this bcos we have for each
     * two columns that will define the composite PK, as so we have to configure that in the ApplicationDbContext**/
    public class Movie
    {
        public int Id { get; set; }
        [StringLength(maximumLength: 75)]
        [Required(ErrorMessage = "The field {0} is required")]
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Trailer { get; set; }
        public bool InTheatres { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string Poster { get; set; }
        public List<MoviesGenres> MovieGenres { get; set; }     //modeling many-to-maany relationship btw Movie entity & Genre entity
        public List<MovieTheatresMovies> MovieTheatresMovie { get; set; }
    }
}
