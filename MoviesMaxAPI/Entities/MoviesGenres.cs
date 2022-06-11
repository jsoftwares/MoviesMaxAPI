namespace MoviesMaxAPI.Entities
{
    public class MoviesGenres
    {
        //Genre & Movie allows of naviage from MoviesGenres into the related entites. You can also skip this bcos from EF Core 5,
        //you can use the entities Genre & Movie directly without needing an intermediate class.
        //We need to configure d PKs on d Entities(MoviesGenres, MoviesActors, & MovieTheatreMovies) the facilitates d many-many
        //relationships; here GenreId & MovieId would define d composite PK & so we have to configure that in d ApplicationDbContext
        public int GenreId { get; set; }
        public int MovieId { get; set; }
        public Genre Genre { get; set; }
        public Movie Movie { get; set; }
    }
}
