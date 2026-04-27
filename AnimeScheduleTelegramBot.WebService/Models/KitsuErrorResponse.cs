using System.Text.Json.Serialization;

namespace AnimeScheduleTelegramBot.WebService.Models;

public sealed record KitsuOAuthErrorResponse(
	[property: JsonPropertyName("error")] string? Error,
	[property: JsonPropertyName("error_description")] string? ErrorDescription
);

public sealed record KitsuJsonApiErrorResponse(
	[property: JsonPropertyName("errors")] IReadOnlyList<KitsuJsonApiErrorItem>? Errors
);

public sealed record KitsuJsonApiErrorItem(
	[property: JsonPropertyName("status")] string? Status,
	[property: JsonPropertyName("code")] string? Code,
	[property: JsonPropertyName("title")] string? Title,
	[property: JsonPropertyName("detail")] string? Detail
);