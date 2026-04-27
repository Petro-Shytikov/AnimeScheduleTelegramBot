namespace AnimeScheduleTelegramBot.WebService.Helpers;

internal static class HttpHelper
{
	public static Uri CrateBaseUri(string baseUrl)
	{
		var normalizedBaseUrl = baseUrl.EndsWith("/", StringComparison.Ordinal)
			? baseUrl
			: $"{baseUrl}/";

		return new Uri(normalizedBaseUrl, UriKind.Absolute);
	}
}