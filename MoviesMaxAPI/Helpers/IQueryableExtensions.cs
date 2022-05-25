using MoviesMaxAPI.DTOs;

namespace MoviesMaxAPI.Helpers
{
    public static class IQueryableExtensions
    {
        /** Skip() allows us skip x-amount of records in our DB & Take() allows of then return x-amount of records from DB. **/
        public static IQueryable<T> Paginate<T>(this IQueryable<T> queryable, PaginationDTO paginationDTO)
        {
            return queryable
                .Skip((paginationDTO.Page - 1) * paginationDTO.RecordsPerPage)
                .Take(paginationDTO.RecordsPerPage);
        }
    }
}
