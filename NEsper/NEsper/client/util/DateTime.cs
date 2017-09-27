///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using java.text;
using java.time;
using java.time.format;

namespace com.espertech.esper.client.util
{
    /// <summary>Utility class for date-time functions.</summary>
    public class DateTime {
    
        /// <summary>The default date-time format.</summary>
        public static readonly string DEFAULT_XMLLIKE_DATE_FORMAT = "yyyy-MM-dd'T'HH:mm:ss.SSS";
    
        /// <summary>The default date-time format with time zone.</summary>
        public static readonly string DEFAULT_XMLLIKE_DATE_FORMAT_WITH_ZONE = "yyyy-MM-dd'T'HH:mm:ss.SSSZ";
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        /// <summary>
        /// Returns a calendar from a given string using the default SimpleDateFormat for parsing.
        /// </summary>
        /// <param name="datestring">to parse</param>
        /// <returns>calendar</returns>
        public static Calendar ToCalendar(string datestring) {
            return ParseGetCal(datestring, new SimpleDateFormat());
        }
    
        /// <summary>
        /// Returns a calendar from a given string using the provided format.
        /// </summary>
        /// <param name="datestring">to parse</param>
        /// <param name="format">to use for parsing</param>
        /// <returns>calendar</returns>
        public static Calendar ToCalendar(string datestring, string format) {
            Date d = Parse(datestring, format);
            Calendar cal = Calendar.Instance;
            cal.TimeInMillis = d.Time;
            return cal;
        }
    
        /// <summary>
        /// Returns a date from a given string using the default SimpleDateFormat for parsing.
        /// </summary>
        /// <param name="datestring">to parse</param>
        /// <returns>date object</returns>
        public static Date ToDate(string datestring) {
            return Parse(datestring);
        }
    
        /// <summary>
        /// Returns a date from a given string using the provided format.
        /// </summary>
        /// <param name="datestring">to parse</param>
        /// <param name="format">to use for parsing</param>
        /// <returns>date object</returns>
        public static Date ToDate(string datestring, string format) {
            return Parse(datestring, format);
        }
    
        /// <summary>
        /// Returns a long-millisecond value from a given string using the default SimpleDateFormat for parsing.
        /// </summary>
        /// <param name="datestring">to parse</param>
        /// <returns>long msec</returns>
        public static long ToMillisec(string datestring) {
            Date date = Parse(datestring);
            if (date == null) {
                return null;
            }
            return Date.Time;
        }
    
        /// <summary>
        /// Returns a long-millisecond value from a given string using the provided format.
        /// </summary>
        /// <param name="datestring">to parse</param>
        /// <param name="format">to use for parsing</param>
        /// <returns>long msec</returns>
        public static long ToMillisec(string datestring, string format) {
            Date date = Parse(datestring, format);
            if (date == null) {
                return null;
            }
            return Date.Time;
        }
    
        /// <summary>
        /// Print the provided date object using the default date format {@link #DEFAULT_XMLLIKE_DATE_FORMAT}
        /// </summary>
        /// <param name="date">should be long, Date or Calendar</param>
        /// <returns>date string</returns>
        public static string Print(Object date) {
            return Print(date, new SimpleDateFormat(DEFAULT_XMLLIKE_DATE_FORMAT));
        }
    
        /// <summary>
        /// Print the provided date object using the default date format {@link #DEFAULT_XMLLIKE_DATE_FORMAT}
        /// </summary>
        /// <param name="date">should be long, Date or Calendar</param>
        /// <returns>date string</returns>
        public static string PrintWithZone(Object date) {
            return Print(date, new SimpleDateFormat(DEFAULT_XMLLIKE_DATE_FORMAT_WITH_ZONE));
        }
    
        private static string Print(Object date, SimpleDateFormat sdf) {
            if (date is long) {
                return Sdf.Format(new Date((long) date));
            }
            if (date is Date) {
                return Sdf.Format((Date) date);
            }
            if (date is Calendar) {
                return Sdf.Format(((Calendar) date).Time);
            }
            throw new IllegalArgumentException("Date format for type '" + date.Class + "' not possible");
        }
    
        /// <summary>
        /// Parse the date-time string using {@link #DEFAULT_XMLLIKE_DATE_FORMAT}.
        /// </summary>
        /// <param name="dateTime">date-time string</param>
        /// <returns>milliseconds</returns>
        public static long ParseDefaultMSec(string dateTime) {
            return Parse(dateTime, new SimpleDateFormat(DEFAULT_XMLLIKE_DATE_FORMAT)).Time;
        }
    
        /// <summary>
        /// Parse the date-time string using {@link #DEFAULT_XMLLIKE_DATE_FORMAT}.
        /// </summary>
        /// <param name="dateTime">date-time string</param>
        /// <returns>LocalDateTime</returns>
        public static LocalDateTime ParseDefaultLocalDateTime(string dateTime) {
            return LocalDateTime.Parse(dateTime, DateTimeFormatter.OfPattern(DEFAULT_XMLLIKE_DATE_FORMAT));
        }
    
        /// <summary>
        /// Parse the date-time string using {@link #DEFAULT_XMLLIKE_DATE_FORMAT} assume System default time zone
        /// </summary>
        /// <param name="dateTime">date-time string</param>
        /// <returns>ZonedDateTime</returns>
        public static ZonedDateTime ParseDefaultZonedDateTime(string dateTime) {
            return ParseDefaultLocalDateTime(dateTime).AtZone(ZoneId.SystemDefault());
        }
    
        /// <summary>
        /// Parse the date-time string using {@link #DEFAULT_XMLLIKE_DATE_FORMAT_WITH_ZONE}.
        /// </summary>
        /// <param name="dateTimeWithZone">date-time string</param>
        /// <returns>milliseconds</returns>
        public static long ParseDefaultMSecWZone(string dateTimeWithZone) {
            return Parse(dateTimeWithZone, new SimpleDateFormat(DEFAULT_XMLLIKE_DATE_FORMAT_WITH_ZONE)).Time;
        }
    
        /// <summary>
        /// Parse the date-time string using {@link #DEFAULT_XMLLIKE_DATE_FORMAT}.
        /// </summary>
        /// <param name="dateTime">date-time string</param>
        /// <returns>date</returns>
        public static Date ParseDefaultDate(string dateTime) {
            return Parse(dateTime, new SimpleDateFormat(DEFAULT_XMLLIKE_DATE_FORMAT));
        }
    
        /// <summary>
        /// Parse the date-time string using {@link #DEFAULT_XMLLIKE_DATE_FORMAT}.
        /// </summary>
        /// <param name="dateTime">date-time string</param>
        /// <returns>calendar</returns>
        public static Calendar ParseDefaultCal(string dateTime) {
            Calendar cal = Calendar.Instance;
            cal.TimeInMillis = ParseDefaultMSec(dateTime);
            return cal;
        }
    
        private static Date Parse(string str) {
            return Parse(str, new SimpleDateFormat());
        }
    
        private static Date Parse(string str, string format) {
            SimpleDateFormat sdf;
            try {
                sdf = new SimpleDateFormat(format);
            } catch (Exception ex) {
                Log.Warn("Error in date format '" + str + "': " + ex.Message, ex);
                return null;
            }
            return Parse(str, sdf);
        }
    
        private static Date Parse(string str, SimpleDateFormat format) {
            Date d;
            try {
                d = format.Parse(str);
            } catch (ParseException e) {
                Log.Warn("Error parsing date '" + str + "' according to format '" + format.ToPattern() + "': " + e.Message, e);
                return null;
            }
            return d;
        }
    
        private static Calendar ParseGetCal(string str, SimpleDateFormat format) {
            Date d = Parse(str, format);
            if (d == null) {
                return null;
            }
            Calendar cal = Calendar.Instance;
            cal.TimeInMillis = d.Time;
            return cal;
        }
    }
} // end of namespace
