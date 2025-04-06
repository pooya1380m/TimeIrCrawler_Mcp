# TimeCrawler

A .NET application that crawls [time.ir](https://www.time.ir) to retrieve current time, date information (Shamsi, Hijri, and Gregorian calendars), and events/holidays from the Persian calendar.

## Features

- Retrieves current time data from time.ir:
  - Current time (HH:MM:SS)
  - Current date in Persian/Shamsi calendar
  - Corresponding Hijri date
  - Corresponding Gregorian date
- Fetches events and holidays for the current month
- Implements Model Context Protocol (MCP) server for integration with AI assistants
- Fallback mechanism using system time when web crawling fails

## Technology Stack

- .NET 9.0
- Selenium WebDriver for web scraping
- ChromeDriver for headless browser automation
- Model Context Protocol (MCP) for AI integration
- Microsoft Extensions for dependency injection and logging

## Prerequisites

- .NET 9.0 SDK
- Chrome browser (for ChromeDriver)

## Getting Started

### Installation

1. Clone the repository:
   ```
   git clone [repository-url]
   cd TimeCrawler
   ```

2. Build the project:
   ```
   dotnet build
   ```

3. Run the application:
   ```
   dotnet run
   ```

## API Reference

The application exposes two main API endpoints through the MCP server:

### Get Current Time Data

Retrieves the current time and date information in multiple calendar formats.

```
GetCurrentTimeData()
```

Returns:
- `CurrentTime`: Current time in HH:MM:SS format
- `CurrentDate`: Current date in Persian/Shamsi calendar
- `HijriDate`: Corresponding date in Hijri calendar
- `GregorianDate`: Corresponding date in Gregorian calendar

### Get Events Data

Retrieves events and holidays for the current month.

```
GetEventsDataAsync()
```

Returns a list of events, each containing:
- `Date`: The date of the event
- `Title`: The title or description of the event
- `Extra`: Additional information (if available)

## Architecture

- **Models**: Data structures for time and event information
- **Interfaces**: Contract for TimeIrCrawler service
- **Services**: Implementation of the crawler using Selenium WebDriver
- **Tools**: MCP tool definitions for exposing functionality to AI assistants
