///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.client.module;
using com.espertech.esper.compat;
using com.espertech.esper.compat.calendar;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.client.util
{
	/// <summary>
	/// Utility class for date-time functions.
	/// </summary>
	public class DateTimeUtil {

	    /// <summary>
	    /// The default date-time format.
	    /// </summary>
	    public const string DEFAULT_XMLLIKE_DATE_FORMAT = "yyyy-MM-dd'T'HH:mm:ss.SSS";

	    /// <summary>
	    /// The default date-time format with time zone.
	    /// </summary>
	    public const string DEFAULT_XMLLIKE_DATE_FORMAT_WITH_ZONE = "yyyy-MM-dd'T'HH:mm:ss.SSSZ";

	    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

	    /// <summary>
	    /// Returns a calendar from a given string using the default SimpleDateFormat for parsing.
	    /// </summary>
	    /// <param name="datestring">to parse</param>
	    /// <returns>calendar</returns>
	    public static DateTimeEx ToDateTimeEx(string datestring) {
	        return ParseGetDateTimeEx(datestring, new SimpleDateFormat());
	    }

	    /// <summary>
	    /// Returns a calendar from a given string using the provided format.
	    /// </summary>
	    /// <param name="datestring">to parse</param>
	    /// <param name="format">to use for parsing</param>
	    /// <returns>calendar</returns>
	    public static DateTimeEx ToDateTimeEx(string datestring, string format) {
	        Date d = Parse(datestring, format);
	        DateTimeEx cal = DateTimeEx.NowUtc();
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
	    public static long? ToMillisec(string datestring) {
	        var date = Parse(datestring);
	        if (date == null) {
	            return null;
	        }
	        return date.Time;
	    }

	    /// <summary>
	    /// Returns a long-millisecond value from a given string using the provided format.
	    /// </summary>
	    /// <param name="datestring">to parse</param>
	    /// <param name="format">to use for parsing</param>
	    /// <returns>long msec</returns>
	    public static long? ToMillisec(string datestring, string format) {
	        var date = Parse(datestring, format);
	        if (date == null) {
	            return null;
	        }
	        return date.Time;
	    }

	    /// <summary>
	    /// Print the provided date object using the default date format <seealso cref="DEFAULT_XMLLIKE_DATE_FORMAT" /></summary>
	    /// <param name="date">should be long, Date or Calendar</param>
	    /// <returns>date string</returns>
	    public static string Print(object date) {
	        return Print(date, new SimpleDateFormat(DEFAULT_XMLLIKE_DATE_FORMAT));
	    }

	    /// <summary>
	    /// Print the provided date object using the default date format <seealso cref="DEFAULT_XMLLIKE_DATE_FORMAT" /></summary>
	    /// <param name="date">should be long, Date or Calendar</param>
	    /// <returns>date string</returns>
	    public static string PrintWithZone(object date) {
	        return Print(date, new SimpleDateFormat(DEFAULT_XMLLIKE_DATE_FORMAT_WITH_ZONE));
	    }

	    private static string Print(object date, SimpleDateFormat sdf) {
	        if (date is long?) {
	            return sdf.Format(new Date((long?) date));
	        }
	        if (date is Date) {
	            return sdf.Format((Date) date);
	        }
	        if (date is DateTimeEx) {
	            return sdf.Format(((DateTimeEx) date).Time);
	        }
	        throw new ArgumentException("Date format for type '" + date.GetType() + "' not possible");
	    }

	    /// <summary>
	    /// Parse the date-time string using <seealso cref="DEFAULT_XMLLIKE_DATE_FORMAT" />.
	    /// </summary>
	    /// <param name="dateTime">date-time string</param>
	    /// <returns>milliseconds</returns>
	    public static long ParseDefaultMSec(string dateTime)
	    {
	        return Parse(dateTime, new SimpleDateFormat(DEFAULT_XMLLIKE_DATE_FORMAT)).Time;
	    }

	    /// <summary>
	    /// Parse the date-time string using <seealso cref="DEFAULT_XMLLIKE_DATE_FORMAT" />.
	    /// </summary>
	    /// <param name="dateTime">date-time string</param>
	    /// <returns>LocalDateTime</returns>
	    public static DateTimeOffset ParseDefaultLocalDateTime(string dateTime)
	    {
	        var dateTimeParser = DateTimeFormatter.ParserFor(DEFAULT_XMLLIKE_DATE_FORMAT);
	        var dateTimeEx = dateTimeParser.Parse(dateTime);
	        return dateTimeEx.DateTime;
	    }

	    /// <summary>
	    /// Parse the date-time string using <seealso cref="DEFAULT_XMLLIKE_DATE_FORMAT_WITH_ZONE" />.
	    /// </summary>
	    /// <param name="dateTimeWithZone">date-time string</param>
	    /// <returns>milliseconds</returns>
	    public static long ParseDefaultMSecWZone(string dateTimeWithZone) {
	        return Parse(dateTimeWithZone, new SimpleDateFormat(DEFAULT_XMLLIKE_DATE_FORMAT_WITH_ZONE)).Time;
	    }

	    /// <summary>
	    /// Parse the date-time string using <seealso cref="DEFAULT_XMLLIKE_DATE_FORMAT" />.
	    /// </summary>
	    /// <param name="dateTime">date-time string</param>
	    /// <returns>date</returns>
	    public static Date ParseDefaultDate(string dateTime) {
	        return Parse(dateTime, new SimpleDateFormat(DEFAULT_XMLLIKE_DATE_FORMAT));
	    }

	    /// <summary>
	    /// Parse the date-time string using <seealso cref="DEFAULT_XMLLIKE_DATE_FORMAT" />.
	    /// </summary>
	    /// <param name="dateTime">date-time string</param>
	    /// <returns>calendar</returns>
	    public static DateTimeEx ParseDefaultCal(string dateTime) {
	        DateTimeEx cal = DateTimeEx.GetInstance();
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

	    private static DateTimeEx Parse(string str, SimpleDateFormat format) {
	        DateTimeEx d;
	        try {
	            d = format.Parse(str);
	        } catch (ParseException e) {
	            Log.Warn("Error parsing date '" + str + "' according to format '" + format.ToPattern() + "': " + e.Message, e);
	            return null;
	        }
	        return d;
	    }

	    private static DateTimeEx ParseGetDateTimeEx(string str, SimpleDateFormat format) {
	        Date d = Parse(str, format);
	        if (d == null) {
	            return null;
	        }
	        DateTimeEx cal = DateTimeEx.NowUtc();
	        cal.TimeInMillis = d.Time;
	        return cal;
	    }
	}
} // end of namespace