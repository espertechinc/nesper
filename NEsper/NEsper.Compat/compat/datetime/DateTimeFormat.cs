///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Globalization;

namespace com.espertech.esper.compat.datetime
{
    public class DateTimeFormat
    {
        public static readonly DateTimeFormat ISO_DATE_TIME;

        /// <summary>
        /// Initializes the <see cref="DateTimeFormat"/> class.
        /// </summary>
        static DateTimeFormat()
        {
            ISO_DATE_TIME = new DateTimeFormat(
                dateTimeString => ParseDefaultEx(dateTimeString, "s"),
                dateTimeOffset => dateTimeOffset.UtcDateTime.ToString("s", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Gets or sets the parser.
        /// </summary>
        public Func<string, DateTimeEx> Parser { get; set; }

        /// <summary>
        /// Gets or sets the renderer.
        /// </summary>
        public Func<DateTimeOffset, string> Formatter { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeFormat"/> class.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="formatter">The renderer.</param>

        public DateTimeFormat(
            Func<string, DateTimeEx> parser,
            Func<DateTimeOffset, string> formatter)
        {
            Parser = parser;
            Formatter = formatter;
        }

        /// <summary>
        /// Parses the specified date time string.
        /// </summary>
        /// <param name="dateTimeString">The date time string.</param>
        /// <returns></returns>
        public DateTimeEx Parse(string dateTimeString)
        {
            return Parser.Invoke(dateTimeString);
        }

        /// <summary>
        /// Formats (renders) the specified date time.
        /// </summary>
        /// <param name="dateTimeEx">The date time.</param>
        /// <returns></returns>
        public string Format(DateTimeEx dateTimeEx)
        {
            return Formatter.Invoke(dateTimeEx.DateTime);
        }

        /// <summary>
        /// Formats (renders) the specified date time.
        /// </summary>
        /// <param name="dateTimeOffset">The date time.</param>
        /// <returns></returns>
        public string Format(DateTimeOffset dateTimeOffset)
        {
            return Formatter.Invoke(dateTimeOffset);
        }

        /// <summary>
        /// Formats the specified date time.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        public string Format(DateTime dateTime)
        {
            return Formatter.Invoke(dateTime);
        }

        /// <summary>
        /// Returns a DateTimeFormat for the specified date time format.
        /// </summary>
        /// <param name="dateTimeFormat">The date time format.</param>
        /// <returns></returns>
        public static DateTimeFormat For(string dateTimeFormat)
        {
            return new DateTimeFormat(
                dateTimeString => ParseDefaultEx(dateTimeString, dateTimeFormat),
                dateTimeOffset => dateTimeOffset.ToString(dateTimeFormat, CultureInfo.InvariantCulture));
        }

        public static DateTimeEx ParseDefaultEx(string dateTimeString, string dateTimeFormat)
        {
            DateTimeOffset dateTime;

            var timeZone = dateTimeString.EndsWith("Z") ? TimeZoneInfo.Utc : TimeZoneInfo.Local;

            if (DateTimeOffset.TryParseExact(dateTimeString, dateTimeFormat, null, DateTimeStyles.None, out dateTime))
            {
                return new DateTimeEx(dateTime, timeZone);
            }

            throw new ArgumentException("unable to parse message");
        }
    }
}
