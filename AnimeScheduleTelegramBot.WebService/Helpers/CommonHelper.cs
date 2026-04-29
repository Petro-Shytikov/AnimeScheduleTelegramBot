namespace AnimeScheduleTelegramBot.WebService.Helpers;

public static class CommonHelper
{
	public static string ServiceVersion =>
		typeof(CommonHelper).Assembly.GetName().Version?.ToString() ?? "unknown";

	public static (int Year, string Season) GetSeason(DateOnly utcDate)
	{
		var season = utcDate.Month switch
		{
			>= 3 and <= 5 => "spring",
			>= 6 and <= 8 => "summer",
			>= 9 and <= 11 => "fall",
			_ => "winter"
		};

		return (utcDate.Year, season);
	}

	public static (DateOnly Start, DateOnly End) GetCurrentWeekRange(DateOnly utcDate)
	{
		var dayOffset = ((int)utcDate.DayOfWeek + 6) % 7;
		var weekStart = utcDate.AddDays(-dayOffset);
		var weekEnd = weekStart.AddDays(6);
		return (weekStart, weekEnd);
	}
}