using AnimeScheduleTelegramBot.WebService.Models;

namespace AnimeScheduleTelegramBot.WebService.Services;

public interface IAnimeProvider
{
	Task<IReadOnlyList<AnimeInfo>> GetCurrentSeasonOngoingsAsync(CancellationToken cancellationToken);
	Task<IReadOnlyList<AnimeWeekEpisodeInfo>> GetCurrentWeekScheduleAsync(DateOnly weekStartDay, DateOnly weekEndDay, CancellationToken cancellationToken);
	Task<IReadOnlyList<AnimeWeekEpisodeInfo>> GetScheduleForDateAsync(DateOnly targetDate, CancellationToken cancellationToken);
}
