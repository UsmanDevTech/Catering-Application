using TimeZoneConverter;

namespace Application.Common.Extensions;


public static class DateTimeExtension
{
    public static DateTime UtcToLocal(this DateTime source, TimeZoneInfo localTimeZone)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(source, localTimeZone);
    }

    public static DateTime LocalToUtc(this DateTime source, TimeZoneInfo localTimeZone)
    {
        //to avoid error during conversion in case when DateTime string from swagger is sent with a "Z"
        //at the end, which sets the Kind property to Utc
        source = DateTime.SpecifyKind(source, DateTimeKind.Unspecified);

        return TimeZoneInfo.ConvertTimeToUtc(source, localTimeZone);
    }

    //******************** using TimeZoneConverter library *************************//
    //To resolve timeZoneId issues, so that works with different kind of Ids such as "Asia/Karachi"

    public static DateTime UtcToLocal(this DateTime source, string localTimeZone)
    {
        TimeZoneInfo myTimeZone = TZConvert.GetTimeZoneInfo(localTimeZone);
        return TimeZoneInfo.ConvertTimeFromUtc(source, myTimeZone);
    }

    ////To deal with nullable DateTime
    //public static DateTime UtcToLocal(this DateTime? source, string localTimeZone)
    //{
    //    DateTime sourceNonNullable = source.Value;

    //    return UtcToLocal(sourceNonNullable, localTimeZone);
    //}

    public static DateTime LocalToUtc(this DateTime source, string localTimeZone)
    {
        //to avoid error during conversion in case when DateTime string from swagger is sent with a "Z"
        //at the end, which sets the Kind property to Utc
        source = DateTime.SpecifyKind(source, DateTimeKind.Unspecified);

        TimeZoneInfo myTimeZone = TZConvert.GetTimeZoneInfo(localTimeZone);
        return TimeZoneInfo.ConvertTimeToUtc(source, myTimeZone);
    }

    //To deal with nullable DateTime
    public static DateTime LocalToUtc(this DateTime? source, string localTimeZone)
    {
        DateTime sourceNonNullable = source.Value;

        return LocalToUtc(sourceNonNullable, localTimeZone);
    }



    ////******************************* without any library*******************************//
    ////works with timeZoneIds like "Pakistan Standard Time"

    //public static DateTime UtcToLocalTime(this DateTime source, string localTimeZone)
    //{
    //    TimeZoneInfo myTimeZone = TimeZoneInfo.FindSystemTimeZoneById(localTimeZone);
    //    return TimeZoneInfo.ConvertTimeFromUtc(source, myTimeZone);
    //}
    //public static DateTime LocaltoUtcTime(this DateTime source, string localTimeZone)
    //{
    //    //to avoid error during conversion in case when DateTime string from swagger is sent with a "Z"
    //    //at the end, which sets the Kind property to Utc
    //    source = DateTime.SpecifyKind(source, DateTimeKind.Unspecified);

    //    TimeZoneInfo myTimeZone = TimeZoneInfo.FindSystemTimeZoneById(localTimeZone);
    //    return TimeZoneInfo.ConvertTimeToUtc(source, myTimeZone);
    //}

}
