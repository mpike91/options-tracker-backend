public static class DateTimeUtils
{
    public static DateTime GetEasternTime()
    {
        var eastern = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, eastern);
    }

    public static DateTime ToEasternTime(this DateTime dateTime)
    {
        var eastern = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
        return TimeZoneInfo.ConvertTimeFromUtc(dateTime, eastern);
    }
}
