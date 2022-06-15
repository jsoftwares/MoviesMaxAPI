using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MoviesMaxAPI.Entities;
using System.Diagnostics.CodeAnalysis;

namespace MoviesMaxAPI
{
    /**We will configure Identity core; & to start we make ApplicationDbContext class inherit from IdentityDbContext instead of DbContext
     * press Ctl+. to find Install package Microsoft.AspNetCore.Identity.EntityFrameworkCore - Find & install latest version. It is 
     * important to have base.OnModelCreating(modelBuilder); in OnModelCreating() method becos d IdentityDbContext is expecting to be
     * called through OnModelCreating().
     * The we add a migration for IdentityCore by tunning CMD (Add-Migration IdentityTables) then update-database
     * Next we need to configure some services in PROGRAM.cs for Identity to work correctly**/
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext([NotNullAttribute] DbContextOptions options) : base(options)
        {
        }

        //Configure composite PKs; eg actors belongs to a movie, so we configure d composite PK here by overriding the
        //OnModelCreating() method. HasKey() allows us configure the PK of an entity. By { x.ActorId, x.MovieId } we are saying d
        //MoviesActors entity will have a PK that is compossed of ActorId & MovieId columns. After this we add migration & update DB
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MoviesActors>().HasKey(x => new { x.ActorId, x.MovieId });
            modelBuilder.Entity< MoviesGenres>().HasKey( x => new {x.GenreId, x.MovieId });
            modelBuilder.Entity< MovieTheatresMovies>().HasKey(x => new {x.MovieTheatreId, x.MovieId});

            base.OnModelCreating(modelBuilder); // needed when building authentication system
        }

        public DbSet<Genre> Genres { get; set; }
        public DbSet<Actor> Actors { get; set; }
        public DbSet<MovieTheatre> MovieTheatres { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<MoviesActors> MoviesActors { get; set; }
        public DbSet<MoviesGenres> MoviesGenres { get; set; }
        public DbSet<MovieTheatresMovies> MovieTheatresMovies { get; set; }
        public DbSet<Rating> Ratings { get; set; }

    }
}
