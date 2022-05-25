using Microsoft.EntityFrameworkCore;

namespace MoviesMaxAPI.Helpers
{
    public static class HttpContextExtensions
    {
        /**
         * <T> method is a generic method. We are extending HttpContext. With this we are putting in response header of the 
         * Http request, a field that holdsthe total number of records from a DB table, this way, the client would be able to access
         * this information
         * We also need to go to PROGRAM.cs to configure CORS by adding .WithExposedHeaders() so that it allows us read
         * "totalAmountOfRecords" header from a web browser based client
         * **/
        public async static Task InsertParametersPaginationInHeader<T>(this HttpContext httpContext, IQueryable<T> queryable)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }
            double count = await queryable.CountAsync();
            httpContext.Response.Headers.Add("totalAmountOfRecords", count.ToString());
        }
    }
}
