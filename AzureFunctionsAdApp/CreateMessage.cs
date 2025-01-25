using AzureAdApp.Data;
using AzureAdApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

public class CreateMessage
{
    private readonly ApplicationDbContext _dbContext;

    public CreateMessage(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [Function("CreateMessage")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "postmessage")] HttpRequest req,
        ILogger log)
    {
        try
        {
            var authCookie = req.Cookies[".AspNetCore.Identity.Application"];
            if (string.IsNullOrEmpty(authCookie))
                return new UnauthorizedResult();

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var message = JsonSerializer.Deserialize<Message>(requestBody);

            if (message == null)
                return new BadRequestObjectResult("Invalid message data");

            message.CreatedDate = DateTime.UtcNow;
            _dbContext.Messages.Add(message);
            await _dbContext.SaveChangesAsync();

            return new OkObjectResult(new { id = message.Id, message = "Message sent successfully." });
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Error creating message");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
