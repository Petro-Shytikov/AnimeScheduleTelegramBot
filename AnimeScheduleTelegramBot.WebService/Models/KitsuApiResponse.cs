using System.Text.Json.Serialization;

namespace AnimeScheduleTelegramBot.WebService.Models;

public sealed record KitsuApiResponse(
	[property: JsonPropertyName("data")] IReadOnlyList<KitsuAnime> Data
);

public sealed record KitsuAnime(
	[property: JsonPropertyName("id")] string Id,
	[property: JsonPropertyName("type")] string Type,
	[property: JsonPropertyName("attributes")] KitsuAnimeAttributes Attributes
);

public sealed record KitsuAnimeAttributes(
	[property: JsonPropertyName("canonicalTitle")] string CanonicalTitle,
	[property: JsonPropertyName("titles")] KitsuAnimeTitles Titles,
	[property: JsonPropertyName("status")] string? Status,
	[property: JsonPropertyName("subtype")] string? Subtype,
	[property: JsonPropertyName("startDate")] string? StartDate,
	[property: JsonPropertyName("endDate")] string? EndDate,
	[property: JsonPropertyName("episodeCount")] int? EpisodeCount,
	[property: JsonPropertyName("averageRating")] string? AverageRating
);

public sealed record KitsuAnimeTitles(
	[property: JsonPropertyName("en")] string? En,
	[property: JsonPropertyName("en_jp")] string? EnJp,
	[property: JsonPropertyName("ja_jp")] string? JaJp
);
