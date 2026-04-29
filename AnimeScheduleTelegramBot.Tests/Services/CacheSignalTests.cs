using AnimeScheduleTelegramBot.WebService.Services;

namespace AnimeScheduleTelegramBot.Tests.Services;

public class CacheSignalTests
{
	[Test]
	public async Task WaitAsync_WithCancellationTokenNone_CompletesSuccessfully()
	{
		var signal = new CacheSignal<int>();

		await signal.WaitAsync(CancellationToken.None);
		signal.Release();
		await signal.WaitAsync(CancellationToken.None);
		signal.Release();
	}

	[Test]
	public async Task WaitAsync_WithAlreadyCanceledToken_ThrowsOperationCanceledException()
	{
		var signal = new CacheSignal<int>();
		using var cancellationTokenSource = new CancellationTokenSource();
		cancellationTokenSource.Cancel();

		await Assert.ThrowsAsync<OperationCanceledException>(async () =>
			await signal.WaitAsync(cancellationTokenSource.Token));
	}

	[Test]
	public async Task WaitAsync_WhenSemaphoreIsHeldAndTokenCancels_ThrowsOperationCanceledException()
	{
		var signal = new CacheSignal<int>();
		await signal.WaitAsync(CancellationToken.None);

		try
		{
			using var cancellationTokenSource = new CancellationTokenSource();
			var waitTask = signal.WaitAsync(cancellationTokenSource.Token);

			Assert.That(waitTask.IsCompleted, Is.False);

			cancellationTokenSource.Cancel();

			await Assert.ThrowsAsync<OperationCanceledException>(async () =>
				await waitTask);
		}
		finally
		{
			signal.Release();
		}
	}
}
