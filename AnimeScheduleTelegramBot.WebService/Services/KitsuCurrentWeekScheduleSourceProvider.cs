using AnimeScheduleTelegramBot.WebService.Models;
using AnimeScheduleTelegramBot.WebService.Services.Kitsu;

namespace AnimeScheduleTelegramBot.WebService.Services;

public sealed class KitsuCurrentWeekScheduleSourceProvider(
	CacheService<KitsuAnime, (int Year, string Season)> ongoingAnimeCacheService,
	IKitsuHttpProvider kitsuHttpProvider,
	ILogger<KitsuCurrentWeekScheduleSourceProvider> logger)
	: ICacheSourceProvider<AnimeWeekEpisodeInfo, AnimeWeekScheduleContext>
{
	public string GetCacheKey(AnimeWeekScheduleContext context) =>
		$"anime:week-schedule:{context.Year}:{context.Season}:{context.WeekStart:yyyy-MM-dd}";

	public async Task<IReadOnlyList<AnimeWeekEpisodeInfo>> LoadAsync(AnimeWeekScheduleContext context, CancellationToken cancellationToken)
	{
		logger.LogInformation(
			"Loading week schedule source for {Season} {Year}. Week: {WeekStart} - {WeekEnd}.",
			context.Season,
			context.Year,
			context.WeekStart,
			context.WeekEnd);

		var ongoings = await ongoingAnimeCacheService.GetAsync((context.Year, context.Season), forceRefresh: false, cancellationToken);
		var scheduleItems = new List<AnimeWeekEpisodeInfo>();

		foreach (var anime in ongoings)
		{
			var episodes = await kitsuHttpProvider.GetEpisodesByMediaIdAsync(anime.Id, cancellationToken);
			var animeTitle = anime.Attributes.Titles.En ?? anime.Attributes.CanonicalTitle;

			foreach (var episode in episodes)
			{
				if (!TryMapEpisodeForWeek(episode, anime.Id, animeTitle, context, out var scheduleItem))
					continue;

				scheduleItems.Add(scheduleItem);
			}
		}

		return scheduleItems
			.OrderBy(item => item.AirDate)
			.ThenBy(item => item.AnimeTitle)
			.ThenBy(item => item.EpisodeNumber ?? int.MaxValue)
			.ToList()
			.AsReadOnly();
	}

	private static bool TryMapEpisodeForWeek(
		KitsuEpisode episode,
		string animeId,
		string animeTitle,
		AnimeWeekScheduleContext context,
		out AnimeWeekEpisodeInfo scheduleItem)
	{
		scheduleItem = default!;

		if (string.IsNullOrWhiteSpace(episode.Attributes.Airdate))
			return false;

		if (!DateOnly.TryParse(episode.Attributes.Airdate, out var airDate))
			return false;

		if (airDate < context.WeekStart || airDate > context.WeekEnd)
			return false;

		scheduleItem = new AnimeWeekEpisodeInfo(
			AnimeId: animeId,
			AnimeTitle: animeTitle,
			EpisodeNumber: episode.Attributes.Number,
			EpisodeTitle: episode.Attributes.CanonicalTitle,
			AirDate: airDate);

		return true;
	}
}