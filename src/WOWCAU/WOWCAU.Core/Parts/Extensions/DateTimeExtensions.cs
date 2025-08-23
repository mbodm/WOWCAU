﻿using System.Globalization;

namespace WOWCAU.Core.Parts.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToIso8601(this DateTime _, bool withMilliseconds = false) =>
            DateTime.UtcNow.ToString(withMilliseconds ? "yyyy-MM-ddTHH:mm:ss.fffZ" : "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
    }
}
