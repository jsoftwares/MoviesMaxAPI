using Microsoft.AspNetCore.Mvc;
using MoviesMaxAPI.Helpers;

namespace MoviesMaxAPI.DTOs
{
    /**Bcos we will also recieve d list of Genres, Theatres & Actors we want to associate with each Movie during creation/edit,
     * and since we are using FROMFORM in Post & Put actions, we will need to help the model binder to bind the lists that will
     * be received as payload. For this we will create and use a custom model binder HELPERS/TypeBinder.cs**/
    public class MovieCreationDTO
    {
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Trailer { get; set; }
        public bool InTheatres { get; set; }
        public DateTime ReleaseDate { get; set; }
        public IFormFile Poster { get; set; }

        [ModelBinder(BinderType = typeof(TypeBinder<List<int>>))]
        public List<int> GenresIds { get; set;}

        [ModelBinder(BinderType = typeof(TypeBinder<List<int>>))]
        public List<int> MovieTheatresIds { get; set; }

        //For actors, we have to create/use a DTO bcos d info of an Actor that's related to a movie will also have d Character they played in that movie 
        [ModelBinder(BinderType = typeof(TypeBinder<List<MoviesActorsCreationDTO>>))]
        public List<MoviesActorsCreationDTO> Actors { get; set; }
    }
}
