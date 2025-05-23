using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using TimeIrCrawler.Interfaces;
using TimeIrCrawler.Models;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Runtime.InteropServices.JavaScript;

namespace TimeIrCrawler.Services;

public class TimeIrCrawlerService : ITimeIrCrawler, IDisposable
{
    private readonly IWebDriver? _driver;
    private readonly WebDriverWait? _wait;
    private readonly ILogger<TimeIrCrawlerService> _logger;
    private bool _initialized = false;

    public TimeIrCrawlerService(ILogger<TimeIrCrawlerService> logger)
    {
        _logger = logger;
        
        try
        {
            _logger.LogInformation("Initializing TimeIrCrawlerService with Chrome WebDriver");
            
            var options = new ChromeOptions();
            options.AddArgument("--headless");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--window-size=1920,1080");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            
            _driver = new ChromeDriver(options);
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            
            _logger.LogInformation("Navigating to time.ir");
            _driver.Navigate().GoToUrl("https://www.time.ir");
            
            // Wait for the page to load
            _wait.Until(d => d.FindElement(By.XPath("//div[contains(@class, 'todayDate')]")));
            _initialized = true;
            _logger.LogInformation("TimeIrCrawlerService initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing TimeIrCrawlerService: {Message}", ex.Message);
        }
    }

    public Task<TimeData> GetCurrentTimeDataAsync()
    {
        var timeData = new TimeData();
        
        try
        {
            if (!_initialized || _driver == null || _wait == null)
            {
                _logger.LogWarning("WebDriver not initialized. Using fallback time data.");
                return Task.FromResult(GetFallbackTimeData());
            }
            
            _logger.LogInformation("Getting current time data");

            // Get current time
            var timeElement = _wait.Until(d => d.FindElement(By.XPath("//div[@id='digitalClock']")));
            timeData.CurrentTime = timeElement.Text.Trim();
            _logger.LogDebug("Current time: {CurrentTime}", timeData.CurrentTime);

            // Get current date (Shamsi)
            var dateElement = _wait.Until(d => d.FindElement(By.XPath("//div[contains(@class, 'today-shamsi')]//span[contains(@class, 'date')]")));
            timeData.CurrentDate = dateElement.Text.Trim();
            _logger.LogDebug("Current date: {CurrentDate}", timeData.CurrentDate);

            // Get Hijri date
            var hijriElement = _wait.Until(d => d.FindElement(By.XPath("//div[contains(@class, 'today-hijri')]//span[contains(@class, 'date')]")));
            timeData.HijriDate = hijriElement.Text.Trim();
            _logger.LogDebug("Hijri date: {HijriDate}", timeData.HijriDate);

            // Get Gregorian date
            var gregorianElement = _wait.Until(d => d.FindElement(By.XPath("//div[contains(@class, 'today-gregorian')]//span[contains(@class, 'date')]")));
            timeData.GregorianDate = gregorianElement.Text.Trim();
            _logger.LogDebug("Gregorian date: {GregorianDate}", timeData.GregorianDate);
            
            _logger.LogInformation("Successfully retrieved time data");
            return Task.FromResult(timeData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching time data: {Message}", ex.Message);
            return Task.FromResult(GetFallbackTimeData());
        }
    }

    public Task<List<EventData>> GetEventsDataAsync()
    {
        try
        {
            if (!_initialized || _driver == null || _wait == null)
            {
                _logger.LogWarning("WebDriver not initialized. Returning empty events list.");
                return Task.FromResult(new List<EventData>());
            }
            
            _logger.LogInformation("Getting events data");
            
            // Get all <li> elements under the <ul> with class="list-unstyled"
            var listItems = _wait.Until(d => d.FindElements(By.XPath("//ul[contains(@class, 'list-unstyled')]/li")));
            
            _logger.LogDebug("Found {Count} list items", listItems.Count);
            
            var events = new List<EventData>();

            foreach (var li in listItems)
            {
                try
                {
                    // First span inside <li> is the date
                    var dateElement = li.FindElement(By.XPath(".//span"));
                    string date = dateElement.Text.Trim();

                    // Extract the full text content of the <li> element
                    string fullText = li.Text.Trim();

                    // Remove the date from the full text to get the event title
                    string title = fullText.StartsWith(date) ? fullText.Substring(date.Length).Trim() : fullText;

                    // Try to find the second span if there's additional info like Gregorian or Hijri date
                    string extra = "";
                    var spans = li.FindElements(By.XPath(".//span"));
                    if (spans.Count > 1)
                    {
                        extra = spans[1].Text.Trim();
                    }

                    events.Add(new EventData()
                    {
                        Date = date,
                        Title = title,
                        Extra = extra
                    });
                }
                catch (NoSuchElementException ex)
                {
                    _logger.LogWarning(ex, "Skipping malformed list item");
                    continue; // skip malformed <li>
                }
            }
            
            _logger.LogInformation("Successfully retrieved {Count} events", events.Count);
            return Task.FromResult(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching events data: {Message}", ex.Message);
            return Task.FromResult(new List<EventData>());
        }
    }
    private TimeData GetFallbackTimeData()
    {
        _logger.LogInformation("Using fallback time data mechanism");
        
        var now = DateTime.Now;
        var persianCalendar = new PersianCalendar();
        var hijriCalendar = new HijriCalendar();
        
        return new TimeData
        {
            CurrentTime = now.ToString("HH:mm:ss"),
            CurrentDate = $"{persianCalendar.GetYear(now)}/{persianCalendar.GetMonth(now)}/{persianCalendar.GetDayOfMonth(now)}",
            HijriDate = $"{hijriCalendar.GetYear(now)}/{hijriCalendar.GetMonth(now)}/{hijriCalendar.GetDayOfMonth(now)}",
            GregorianDate = now.ToString("yyyy/MM/dd")
        };
    }

    public void Dispose()
    {
        try
        {
            _logger.LogInformation("Disposing TimeIrCrawlerService");
            _driver?.Quit();
            _driver?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing WebDriver: {Message}", ex.Message);
        }
    }
} 