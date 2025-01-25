# Azure Advertisement Application

This application is a web-based platform built using ASP.NET Core Razor Pages and Azure Functions, designed for posting and managing advertisements with user authentication.

## Architecture Components

**Web Application (Razor Pages)**
- ASP.NET Core Razor Pages
- Azure Key Vault integration
- Entity Framework Core with SQL Server
- ASP.NET Core Identity for authentication
- HTTPS enforcement and security features

**Azure Functions**
- Serverless backend functionality
- Azure Key Vault integration
- Shared database context with main application

## Prerequisites

- .NET 7.0 or later
- Azure subscription
- Azure SQL Database
- Azure Key Vault
- Visual Studio 2022 or VS Code

## Configuration

**Azure Key Vault**
```json
{
  "VaultUri": "your-key-vault-url",
  "SqlConnectionString": "your-sql-connection-string"
}
```

**Identity Settings**
- Password Requirements:
  - Minimum length: 6 characters
  - Must contain uppercase letters
  - Must contain lowercase letters
  - Must contain numbers
  - Must contain special characters

## Authentication Flow

The application implements a secure authentication flow:
- Unauthenticated users are redirected to `/Account/Login`
- Authenticated users are redirected to `/Advertisements/Index`
- Access denied scenarios redirect to `/Account/AccessDenied`

## Security Features

- HTTPS enforcement
- Azure Managed Identity integration
- Secure credential management via Azure Key Vault
- Cookie-based authentication
- ASP.NET Core Identity security features

## Project Structure

**Main Web Application**
```plaintext
├── Program.cs              # Application entry point and configuration
├── Data/
│   └── ApplicationDbContext.cs
├── Pages/
│   ├── Account/           # Authentication pages
│   └── Advertisements/    # Advertisement management pages
```

**Azure Functions**
```plaintext
├── Program.cs              # Functions host configuration
└── Functions/             # Individual function implementations
```

## Local Development

1. Configure Azure credentials:
```bash
az login
```

2. Set up local user secrets:
```bash
dotnet user-secrets set "VaultUri" "your-key-vault-url"
```

3. Run the application:
```bash
dotnet run
```

## Deployment

**Prerequisites**
- Azure CLI installed
- Azure subscription configured
- Appropriate Azure roles assigned

**Required Azure Resources**
- Azure App Service
- Azure Functions
- Azure SQL Database
- Azure Key Vault
- Azure Managed Identity

## Environment Variables

The following environment variables must be configured:
- `VaultUri`: URL of your Azure Key Vault
- `SqlConnectionString`: Stored in Key Vault
- `AZURE_TENANT_ID`: Your Azure tenant ID
- `AZURE_CLIENT_ID`: Your application's client ID

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is licensed under the MIT License.
