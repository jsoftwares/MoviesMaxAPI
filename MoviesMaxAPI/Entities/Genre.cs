using MoviesMaxAPI.Validations;
using System.ComponentModel.DataAnnotations;

namespace MoviesMaxAPI.Entities
{
    public class Genre
    {
        public int Id { get; set; }
        [Required(ErrorMessage ="The field {0} is required")]
        [StringLength(50)]
        [FirstLetterUppercase]
        public string Name { get; set; }
    }
}
