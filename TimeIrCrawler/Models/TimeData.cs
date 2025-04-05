namespace TimeIrCrawler.Models;

public class TimeData
{
    public string CurrentTime { get; set; } = string.Empty;
    public string CurrentDate { get; set; } = string.Empty;
    public string HijriDate { get; set; } = string.Empty;
    public string GregorianDate { get; set; } = string.Empty;
    
    public List<string> Events { get; set; } = new();
} 