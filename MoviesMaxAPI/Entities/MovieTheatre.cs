using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations;

namespace MoviesMaxAPI.Entities
{
    public class MovieTheatre
    {
        /**To store the location of our MovieTheatres, we will use a library Microsoft.EntityFrameworkCore.SqlServer.NetTopologySuite
         * that allows us to work with things like location, distances. It gives us a data type called Point that we can use
         * To use it we need to configure EntityFrameworkCore in Program.cs**/
        public int Id { get; set; }
        [Required]
        [StringLength(75)]
        public string Name { get; set; }
        public Point Location { get; set; }

    }
}
