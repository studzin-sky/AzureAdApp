using AzureAdApp.Data;
using AzureAdApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class GetAdvertisementMessages
{
    private readonly ApplicationDbContext _dbContext;

    public GetAdvertisementMessages(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [Function("GetAdvertisementMessages")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "advertisements/{id}/messages")] HttpRequest req,
        int id,
        ILogger log)
    {
        try
        {
            var messages = await _dbContext.Messages
                .Where(m => m.AdvertisementId == id)
                .OrderByDescending(m => m.CreatedDate)
                .ToListAsync();

            var advertisement = await _dbContext.Advertisements
                .FirstOrDefaultAsync(a => a.Id == id);

            if (advertisement == null)
                return new NotFoundResult();

            return new OkObjectResult(new
            {
                Advertisement = advertisement,
                Messages = messages ?? new List<Message>()
            });
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Error fetching advertisement messages");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
