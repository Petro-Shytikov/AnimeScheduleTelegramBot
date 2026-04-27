namespace AnimeScheduleTelegramBot.WebService.Helpers;

internal static class HttpHelper
{
	public const string KitsuAcceptMediaType = "application/vnd.api+json";

	public static Uri CrateBaseUri(string baseUrl)
	{
		var normalizedBaseUrl = baseUrl.EndsWith("/", StringComparison.Ordinal)
			? baseUrl
			: $"{baseUrl}/";

		return new Uri(normalizedBaseUrl, UriKind.Absolute);
	}
}