namespace AnimeScheduleTelegramBot.WebService.Models;

public sealed record AnimeWeekEpisodeInfo(
	string AnimeId,
	string AnimeTitle,
	int? EpisodeNumber,
	string? EpisodeTitle,
	DateOnly AirDate
);