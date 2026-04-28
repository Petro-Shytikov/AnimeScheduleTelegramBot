namespace AnimeScheduleTelegramBot.WebService.Models;

public sealed record AnimeWeekScheduleContext(
	int Year,
	string Season,
	DateOnly WeekStart,
	DateOnly WeekEnd
);