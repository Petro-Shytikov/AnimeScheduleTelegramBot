using System.Net;
using Polly;
using Polly.Extensions.Http;

namespace AnimeScheduleTelegramBot.WebService.HttpClientHandlers;

internal sealed class RetryPolicyHandler(
	IAppConfiguration appConfiguration,
	ILogger<RetryPolicyHandler> logger) : DelegatingHandler
{
	private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy = CreateRetryPolicy(appConfiguration, logger);

	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
		_retryPolicy.ExecuteAsync(
			async ct => await base.SendAsync(request, ct),
			cancellationToken);

	private static IAsyncPolicy<HttpResponseMessage> CreateRetryPolicy(
		IAppConfiguration appConfiguration,
		ILogger<RetryPolicyHandler> logger)
	{
		var retryCount = Math.Max(0, appConfiguration.KitsuMaxRetries - 1);

		return HttpPolicyExtensions
			.HandleTransientHttpError()
			.OrResult(response => response.StatusCode == HttpStatusCode.TooManyRequests)
			.WaitAndRetryAsync(
				retryCount,
				retryAttempt => TimeSpan.FromMilliseconds(
					appConfiguration.KitsuRetryDelay.TotalMilliseconds * Math.Pow(2, retryAttempt - 1)),
				(outcome, delay, retryAttempt, _) =>
				{
					var statusCode = outcome.Result is null ? "exception" : ((int)outcome.Result.StatusCode).ToString();
					logger.LogWarning(
						"Kitsu HTTP retry {RetryAttempt}/{RetryCount} after {Delay} due to status {StatusCode}.",
						retryAttempt,
						retryCount,
						delay,
						statusCode);
				});
	}
}