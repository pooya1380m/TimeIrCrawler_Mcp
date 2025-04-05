using TimeIrCrawler.Models;

namespace TimeIrCrawler.Interfaces;

public interface ITimeIrCrawler
{
    Task<TimeData> GetCurrentTimeDataAsync();
    Task<List<EventData>> GetEventsDataAsync();
} 