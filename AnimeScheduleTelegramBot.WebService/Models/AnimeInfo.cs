namespace AnimeScheduleTelegramBot.WebService.Models;

public sealed record AnimeInfo(
	string Id,
	string CanonicalTitle,
	string? EnglishTitle,
	string? Status,
	string? Subtype,
	string? StartDate,
	int? EpisodeCount
);
