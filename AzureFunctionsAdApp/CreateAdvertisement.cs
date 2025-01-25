
using AzureAdApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Functions.Worker;
using AzureAdApp.Data;
using System.Security.Claims;

public class CreateAdvertisement
{

    private readonly ApplicationDbContext _dbContext;

    public CreateAdvertisement(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [Function("CreateAdvertisement")]
    public async Task<IActionResult> Run(
     [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "advertisements")] HttpRequest req,
     ILogger log)
    {
        try
        {

            var authCookie = req.Cookies[".AspNetCore.Identity.Application"];
            if (string.IsNullOrEmpty(authCookie))
                return new UnauthorizedResult();

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var ad = JsonConvert.DeserializeObject<Advertisement>(requestBody);

            if (ad == null)
                return new BadRequestObjectResult("Invalid advertisement data");


            _dbContext.Advertisements.Add(ad);
            await _dbContext.SaveChangesAsync();

            return new OkObjectResult(new { id = ad.Id, message = $"Advertisement '{ad.Title}' created successfully." });
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Error creating advertisement");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }

}
