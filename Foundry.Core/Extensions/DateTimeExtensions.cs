using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Foundry
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Returns whether the <see cref="DateTime"/> element contains a Date component equal
        /// to the maximum possible date.
        /// </summary>
        /// <param name="element">The date element.</param>
        /// <returns><see langword="true"/> if the <see cref="DateTime"/> element contains a Date component equal to the
        /// maximum possible date, <see langword="false"/> otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsMaxDate(this DateTime element)
            => element.Date == DateTime.MaxValue.Date;

        /// <summary>
        /// Returns whether the <see cref="DateTimeOffset"/> element contains a Date component equal
        /// to the maximum possible date.
        /// </summary>
        /// <param name="element">The date element.</param>
        /// <returns><see langword="true"/> if the <see cref="DateTimeOffset"/> element contains a Date component equal to the
        /// maximum possible date, <see langword="false"/> otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsMaxDate(this DateTimeOffset element)
            => element.Date == DateTimeOffset.MaxValue.Date;

        /// <summary>
        /// Returns whether the <see cref="DateTime"/> element contains a Time component equal
        /// to the maximum possible time.
        /// </summary>
        /// <param name="element">The date element.</param>
        /// <returns><see langword="true"/> if the <see cref="DateTime"/> element contains a Time component equal to the
        /// maximum possible time, <see langword="false"/> otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsMaxTime(this DateTime element)
            => element.TimeOfDay == DateTime.MaxValue.TimeOfDay;

        /// <summary>
        /// Returns whether the <see cref="DateTimeOffset"/> element contains a Time component equal
        /// to the maximum possible time.
        /// </summary>
        /// <param name="element">The date element.</param>
        /// <returns><see langword="true"/> if the <see cref="DateTimeOffset"/> element contains a Time component equal to the
        /// maximum possible time, <see langword="false"/> otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsMaxTime(this DateTimeOffset element)
            => element.TimeOfDay == DateTimeOffset.MaxValue.TimeOfDay;

        /// <summary>
        /// Returns whether the <see cref="DateTime"/> element contains a Date component equal
        /// to the minimum possible date.
        /// </summary>
        /// <param name="element">The date element.</param>
        /// <returns><see langword="true"/> if the <see cref="DateTime"/> element contains a Date component equal to the
        /// minimum possible date, <see langword="false"/> otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsMinDate(this DateTime element)
            => element.Date == DateTime.MinValue.Date;

        /// <summary>
        /// Returns whether the <see cref="DateTimeOffset"/> element contains a Date component equal
        /// to the minimum possible date.
        /// </summary>
        /// <param name="element">The date element.</param>
        /// <returns><see langword="true"/> if the <see cref="DateTimeOffset"/> element contains a Date component equal to the
        /// minimum possible date, <see langword="false"/> otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsMinDate(this DateTimeOffset element)
            => element.Date == DateTimeOffset.MinValue.Date;

        /// <summary>
        /// Returns whether the <see cref="DateTime"/> element contains a Date component equal
        /// to the minimum or maximum possible date.
        /// </summary>
        /// <param name="element">The date element.</param>
        /// <returns><see langword="true"/> if the <see cref="DateTime"/> element contains a Date component equal to the
        /// minimum or maximum possible date, <see langword="false"/> otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsMinOrMaxDate(this DateTime element)
            => element.IsMinDate() || element.IsMaxDate();

        /// <summary>
        /// Returns whether the <see cref="DateTimeOffset"/> element contains a Date component equal
        /// to the minimum or maximum possible date.
        /// </summary>
        /// <param name="element">The date element.</param>
        /// <returns><see langword="true"/> if the <see cref="DateTimeOffset"/> element contains a Date component equal to the
        /// minimum or maximum possible date, <see langword="false"/> otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsMinOrMaxDate(this DateTimeOffset element)
            => element.IsMinDate() || element.IsMaxDate();

        /// <summary>
        /// Returns whether the <see cref="DateTime"/> element contains a Time component equal
        /// to the minimum or maximum possible Time.
        /// </summary>
        /// <param name="element">The date element.</param>
        /// <returns><see langword="true"/> if the <see cref="DateTime"/> element contains a Time component equal to the
        /// minimum or maximum possible time, <see langword="false"/> otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsMinOrMaxTime(this DateTime element)
            => element.IsMinTime() || element.IsMaxTime();

        /// <summary>
        /// Returns whether the <see cref="DateTimeOffset"/> element contains a Time component equal
        /// to the minimum or maximum possible Time.
        /// </summary>
        /// <param name="element">The date element.</param>
        /// <returns><see langword="true"/> if the <see cref="DateTimeOffset"/> element contains a Time component equal to the
        /// minimum or maximum possible time, <see langword="false"/> otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsMinOrMaxTime(this DateTimeOffset element)
            => element.IsMinTime() || element.IsMaxTime();

        /// <summary>
        /// Returns whether the <see cref="DateTime"/> element contains a Time component equal
        /// to the minimum possible time.
        /// </summary>
        /// <param name="element">The date element.</param>
        /// <returns><see langword="true"/> if the <see cref="DateTime"/> element contains a Time component equal to the
        /// minimum possible time, <see langword="false"/> otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsMinTime(this DateTime element)
            => element.TimeOfDay == DateTime.MinValue.TimeOfDay;

        /// <summary>
        /// Returns whether the <see cref="DateTimeOffset"/> element contains a Time component equal
        /// to the minimum possible time.
        /// </summary>
        /// <param name="element">The date element.</param>
        /// <returns><see langword="true"/> if the <see cref="DateTimeOffset"/> element contains a Time component equal to the
        /// minimum possible time, <see langword="false"/> otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsMinTime(this DateTimeOffset element)
            => element.TimeOfDay == DateTimeOffset.MinValue.TimeOfDay;

        /// <summary>
        /// Returns the <see cref="DayOfWeek"/> that corresponds to the next day of the <see cref="DateTime"/> instance.
        /// </summary>
        /// <param name="element">The date element.</param>
        /// <returns>The <see cref="DayOfWeek"/> that corresponds to the next day of the <see cref="DateTime"/> instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The resulting <see cref="T:System.DateTime" /> is less than <see cref="F:System.DateTime.MinValue" /> or greater than <see cref="F:System.DateTime.MaxValue" />. </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DayOfWeek NextDay(this DateTime element)
            => element.AddDays(1).DayOfWeek;

        /// <summary>
        /// Returns the <see cref="DayOfWeek"/> that corresponds to the next day of the <see cref="DateTimeOffset"/> instance.
        /// </summary>
        /// <param name="element">The date element.</param>
        /// <returns>The <see cref="DayOfWeek"/> that corresponds to the next day of the <see cref="DateTimeOffset"/> instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The resulting <see cref="T:System.DateTimeOffset" /> is less than <see cref="F:System.DateTimeOffset.MinValue" /> or greater than <see cref="F:System.DateTimeOffset.MaxValue" />. </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DayOfWeek NextDay(this DateTimeOffset element)
            => element.AddDays(1).DayOfWeek;

        /// <summary>
        /// Returns the <see cref="DateTime"/> containing the next occurrence of the specified day of week relative to this <see cref="DateTime"/>.
        /// (i.e. From this date, what date is the next upcoming Friday?)
        /// </summary>
        /// <param name="element">The date element.</param>
        /// <param name="nextDayOfWeek">The next day of week.</param>
        /// <returns>The <see cref="DateTime"/> containing the next occurrence of the specified day of week relative to this <see cref="DateTime"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The resulting <see cref="T:System.DateTime" /> is less than <see cref="F:System.DateTime.MinValue" /> or greater than <see cref="F:System.DateTime.MaxValue" />. </exception>
        public static DateTime NextDay(this DateTime element, DayOfWeek nextDayOfWeek)
        {
            int offsetDays = nextDayOfWeek - element.DayOfWeek;
            if (offsetDays <= 0)
            {
                offsetDays += 7;
            }

            return element.AddDays(offsetDays);
        }

        /// <summary>
        /// Returns the <see cref="DateTimeOffset"/> containing the next occurrence of the specified day of week relative to this <see cref="DateTimeOffset"/>.
        /// (i.e. From this date, what date is the next upcoming Friday?)
        /// </summary>
        /// <param name="element">The date element.</param>
        /// <param name="nextDayOfWeek">The next day of week.</param>
        /// <returns>The <see cref="DateTimeOffset"/> containing the next occurrence of the specified day of week relative to this <see cref="DateTimeOffset"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The resulting <see cref="T:System.DateTimeOffset" /> is less than <see cref="F:System.DateTimeOffset.MinValue" /> or greater than <see cref="F:System.DateTimeOffset.MaxValue" />. </exception>
        public static DateTimeOffset NextDay(this DateTimeOffset element, DayOfWeek nextDayOfWeek)
        {
            int offsetDays = nextDayOfWeek - element.DayOfWeek;
            if (offsetDays <= 0)
            {
                offsetDays += 7;
            }

            return element.AddDays(offsetDays);
        }

        /// <summary>
        /// Returns the <see cref="DayOfWeek"/> that corresponds to the previous day of the <see cref="DateTime"/> instance.
        /// </summary>
        /// <param name="element">The date element.</param>
        /// <returns>The <see cref="DayOfWeek"/> that corresponds to the previous day of the <see cref="DateTime"/> instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The resulting <see cref="T:System.DateTime" /> is less than <see cref="F:System.DateTime.MinValue" /> or greater than <see cref="F:System.DateTime.MaxValue" />. </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DayOfWeek PrevDay(this DateTime element)
            => element.AddDays(-1).DayOfWeek;

        /// <summary>
        /// Returns the <see cref="DayOfWeek"/> that corresponds to the previous day of the <see cref="DateTimeOffset"/> instance.
        /// </summary>
        /// <param name="element">The date element.</param>
        /// <returns>The <see cref="DayOfWeek"/> that corresponds to the previous day of the <see cref="DateTimeOffset"/> instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The resulting <see cref="T:System.DateTimeOffset" /> is less than <see cref="F:System.DateTimeOffset.MinValue" /> or greater than <see cref="F:System.DateTimeOffset.MaxValue" />. </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DayOfWeek PrevDay(this DateTimeOffset element)
            => element.AddDays(-1).DayOfWeek;

        /// <summary>
        /// Returns the <see cref="DateTime"/> containing the last occurrence of the specified day of week relative to this <see cref="DateTime"/>.
        /// (i.e. From this date, what date is the next upcoming Friday?)
        /// </summary>
        /// <param name="element">The date element.</param>
        /// <param name="nextDayOfWeek">The next day of week.</param>
        /// <returns>The <see cref="DateTime"/> containing the last occurrence of the specified day of week relative to this <see cref="DateTime"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The resulting <see cref="T:System.DateTime" /> is less than <see cref="F:System.DateTime.MinValue" /> or greater than <see cref="F:System.DateTime.MaxValue" />. </exception>
        public static DateTime PrevDay(this DateTime element, DayOfWeek nextDayOfWeek)
        {
            int offsetDays = nextDayOfWeek - element.DayOfWeek;
            if (offsetDays > 0)
            {
                offsetDays -= 7;
            }

            return element.AddDays(offsetDays);
        }

        /// <summary>
        /// Returns the <see cref="DateTimeOffset"/> containing the last occurrence of the specified day of week relative to this <see cref="DateTimeOffset"/>.
        /// (i.e. From this date, what date is the next upcoming Friday?)
        /// </summary>
        /// <param name="element">The date element.</param>
        /// <param name="nextDayOfWeek">The next day of week.</param>
        /// <returns>The <see cref="DateTimeOffset"/> containing the last occurrence of the specified day of week relative to this <see cref="DateTimeOffset"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The resulting <see cref="T:System.DateTimeOffset" /> is less than <see cref="F:System.DateTimeOffset.MinValue" /> or greater than <see cref="F:System.DateTimeOffset.MaxValue" />. </exception>
        public static DateTimeOffset PrevDay(this DateTimeOffset element, DayOfWeek nextDayOfWeek)
        {
            int offsetDays = nextDayOfWeek - element.DayOfWeek;
            if (offsetDays > 0)
            {
                offsetDays -= 7;
            }

            return element.AddDays(offsetDays);
        }

        /// <summary>
        /// Converts a <see cref="DateTime"/> object into a string format suitable for
        /// file names.
        /// </summary>
        /// <param name="dateTime">The <see cref="DateTime"/> object.</param>
        public static string ToFileNameTime(this DateTime dateTime)
            => dateTime.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);

        /// <summary>
        /// Converts a <see cref="DateTimeOffset"/> object into a string format suitable for
        /// file names.
        /// </summary>
        /// <param name="dateTime">The <see cref="DateTimeOffset"/> object.</param>
        public static string ToFileNameTime(this DateTimeOffset dateTime)
            => dateTime.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);

        /// <summary>
        /// Returns the <see cref="DateTime"/> as a human readable sentence (i.e. 'one second ago' or '4 months ago').
        /// </summary>
        /// <param name="element">The date element.</param>
        /// <returns>The <see cref="DateTime"/> as a human readable sentence.</returns>
        /// <exception cref="OverflowException"></exception>
        public static string ToHumanReadableString(this DateTime element)
        {
            var timeSince = new TimeSpan((element.Kind == DateTimeKind.Utc ? DateTime.UtcNow.Ticks : DateTime.Now.Ticks) - element.Ticks);
            double delta = timeSince.TotalSeconds;
            if (delta < 60)
            {
                return timeSince.Seconds == 1 ? "one second ago" : timeSince.Seconds + " seconds ago";
            }
            if (delta < 120)
            {
                return "a minute ago";
            }
            if (delta < 2700) // 45 * 60
            {
                return timeSince.Minutes + " minutes ago";
            }
            if (delta < 5400) // 90 * 60
            {
                return "an hour ago";
            }
            if (delta < 86400) // 24 * 60 * 60
            {
                return timeSince.Hours + " hours ago";
            }
            if (delta < 172800) // 48 * 60 * 60
            {
                return "yesterday";
            }
            if (delta < 2592000) // 30 * 24 * 60 * 60
            {
                return timeSince.Days + " days ago";
            }
            if (delta < 31104000) // 12 * 30 * 24 * 60 * 60
            {
                // We use Convert.ToInt32 since we need correct rounding behavior.
                int months = Convert.ToInt32(Math.Floor((double)timeSince.Days / 30));
                return months <= 1 ? "one month ago" : months + " months ago";
            }

            // We use Convert.ToInt32 since we need correct rounding behavior.
            int years = Convert.ToInt32(Math.Floor((double)timeSince.Days / 365));
            return years <= 1 ? "one year ago" : years + " years ago";
        }

        /// <summary>
        /// Returns the <see cref="DateTimeOffset"/> as a human readable sentence (i.e. 'one second ago' or '4 months ago').
        /// </summary>
        /// <param name="element">The date element.</param>
        /// <returns>The <see cref="DateTimeOffset"/> as a human readable sentence.</returns>
        /// <exception cref="OverflowException"></exception>
        public static string ToHumanReadableString(this DateTimeOffset element)
        {
            var timeSince = new TimeSpan((element.Offset == TimeSpan.Zero ? DateTimeOffset.UtcNow.Ticks : DateTimeOffset.Now.Ticks) - element.Ticks);
            double delta = timeSince.TotalSeconds;
            if (delta < 60)
            {
                return timeSince.Seconds == 1 ? "one second ago" : timeSince.Seconds + " seconds ago";
            }
            if (delta < 120)
            {
                return "a minute ago";
            }
            if (delta < 2700) // 45 * 60
            {
                return timeSince.Minutes + " minutes ago";
            }
            if (delta < 5400) // 90 * 60
            {
                return "an hour ago";
            }
            if (delta < 86400) // 24 * 60 * 60
            {
                return timeSince.Hours + " hours ago";
            }
            if (delta < 172800) // 48 * 60 * 60
            {
                return "yesterday";
            }
            if (delta < 2592000) // 30 * 24 * 60 * 60
            {
                return timeSince.Days + " days ago";
            }
            if (delta < 31104000) // 12 * 30 * 24 * 60 * 60
            {
                // We use Convert.ToInt32 since we need correct rounding behavior.
                int months = Convert.ToInt32(Math.Floor((double)timeSince.Days / 30));
                return months <= 1 ? "one month ago" : months + " months ago";
            }

            // We use Convert.ToInt32 since we need correct rounding behavior.
            int years = Convert.ToInt32(Math.Floor((double)timeSince.Days / 365));
            return years <= 1 ? "one year ago" : years + " years ago";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToTimeSpanString(this DateTime startTime)
        {
            var ts = new TimeSpan((startTime.Kind == DateTimeKind.Utc ? DateTime.UtcNow.Ticks : DateTime.Now.Ticks) - startTime.Ticks);
            return ToTimeSpanString(ts);
        }

        public static string ToTimeSpanString(this TimeSpan timeSpan)
        {
            int i = TimeRanges.Keys.ToList().BinarySearch((long)timeSpan.TotalSeconds);
            // if index is positive, then one of the keys was an exact match (0-based)
            // if negative, the absolute value is the index of the next largest key (1-based)
            if (i < 0)
            {
                i = Math.Abs(i) - 1;
            }
            return string.Format(new HourMinSecFormatter(),
                TimeRanges[TimeRanges.Keys[i]],
                timeSpan.Days,
                timeSpan.Hours,
                timeSpan.Minutes,
                timeSpan.Seconds);
        }

        private static readonly SortedList<long, string> TimeRanges = new SortedList<long, string>
        {
            { 59, "{3:S}" },
            { 60, "{2:M}" },
            { 60 * 60 - 1, "{2:M}, {3:S}" },
            { 60 * 60, "{1:H}" },
            { 24 * 60 * 60 - 1, "{1:H}, {2:M}, {3:S}" },
            { 24 * 60 * 60, "{0:D}" },
            { long.MaxValue, "{0:N}, {1:H}, {2:M}" }
        };

        private sealed class HourMinSecFormatter : ICustomFormatter, IFormatProvider
        {
            private static readonly Dictionary<string, string> TimeFormats = new Dictionary<string, string>
            {
                { "S", "{0:P:Seconds:Second}" },
                { "M", "{0:P:Minutes:Minute}" },
                { "H", "{0:P:Hours:Hour}" },
                { "D", "{0:P:Days:Day}" },
                { "N", "{0:PN:Days:Day}" }
            };

            public string Format(string? format, object? arg, IFormatProvider? formatProvider)
                => string.Format(new PluralFormatter(), TimeFormats[format!], arg);

            public object? GetFormat(Type? formatType)
                => formatType == typeof(ICustomFormatter) ? this : null;

            private sealed class PluralFormatter : ICustomFormatter, IFormatProvider
            {
                public string Format(string? format, object? arg, IFormatProvider? formatProvider)
                {
                    if (format == null)
                    {
                        throw new ArgumentNullException(nameof(format));
                    }

                    if (arg == null)
                    {
                        throw new ArgumentNullException(nameof(arg));
                    }

                    string[] parts = format.Split(':');

                    if (parts[0] == "P")
                    {
                        int p = int.TryParse(arg.ToString(), out int a) && a == 1 ? 2 : 1;
                        return $"{arg} {(parts.Length > p ? parts[p] : string.Empty)}";
                    }
                    if (parts[0] == "PN")
                    {
                        int p = int.TryParse(arg.ToString(), out int a) && a == 1 ? 2 : 1;
                        return $"{arg:N0} {(parts.Length > p ? parts[p] : string.Empty)}";
                    }

                    throw new FormatException();
                }

                public object? GetFormat(Type? formatType)
                    => formatType == typeof(ICustomFormatter) ? this : null;
            }
        }
    }
}
