using Microsoft.EntityFrameworkCore;
using MoviesMaxAPI.Entities;
using System.Diagnostics.CodeAnalysis;

namespace MoviesMaxAPI
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext([NotNullAttribute] DbContextOptions options) : base(options)
        {
        }

        //Configure composite PKs. HasKey() allows us configure the PK of an entity
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

    }
}
