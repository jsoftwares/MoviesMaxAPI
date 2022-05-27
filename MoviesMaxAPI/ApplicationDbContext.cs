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

        public DbSet<Genre> Genres { get; set; }
        public DbSet<Actor> Actors { get; set; }
        public DbSet<MovieTheatre> MovieTheatres { get; set; }
    }
}
