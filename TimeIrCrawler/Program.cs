using TimeIrCrawler.Interfaces;
using TimeIrCrawler.Services;
using TimeIrCrawler.Tools;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

// Clear all logging to prevent console output interference with MCP stdio
builder.Logging.ClearProviders();
// Optional: Add file logging for debugging instead of console
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Register services
builder.Services.AddScoped<ITimeIrCrawler, TimeIrCrawlerService>();

// Add MCP Server with stdio transport
builder.Services.AddMcpServer(options =>
    {
        options.ServerInfo = new()
        {
            Name = "TimeIrServer",
            Version = "1.0.0"
        };
        options.ServerInstructions = "This server provides tools to get current time data from Time.ir website";
    }).WithStdioServerTransport()
    .WithTools<TimeIrCrawlerTool>();

var host = builder.Build();

// Run the application as an MCP server
try
{
    var logger = host.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Starting MCP server...");
    await host.RunAsync();
    logger.LogInformation("MCP server stopped.");
}
catch (Exception ex)
{
    var logger = host.Services.GetService<ILogger<Program>>();
    logger?.LogError(ex, "MCP server failed to start");
    throw; // Let the exception bubble up for visibility
}