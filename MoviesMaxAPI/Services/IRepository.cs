using MoviesMaxAPI.Entities;

namespace MoviesMaxAPI.Services
{
    public interface IRepository
    {
        void AddGenre(Genre genre);
        List<Genre> GetAllGenres();
        Genre GetGenreById(int Id);
    }
}
