using FluentMigrator.Runner;
using FluentMigrator.Runner.Logging;
using JK.Orleans.Migration.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                  ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                  ?? "Production";

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

var connectionString = configuration.GetConnectionString("Orleans") 
                       ?? configuration["ConnectionStrings__Orleans"] 
                       ?? configuration["ORLEANS_CONNECTION_STRING"];

if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("Connection string 'Orleans' not found in configuration.");
    return;
}

var serviceProvider = CreateServices(connectionString);

using (var scope = serviceProvider.CreateScope())
{
    Console.WriteLine($"[DEBUG] Using Connection String: {connectionString}");
    UpdateDatabase(scope.ServiceProvider);
}

static IServiceProvider CreateServices(string connectionString)
{
    return new ServiceCollection()
        .AddFluentMigratorCore()
        .ConfigureRunner(rb => rb
            .AddSqlServer()
            .WithGlobalConnectionString(connectionString)
            .ScanIn(typeof(_0001_CreateOrleansMain).Assembly).For.All())
        .AddLogging(lb => lb
            .AddFluentMigratorConsole()
            .AddConsole())
        .Configure<FluentMigratorLoggerOptions>(opt =>
        {
            opt.ShowSql = true;
            opt.ShowElapsedTime = true;
        })
        .BuildServiceProvider(false);
}

static void UpdateDatabase(IServiceProvider serviceProvider)
{
    try
    {
        Console.WriteLine("[DEBUG] Starting MigrateUp...");
        var runner = serviceProvider.GetRequiredService<IMigrationRunner>();
        
        if (runner.HasMigrationsToApplyUp())
        {
            Console.WriteLine("[DEBUG] Found migrations to apply.");
        }
        else
        {
            Console.WriteLine("[DEBUG] No migrations to apply.");
        }

        runner.MigrateUp();
        Console.WriteLine("[DEBUG] MigrateUp finished successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] Migration failed: {ex.Message}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"[ERROR] Inner Exception: {ex.InnerException.Message}");
            if (ex.InnerException.InnerException != null)
            {
                Console.WriteLine($"[ERROR] Inner Inner Exception: {ex.InnerException.InnerException.Message}");
            }
        }
        Console.WriteLine(ex.StackTrace);
        throw;
    }
}
