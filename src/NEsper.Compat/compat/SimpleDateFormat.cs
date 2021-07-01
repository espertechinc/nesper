///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Globalization;

namespace com.espertech.esper.compat
{
    public class SimpleDateFormat : DateFormat
    {
        /// <summary>
        /// Initialize a new instance of the <see cref="SimpleDateFormat"/> class.
        /// </summary>
        public SimpleDateFormat()
        {
            FormatString = "s";
            FormatProvider = CultureInfo.InvariantCulture;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SimpleDateFormat" /> class.
        /// </summary>
        /// <param name="formatString">The date format string.</param>
        public SimpleDateFormat(string formatString)
        {
            FormatString = formatString;
            FormatProvider = CultureInfo.CurrentCulture;
        }

        /// <summary>Initializes a new instance of the <see cref="SimpleDateFormat" /> class.</summary>
        /// <param name="formatString">The date format string.</param>
        /// <param name="formatProvider">The format provider.</param>
        public SimpleDateFormat(
            string formatString,
            IFormatProvider formatProvider)
        {
            FormatString = formatString;
            FormatProvider = formatProvider;
        }

        /// <summary>
        /// Returns an instance of a <see cref="SimpleDateFormat" /> class.
        /// </summary>
        /// <returns></returns>
        public static SimpleDateFormat GetInstance()
        {
            return new SimpleDateFormat("s", CultureInfo.InvariantCulture);
        } 

        /// <summary>
        ///     Returns the date format string.
        /// </summary>
        public string FormatString { get; }

        /// <summary>Gets the format provider.</summary>
        /// <value>The format provider.</value>
        public IFormatProvider FormatProvider { get; }

        /// <summary>
        /// Formats the specified date time.
        /// </summary>
        /// <param name="timeInMillis">The time in milliseconds.</param>
        /// <returns></returns>
        public string Format(long? timeInMillis)
        {
            if (timeInMillis == null) {
                return null;
            }
            return Format(timeInMillis.Value.TimeFromMillis());
        }

        /// <summary>
        ///     Formats the specified date time.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        public string Format(DateTime? dateTime)
        {
            return dateTime?.ToString(FormatString, FormatProvider);
        }

        /// <summary>
        ///     Formats the specified date time.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        public string Format(DateTimeOffset? dateTime)
        {
            return dateTime?.ToString(FormatString, FormatProvider);
        }

        /// <summary>
        ///     Formats the specified date time.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        public string Format(DateTimeEx dateTime)
        {
            return dateTime
                .DateTime
                .ToString(FormatString, FormatProvider);
        }

        /// <summary>
        ///     Parses the specified date time string.
        /// </summary>
        /// <param name="dateTimeString">The date time string.</param>
        /// <returns></returns>
        public DateTimeEx Parse(string dateTimeString)
        {
            var timeZone = TimeZoneInfo.Utc;
            
            if (DateTime.TryParseExact(
                dateTimeString,
                FormatString,
                null,
                DateTimeStyles.None,
                out var dateTime)) {
                if (dateTime.Kind == DateTimeKind.Unspecified) {
                    return new DateTimeEx(new DateTimeOffset(dateTime, TimeSpan.Zero), timeZone);
                }
            }
            
            if (DateTimeOffset.TryParseExact(
                dateTimeString,
                FormatString,
                null,
                DateTimeStyles.None,
                out var dateTimeOffset)) {
                return new DateTimeEx(dateTimeOffset, timeZone);
            }
            
            throw new ArgumentException(
                $"Exception parsing date '{dateTimeString}' format '{FormatString}': Unparseable date: \"{dateTimeString}\"' in text");
        }
    }
}