using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using AzureAdApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace AzureAdApp.Pages.Advertisements
{
    public class DetailsModel : PageModel
    {
        private readonly HttpClient _client;
        private readonly IConfiguration _configuration;
        private readonly SecretClient _secretClient;

        public Advertisement Advertisement { get; set; }
        public List<Message> Messages { get; set; }

        public DetailsModel(HttpClient client, IConfiguration configuration)
        {
            _client = client;
            _configuration = configuration;
            var keyVaultUrl = _configuration["VaultUri"];
            _secretClient = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var functionUrlSecret = await _secretClient.GetSecretAsync("AzureFunctionUrl");
            var functionBaseUrl = functionUrlSecret.Value.Value;
            var functionUrl = $"{functionBaseUrl}/api/advertisements/{id}/messages";
            //var functionUrl = $"http://localhost:7181/api/advertisements/{id}/messages";
            var response = await _client.GetAsync(functionUrl);

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var result = JsonSerializer.Deserialize<AdvertisementDetailsResponse>(jsonString, options);
                Advertisement = result.Advertisement;
                Messages = result.Messages ?? new List<Message>();
                return Page();
            }

            return NotFound();
        }

        [BindProperty]
        public MessageInputModel MessageInput { get; set; }

        public class MessageInputModel
        {
            public int AdvertisementId { get; set; }
            public string ToUserId { get; set; }

            [Required]
            [StringLength(500, ErrorMessage = "Message cannot exceed 500 characters.")]
            public string Content { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync(MessageInput.AdvertisementId);
                return Page();
            }

            var message = new Message
            {
                AdvertisementId = MessageInput.AdvertisementId,
                FromUserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                ToUserId = MessageInput.ToUserId,
                Content = MessageInput.Content,
                CreatedDate = DateTime.UtcNow
            };

            var functionUrlSecret = await _secretClient.GetSecretAsync("AzureFunctionUrl");
            var functionBaseUrl = functionUrlSecret.Value.Value;
            var functionUrl = $"{functionBaseUrl}/api/postmessage";
            //var functionUrl = "http://localhost:7181/api/postmessage";

            var authCookie = Request.Cookies[".AspNetCore.Identity.Application"];
            var request = new HttpRequestMessage(HttpMethod.Post, functionUrl);
            request.Headers.Add("Cookie", $".AspNetCore.Identity.Application={authCookie}");

            var jsonContent = JsonSerializer.Serialize(message);
            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
                return RedirectToPage(new { id = MessageInput.AdvertisementId });

            ModelState.AddModelError(string.Empty, "Failed to send message.");
            await OnGetAsync(MessageInput.AdvertisementId);
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAdvertisementAsync(int advertisementId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var functionUrlSecret = await _secretClient.GetSecretAsync("AzureFunctionUrl");
            var functionBaseUrl = functionUrlSecret.Value.Value;
            var functionUrl = $"{functionBaseUrl}/api/advertisements/{advertisementId}";

            var authCookie = Request.Cookies[".AspNetCore.Identity.Application"];
            var request = new HttpRequestMessage(HttpMethod.Delete, functionUrl);
            request.Headers.Add("Cookie", $".AspNetCore.Identity.Application={authCookie}");
            // Add the user ID to the request headers
            request.Headers.Add("X-User-Id", userId);

            var response = await _client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Failed to delete advertisement.";
                return RedirectToPage(new { id = advertisementId });
            }

            return RedirectToPage("./Index");
        }

    }

    public class AdvertisementDetailsResponse
    {
        public Advertisement Advertisement { get; set; }
        public List<Message> Messages { get; set; }
    }

}
