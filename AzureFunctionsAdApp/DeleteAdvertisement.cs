using AzureAdApp.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace AzureFunctionsAdApp
{
    public class DeleteAdvertisement
    {
        private readonly ApplicationDbContext _dbContext;

        public DeleteAdvertisement(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Function("DeleteAdvertisement")]
        public async Task<IActionResult> Run(
    [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "advertisements/{id}")] HttpRequest req,
    int id,
    ILogger log)
        {
            try
            {
                var userId = req.Headers["X-User-Id"].FirstOrDefault();
                if (string.IsNullOrEmpty(userId))
                    return new UnauthorizedResult();

                var advertisement = await _dbContext.Advertisements
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (advertisement == null)
                    return new NotFoundResult();

                // Verify the user owns this advertisement
                if (advertisement.UserId != userId)
                    return new UnauthorizedResult();

                // Delete associated messages
                var messages = await _dbContext.Messages
                    .Where(m => m.AdvertisementId == id)
                    .ToListAsync();

                _dbContext.Messages.RemoveRange(messages);
                _dbContext.Advertisements.Remove(advertisement);
                await _dbContext.SaveChangesAsync();

                return new OkResult();
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error deleting advertisement");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

    }


}
