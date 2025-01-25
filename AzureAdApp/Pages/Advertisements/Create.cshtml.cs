using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using AzureAdApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AzureAdApp.Pages.Advertisements
{
    public class CreateModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly SecretClient _secretClient;

        public CreateModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
            var keyVaultUrl = _configuration["VaultUti"];
            _secretClient = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [StringLength(50, ErrorMessage = "The title must be at most 10 characters long.")]
            public string Title { get; set; }

            [Required]
            [StringLength(500, ErrorMessage = "The description must be at most 100 characters long.")]
            public string Description { get; set; }
        }


        public void OnGet()
        {
            // Initialize any default values if needed
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var authCookie = Request.Cookies[".AspNetCore.Identity.Application"];

            // Map input to Advertisement model
            var advertisement = new Advertisement
            {
                Title = Input.Title,
                Description = Input.Description,
                CreatedDate = DateTime.UtcNow,
                UserId = userId
            };

            var functionUrlSecret = await _secretClient.GetSecretAsync("AzureFunctionUrl");
            var functionBaseUrl = functionUrlSecret.Value.Value;
            var request = new HttpRequestMessage(HttpMethod.Post, $"{functionBaseUrl}/api/advertisements");

            //var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:7181/api/advertisements");
            //var request = new HttpRequestMessage(HttpMethod.Post, "https://azurefunctionsadapp20250125134356.azurewebsites.net/api/advertisements");
            request.Headers.Add("Cookie", $".AspNetCore.Identity.Application={authCookie}");
            
            var jsonContent = JsonSerializer.Serialize(advertisement);

            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
                return RedirectToPage("./Index");

            ModelState.AddModelError(string.Empty, "An error occurred while creating the advertisement.");
            return Page();
        }
    }
}
