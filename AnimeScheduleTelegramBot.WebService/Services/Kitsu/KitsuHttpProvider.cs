using System.Net.Http.Json;
using AnimeScheduleTelegramBot.WebService.Helpers;
using AnimeScheduleTelegramBot.WebService.Models;

namespace AnimeScheduleTelegramBot.WebService.Services.Kitsu;

public sealed class KitsuHttpProvider(
	HttpClient httpClient,
	ILogger<KitsuHttpProvider> logger) : IKitsuHttpProvider
{
	private const string AnimePath = "anime";

	public async Task<IReadOnlyList<KitsuAnime>> GetCurrentSeasonOngoingsAsync(int year, string season, CancellationToken cancellationToken)
	{
		var requestUri = BuildRequestUri(year, season);

		logger.LogInformation("Fetching ongoings from Kitsu API: {RequestUri}", requestUri);

		using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

		using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
		var isSuccessfulResponse = await KitsuHttpErrorsHandlerHelper.EnsureSuccessStatusCodeAsync(response, logger, cancellationToken);
		if (!isSuccessfulResponse)
		{
			logger.LogWarning("Returning empty Kitsu response due to upstream API failure. RequestUri: {RequestUri}", requestUri);
			return [];
		}

		var kitsuResponse = await response.Content.ReadFromJsonAsync<KitsuApiResponse>(cancellationToken);

		return kitsuResponse is null
			? []
			: kitsuResponse.Data.ToList().AsReadOnly();
	}

	private static string BuildRequestUri(int year, string season)
	{
		var encodedYear = Uri.EscapeDataString(year.ToString());
		var encodedSeason = Uri.EscapeDataString(season);
		return $"{AnimePath}?filter[seasonYear]={encodedYear}&filter[season]={encodedSeason}";
	}
}