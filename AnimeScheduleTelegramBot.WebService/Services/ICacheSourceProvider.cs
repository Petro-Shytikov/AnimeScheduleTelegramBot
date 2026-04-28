namespace AnimeScheduleTelegramBot.WebService.Services;

public interface ICacheSourceProvider<T, TContext>
{
	string GetCacheKey(TContext context);

	Task<IReadOnlyList<T>> LoadAsync(TContext context, CancellationToken cancellationToken);
}