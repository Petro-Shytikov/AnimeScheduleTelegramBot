using AnimeScheduleTelegramBot.WebService.Models;

namespace AnimeScheduleTelegramBot.WebService.Services.Kitsu;

public interface IKitsuHttpProvider
{
	Task<IReadOnlyList<AnimeInfo>> GetCurrentSeasonOngoingsAsync(int year, string season, CancellationToken cancellationToken);
}