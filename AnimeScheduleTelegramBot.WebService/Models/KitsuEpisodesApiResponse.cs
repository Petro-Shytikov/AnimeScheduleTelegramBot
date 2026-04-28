using System.Text.Json.Serialization;

namespace AnimeScheduleTelegramBot.WebService.Models;

public sealed record KitsuEpisodesApiResponse(
	[property: JsonPropertyName("data")] IReadOnlyList<KitsuEpisode> Data,
	[property: JsonPropertyName("links")] KitsuPageLinks? Links
);

public sealed record KitsuEpisode(
	[property: JsonPropertyName("id")] string Id,
	[property: JsonPropertyName("type")] string Type,
	[property: JsonPropertyName("attributes")] KitsuEpisodeAttributes Attributes
);

public sealed record KitsuEpisodeAttributes(
	[property: JsonPropertyName("canonicalTitle")] string? CanonicalTitle,
	[property: JsonPropertyName("number")] int? Number,
	[property: JsonPropertyName("airdate")] string? Airdate
);

public sealed record KitsuPageLinks(
	[property: JsonPropertyName("next")] string? Next
);