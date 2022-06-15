using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesMaxAPI.DTOs;
using MoviesMaxAPI.Entities;

namespace MoviesMaxAPI.Controllers
{
    [ApiController]
    [Route("api/ratings")]
    public class RatingController: ControllerBase
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<IdentityUser> userManager;

        public RatingController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            this.db = db;
            this.userManager = userManager;
        }
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Post([FromBody] RatingDTO ratingDTO)
        {
            //REM: during register or login in AccountController we are creating $ adding a new "email" claim for d user, hence we are
            //retrieving that from d request (HttpContext) here, since this us an authenticated route. We are want d email so that we can
            //use that to get the user ID

            var email = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "email").Value;
            var user = await userManager.FindByEmailAsync(email);
            var userId = user.Id;

            var currentRating = await db.Ratings.FirstOrDefaultAsync(x => x.MovieId == ratingDTO.MovieId && x.UserId == userId);

            if (currentRating == null)
            {
                var rating = new Rating();
                rating.MovieId = ratingDTO.MovieId;
                rating.UserId = userId;
                rating.Rate = ratingDTO.Rating;
                db.Add(rating);
            }
            else
            {
                currentRating.Rate = ratingDTO.Rating;
            }

            await db.SaveChangesAsync();
            return NoContent();
        }
    }
}
