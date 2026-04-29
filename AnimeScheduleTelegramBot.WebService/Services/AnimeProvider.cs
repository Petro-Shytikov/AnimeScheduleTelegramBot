using AnimeScheduleTelegramBot.WebService.Models;
using AnimeScheduleTelegramBot.WebService.Helpers;

namespace AnimeScheduleTelegramBot.WebService.Services;

public sealed class AnimeProvider(
	CacheService<KitsuAnime, (int Year, string Season)> cacheService,
	CacheService<AnimeWeekEpisodeInfo, AnimeWeekScheduleContext> weekScheduleCacheService) : IAnimeProvider
{
	public async Task<IReadOnlyList<AnimeInfo>> GetCurrentSeasonOngoingsAsync(CancellationToken cancellationToken)
	{
		var loadedOngoings = await cacheService.GetAsync(CommonHelper.GetSeason(DateOnly.FromDateTime(DateTime.UtcNow)), forceRefresh: false, cancellationToken);
		return MapToAnimeInfoList(loadedOngoings);
	}

	public async Task<IReadOnlyList<AnimeWeekEpisodeInfo>> GetCurrentWeekScheduleAsync(DateOnly weekStartDay, DateOnly weekEndDay, CancellationToken cancellationToken)
	{
		var (year, season) = CommonHelper.GetSeason(weekStartDay);
		var context = new AnimeWeekScheduleContext(
			Year: year,
			Season: season,
			WeekStart: weekStartDay,
			WeekEnd: weekEndDay);

		return await weekScheduleCacheService.GetAsync(context, forceRefresh: false, cancellationToken);
	}

	public async Task<IReadOnlyList<AnimeWeekEpisodeInfo>> GetScheduleForDateAsync(DateOnly targetDate, CancellationToken cancellationToken)
	{
		var weekRange = CommonHelper.GetCurrentWeekRange(targetDate);
		var (year, season) = CommonHelper.GetSeason(targetDate);
		var context = new AnimeWeekScheduleContext(
			Year: year,
			Season: season,
			WeekStart: weekRange.Start,
			WeekEnd: weekRange.End);

		var weekSchedule = await weekScheduleCacheService.GetAsync(context, forceRefresh: false, cancellationToken);
		return weekSchedule.Where(item => item.AirDate == targetDate).ToList().AsReadOnly();
	}

	private static IReadOnlyList<AnimeInfo> MapToAnimeInfoList(IReadOnlyList<KitsuAnime> animes) =>
		animes.Select(MapToAnimeInfo).ToList().AsReadOnly();

	private static AnimeInfo MapToAnimeInfo(KitsuAnime anime) =>
		new(
			Id: anime.Id,
			CanonicalTitle: anime.Attributes.CanonicalTitle,
			EnglishTitle: anime.Attributes.Titles.En,
			Status: anime.Attributes.Status,
			Subtype: anime.Attributes.Subtype,
			StartDate: anime.Attributes.StartDate,
			EpisodeCount: anime.Attributes.EpisodeCount
		);
}