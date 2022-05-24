using MoviesMaxAPI.Validations;
using System.ComponentModel.DataAnnotations;

namespace MoviesMaxAPI.DTOs
{
    public class GenreCreationDTO
    {
        [Required(ErrorMessage = "The field {0} iis required")]
        [StringLength(50)]
        [FirstLetterUppercase]
        public string Name { get; set; }
    }
}
