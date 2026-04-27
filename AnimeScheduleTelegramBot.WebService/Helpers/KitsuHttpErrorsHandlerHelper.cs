using System.Text.Json;
using AnimeScheduleTelegramBot.WebService.Models;

namespace AnimeScheduleTelegramBot.WebService.Helpers;

internal static class KitsuHttpErrorsHandlerHelper
{
	public static async Task EnsureSuccessStatusCodeAsync(
		HttpResponseMessage response,
		ILogger logger,
		CancellationToken cancellationToken)
	{
		if (response.IsSuccessStatusCode)
			return;

		await HandleErrorResponseAsync(response, logger, cancellationToken);
	}

	private static async Task HandleErrorResponseAsync(
		HttpResponseMessage response,
		ILogger logger,
		CancellationToken cancellationToken)
	{
		var errorDetails = await ParseErrorResponseAsync(response, cancellationToken);

		logger.LogError(
			"Kitsu API returned non-success status code {StatusCode}. Error code: {ErrorCode}. Error description: {ErrorDescription}. Response body: {ResponseBody}",
			(int)response.StatusCode,
			errorDetails.ErrorCode,
			errorDetails.ErrorDescription,
			errorDetails.RawBody);

		throw new HttpRequestException(
			$"Kitsu API returned non-success status code {(int)response.StatusCode} ({response.StatusCode}). " +
			$"Error code: {errorDetails.ErrorCode ?? "n/a"}. " +
			$"Error description: {errorDetails.ErrorDescription ?? "n/a"}.",
			null,
			response.StatusCode);
	}

	private static async Task<ErrorDetails> ParseErrorResponseAsync(
		HttpResponseMessage response,
		CancellationToken cancellationToken)
	{
		var rawBody = await response.Content.ReadAsStringAsync(cancellationToken);
		if (string.IsNullOrWhiteSpace(rawBody))
			return new ErrorDetails(null, null, rawBody);

		try
		{
			var oAuthError = JsonSerializer.Deserialize<KitsuOAuthErrorResponse>(rawBody);
			if (!string.IsNullOrWhiteSpace(oAuthError?.Error) || !string.IsNullOrWhiteSpace(oAuthError?.ErrorDescription))
				return new ErrorDetails(oAuthError.Error, oAuthError.ErrorDescription, rawBody);

			var jsonApiError = JsonSerializer.Deserialize<KitsuJsonApiErrorResponse>(rawBody);
			var firstError = jsonApiError?.Errors?.FirstOrDefault();
			if (firstError is null)
				return new ErrorDetails(null, null, rawBody);

			return new ErrorDetails(
				firstError.Code ?? firstError.Status,
				firstError.Detail ?? firstError.Title,
				rawBody);
		}
		catch (JsonException)
		{
			return new ErrorDetails(null, null, rawBody);
		}
	}

	private sealed record ErrorDetails(string? ErrorCode, string? ErrorDescription, string? RawBody);
}