using AnimeScheduleTelegramBot.WebService.Models;

namespace AnimeScheduleTelegramBot.WebService.Services;

public interface IAnimeProvider
{
	Task<IReadOnlyList<AnimeInfo>> GetCurrentSeasonOngoingsAsync(CancellationToken cancellationToken);
	Task<IReadOnlyList<AnimeWeekEpisodeInfo>> GetCurrentWeekScheduleAsync(CancellationToken cancellationToken);
	Task<IReadOnlyList<AnimeWeekEpisodeInfo>> GetYesterdayScheduleAsync(CancellationToken cancellationToken);
	Task<IReadOnlyList<AnimeWeekEpisodeInfo>> GetTodayScheduleAsync(CancellationToken cancellationToken);
	Task<IReadOnlyList<AnimeWeekEpisodeInfo>> GetTomorrowScheduleAsync(CancellationToken cancellationToken);
}
