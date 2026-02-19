namespace EntryLog.Business.Utils;

public static class TimeFunctions
{
    private const string CENTRAL_AMERICA_STANDARD_TIME = "Central America Standard Time";

    public static DateTime GetCentralAmericaStandardTime()
        => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(CENTRAL_AMERICA_STANDARD_TIME));

    public static DateTime GetCentralAmericaStandardTime(DateTime time)
        => TimeZoneInfo.ConvertTimeFromUtc(time, TimeZoneInfo.FindSystemTimeZoneById(CENTRAL_AMERICA_STANDARD_TIME));
}
