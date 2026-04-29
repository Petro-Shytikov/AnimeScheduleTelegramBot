using AnimeScheduleTelegramBot.WebService.Helpers;

namespace AnimeScheduleTelegramBot.Tests.Helpers;

public class CommonHelperTests
{
	[Test]
	[Arguments("2025-01-15", 2025, "winter")]
	[Arguments("2025-02-28", 2025, "winter")]
	[Arguments("2025-03-01", 2025, "spring")]
	[Arguments("2025-05-31", 2025, "spring")]
	[Arguments("2025-06-01", 2025, "summer")]
	[Arguments("2025-08-31", 2025, "summer")]
	[Arguments("2025-09-01", 2025, "fall")]
	[Arguments("2025-11-30", 2025, "fall")]
	[Arguments("2025-12-01", 2025, "winter")]
	[Arguments("2026-12-31", 2026, "winter")]
	public async Task GetSeason_ReturnsExpectedYearAndSeason(string inputDateValue, int expectedYear, string expectedSeason)
	{
		var inputDate = DateOnly.Parse(inputDateValue);

		var result = CommonHelper.GetSeason(inputDate);

		await Assert.That(result.Year).IsEqualTo(expectedYear);
		await Assert.That(result.Season).IsEqualTo(expectedSeason);
	}

	[Test]
	[Arguments("2025-01-06", "2025-01-06", "2025-01-12")] // Monday
	[Arguments("2025-01-07", "2025-01-06", "2025-01-12")] // Tuesday
	[Arguments("2025-01-08", "2025-01-06", "2025-01-12")] // Wednesday
	[Arguments("2025-01-09", "2025-01-06", "2025-01-12")] // Thursday
	[Arguments("2025-01-10", "2025-01-06", "2025-01-12")] // Friday
	[Arguments("2025-01-11", "2025-01-06", "2025-01-12")] // Saturday
	[Arguments("2025-01-12", "2025-01-06", "2025-01-12")] // Sunday
	public async Task GetCurrentWeekRange_ReturnsMondayToSundayRange(string inputDateValue, string expectedStartValue, string expectedEndValue)
	{
		var inputDate = DateOnly.Parse(inputDateValue);
		var expectedStart = DateOnly.Parse(expectedStartValue);
		var expectedEnd = DateOnly.Parse(expectedEndValue);

		var result = CommonHelper.GetCurrentWeekRange(inputDate);

		await Assert.That(result.Start).IsEqualTo(expectedStart);
		await Assert.That(result.End).IsEqualTo(expectedEnd);
	}

	[Test]
	public async Task GetCurrentWeekRange_EndDateIsAlwaysSixDaysAfterStartDate()
	{
		var inputDate = new DateOnly(2026, 12, 31);

		var result = CommonHelper.GetCurrentWeekRange(inputDate);

		await Assert.That(result.End.DayNumber - result.Start.DayNumber).IsEqualTo(6);
	}
}
