using System.ComponentModel;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using TimeIrCrawler.Interfaces;
using TimeIrCrawler.Models;

namespace TimeIrCrawler.Tools;

[McpServerToolType]
public sealed class TimeIrCrawlerTool
{
    private readonly ITimeIrCrawler _crawler;
    private readonly ILogger<TimeIrCrawlerTool> _logger;

    public TimeIrCrawlerTool(ITimeIrCrawler crawler, ILogger<TimeIrCrawlerTool> logger)
    {
        _crawler = crawler;
        _logger = logger;
        _logger.LogInformation("TimeIrCrawlerTool initialized");
    }

    [McpServerTool, Description("Gets the current time.")]
    public async Task<TimeDataResponse> GetCurrentTimeData()
    {
        try
        {
            _logger.LogInformation("MCP Tool: GetCurrentTimeData called");
            var timeData = await _crawler.GetCurrentTimeDataAsync();
            _logger.LogInformation("MCP Tool: GetCurrentTimeData completed successfully");
            
            return new TimeDataResponse
            {
                CurrentTime = timeData.CurrentTime,
                CurrentDate = timeData.CurrentDate,
                HijriDate = timeData.HijriDate,
                GregorianDate = timeData.GregorianDate
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetCurrentTimeData: {Message}", ex.Message);
            return new TimeDataResponse
            {
                CurrentTime = DateTime.Now.ToString("HH:mm:ss"),
                CurrentDate = "Error fetching date",
                HijriDate = "Error fetching date",
                GregorianDate = DateTime.Now.ToString("yyyy/MM/dd")
            };
        }
    }

    [McpServerTool, Description("Gets the events of month.")]
    public async Task<List<EventDataResponse>> GetEventsDataAsync()
    {
        var eventsData = await _crawler.GetEventsDataAsync();
        
        return eventsData.Select(e => new EventDataResponse
        {
            Date = e.Date,
            Title = e.Title,
            Extra = e.Extra
        }).ToList();
    }
}