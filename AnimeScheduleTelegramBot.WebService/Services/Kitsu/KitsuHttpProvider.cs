using System.Net.Http.Json;
using AnimeScheduleTelegramBot.WebService.Helpers;
using AnimeScheduleTelegramBot.WebService.Models;

namespace AnimeScheduleTelegramBot.WebService.Services.Kitsu;

public sealed class KitsuHttpProvider(
	HttpClient httpClient,
	ILogger<KitsuHttpProvider> logger) : IKitsuHttpProvider
{
	private const string AnimePath = "anime";
	private const string EpisodesPath = "episodes";
	private const int EpisodesPageLimit = 20;
	private const int MaxEpisodesPages = 20;

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

	public async Task<IReadOnlyList<KitsuEpisode>> GetEpisodesByMediaIdAsync(string mediaId, CancellationToken cancellationToken)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(mediaId);

		var episodes = new List<KitsuEpisode>();
		var requestUri = BuildEpisodesRequestUri(mediaId);

		for (var pageNumber = 1; pageNumber <= MaxEpisodesPages && !string.IsNullOrWhiteSpace(requestUri); pageNumber++)
		{
			logger.LogInformation("Fetching episodes from Kitsu API: {RequestUri}", requestUri);

			using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
			using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

			var isSuccessfulResponse = await KitsuHttpErrorsHandlerHelper.EnsureSuccessStatusCodeAsync(response, logger, cancellationToken);
			if (!isSuccessfulResponse)
			{
				logger.LogWarning("Returning empty episodes response due to upstream API failure. RequestUri: {RequestUri}", requestUri);
				return [];
			}

			var kitsuResponse = await response.Content.ReadFromJsonAsync<KitsuEpisodesApiResponse>(cancellationToken);
			if (kitsuResponse is null)
				return [];

			episodes.AddRange(kitsuResponse.Data);
			requestUri = kitsuResponse.Links?.Next;
		}

		return episodes.AsReadOnly();
	}

	private static string BuildEpisodesRequestUri(string mediaId)
	{
		var encodedMediaId = Uri.EscapeDataString(mediaId);
		return $"{EpisodesPath}?filter[mediaId]={encodedMediaId}&page[limit]={EpisodesPageLimit}";
	}
}