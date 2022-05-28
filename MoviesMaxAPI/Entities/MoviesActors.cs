using System.ComponentModel.DataAnnotations;

namespace MoviesMaxAPI.Entities
{
    public class MoviesActors
    {
        public int ActorId { get; set; }
        public int MovieId { get; set; }
        [StringLength(maximumLength: 75)]
        public string Character { get; set; }
        public int Order { get; set; }      //we'd use to define d order in which d actors should appear in d movie details screen
        public Actor Actor { get; set; }
        public Movie Movie { get; set; }
    }
}
