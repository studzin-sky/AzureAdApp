using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using AzureAdApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        var keyVaultUrl = context.Configuration["VaultUri"];
        var secretClient = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
        var sqlConnection = secretClient.GetSecret("SqlConnectionString").Value.Value;

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(sqlConnection));

        services.AddAuthentication()
            .AddCookie();
    })
    .Build();

host.Run();
