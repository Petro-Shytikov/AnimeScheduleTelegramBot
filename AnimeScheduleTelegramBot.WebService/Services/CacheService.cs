using Microsoft.Extensions.Caching.Memory;

namespace AnimeScheduleTelegramBot.WebService.Services;

public sealed class CacheService<T, TContext>(
	IMemoryCache memoryCache,
	ICacheSourceProvider<T, TContext> sourceProvider,
	CacheSignal<T> refreshSignal,
	IAppConfiguration appConfiguration,
	ILogger<CacheService<T, TContext>> logger)
{
	public async Task<IReadOnlyList<T>> GetAsync(TContext context, bool forceRefresh, CancellationToken cancellationToken)
	{
		var cacheKey = sourceProvider.GetCacheKey(context);

		if (!forceRefresh)
		{
			var cachedItems = GetCachedItems(cacheKey);
			if (cachedItems is not null)
				return cachedItems;
		}

		await refreshSignal.WaitAsync().WaitAsync(cancellationToken);
		try
		{
			cacheKey = sourceProvider.GetCacheKey(context);

			if (!forceRefresh)
			{
				var cachedItems = GetCachedItems(cacheKey);
				if (cachedItems is not null)
					return cachedItems;
			}

			var loadedItems = await sourceProvider.LoadAsync(context, cancellationToken);
			if (loadedItems.Count == 0)
			{
				var cachedItems = GetCachedItems(cacheKey);
				if (cachedItems is not null)
				{
					logger.LogWarning(
						"Keeping existing cache entry {CacheKey} because source returned no items.",
						cacheKey);
					return cachedItems;
				}

				logger.LogWarning("Source returned no items for cache key {CacheKey}. Cache remains empty.", cacheKey);
				return [];
			}

			var persistedItems = loadedItems.ToList().AsReadOnly();
			memoryCache.Set(cacheKey, persistedItems, appConfiguration.AnimeCacheLifetime);

			logger.LogInformation(
				"Cache updated for key {CacheKey}. Items count: {Count}. UpdatedAtUtc: {UpdatedAtUtc}.",
				cacheKey,
				persistedItems.Count,
				DateTimeOffset.UtcNow);

			return persistedItems;
		}
		finally
		{
			refreshSignal.Release();
		}
	}

	private IReadOnlyList<T>? GetCachedItems(string cacheKey)
	{
		if (!memoryCache.TryGetValue<IReadOnlyList<T>>(cacheKey, out var cachedItems) || cachedItems is null)
			return null;

		return cachedItems;
	}
}