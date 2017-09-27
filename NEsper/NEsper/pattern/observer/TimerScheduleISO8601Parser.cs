///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text.RegularExpressions;

using com.espertech.esper.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.schedule;

namespace com.espertech.esper.pattern.observer
{
    /// <summary>
    /// Factory for ISO8601 repeating interval observers that indicate truth when a time point was reached.
    /// </summary>
    public class TimerScheduleISO8601Parser
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static TimerScheduleSpec Parse(string iso)
        {
            if (iso == null)
            {
                throw new ScheduleParameterException("Received a null value");
            }
            iso = iso.Trim();
            if (string.IsNullOrEmpty(iso))
            {
                throw new ScheduleParameterException("Received an empty string");
            }

            var split = iso.Split('/');

            long? optionalRepeats = null;
            DateTimeEx optionalDate = null;
            TimePeriod optionalTimePeriod = null;

            try
            {
                if (iso.Equals("/"))
                {
                    throw new ScheduleParameterException("Invalid number of parts");
                }
                if (iso.EndsWith("/"))
                {
                    throw new ScheduleParameterException("Missing the period part");
                }

                if (split.Length == 3)
                {
                    optionalRepeats = ParseRepeat(split[0]);
                    optionalDate = ParseDate(split[1]);
                    optionalTimePeriod = ParsePeriod(split[2]);
                }
                else if (split.Length == 2)
                {
                    // there are two forms:
                    // partial-form-1: "R<?>/P<period>"
                    // partial-form-2: "<date>/P<period>"
                    if (string.IsNullOrEmpty(split[0]))
                    {
                        throw new ScheduleParameterException("Expected either a recurrence or a date but received an empty string");
                    }
                    if (split[0][0] == 'R')
                    {
                        optionalRepeats = ParseRepeat(split[0]);
                    }
                    else
                    {
                        optionalDate = ParseDate(split[0]);
                    }
                    optionalTimePeriod = ParsePeriod(split[1]);
                }
                else if (split.Length == 1)
                {
                    // there are two forms:
                    // just date: "<date>"
                    // just period: "P<period>"
                    if (split[0][0] == 'P')
                    {
                        optionalTimePeriod = ParsePeriod(split[0]);
                    }
                    else
                    {
                        optionalDate = ParseDate(split[0]);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ScheduleParameterException("Failed to parse '" + iso + "': " + ex.Message, ex);
            }

            // parse repeating interval
            return new TimerScheduleSpec(optionalDate, null, optionalRepeats, optionalTimePeriod);
        }

        public static DateTimeEx ParseDate(string dateText)
        {
            try
            {
                return DateTimeParser.ParseDefaultEx(dateText);
                //return javax.xml.datatype.DatatypeFactory.NewInstance().NewXMLGregorianCalendar(dateText).ToGregorianCalendar();
            }
            catch (Exception e)
            {
                var message = "Exception parsing date '" + dateText + "', the date is not a supported ISO 8601 date";
                Log.Debug(message, e);
                throw new ScheduleParameterException(message);
            }
        }

        private static long ParseRepeat(string repeat)
        {
            if (repeat[0] != 'R')
            {
                throw new ScheduleParameterException("Invalid repeat '" + repeat + "', expecting 'R' but received '" + repeat[0] + "'");
            }
            long numRepeats = -1;
            if (repeat.Length > 1)
            {
                if (!long.TryParse(repeat.Substring(1), out numRepeats))
                {
                    var message = "Invalid repeat '" + repeat + "', expecting an long-typed value but received '" + repeat.Substring(1) + "'";
                    throw new ScheduleParameterException(message);
                }
            }
            return numRepeats;
        }

        private static TimePeriod ParsePeriod(string period)
        {
            var p = new Regex("^P((\\d+Y)?(\\d+M)?(\\d+W)?(\\d+D)?)?(T(\\d+H)?(\\d+M)?(\\d+S)?)?$");
            var matcher = p.Match(period);
            if (matcher == Match.Empty)
            {
                throw new ScheduleParameterException("Invalid period '" + period + "'");
            }

            var timePeriod = new TimePeriod();
            var indexOfT = period.IndexOf('T');
            if (indexOfT < 1)
            {
                ParsePeriodDatePart(period.Substring(1), timePeriod);
            }
            else
            {
                ParsePeriodDatePart(period.Substring(1, indexOfT - 1), timePeriod);
                ParsePeriodTimePart(period.Substring(indexOfT + 1), timePeriod);
            }

            var largestAbsolute = timePeriod.LargestAbsoluteValue();
            if (largestAbsolute == null || largestAbsolute == 0)
            {
                throw new ScheduleParameterException("Invalid period '" + period + "'");
            }
            return timePeriod;
        }

        private static void ParsePeriodDatePart(string datePart, TimePeriod timePeriod)
        {
            var pattern = new Regex("^(\\d+Y)?(\\d+M)?(\\d+W)?(\\d+D)?$");
            var matcher = pattern.Match(datePart);
            if (matcher == Match.Empty)
            {
                throw new IllegalStateException();
            }
            for (var i = 0; i < matcher.Groups.Count; i++)
            {
                var group = matcher.Groups[i + 1].Value;
                if (string.IsNullOrWhiteSpace(group))
                {
                }
                else if (group.EndsWith("Y"))
                {
                    timePeriod.Years = SafeParsePrefixedInt(group);
                }
                else if (group.EndsWith("M"))
                {
                    timePeriod.Months = SafeParsePrefixedInt(group);
                }
                else if (group.EndsWith("D"))
                {
                    timePeriod.Days = SafeParsePrefixedInt(group);
                }
                else if (group.EndsWith("W"))
                {
                    timePeriod.Weeks = SafeParsePrefixedInt(group);
                }
            }
        }

        private static int? SafeParsePrefixedInt(string group)
        {
            return int.Parse(group.Substring(0, group.Length - 1));
        }

        private static void ParsePeriodTimePart(string timePart, TimePeriod timePeriod)
        {
            var pattern = new Regex("^(\\d+H)?(\\d+M)?(\\d+S)?$");
            var matcher = pattern.Match(timePart);
            if (matcher == Match.Empty)
            {
                throw new IllegalStateException();
            }
            for (var i = 0; i < matcher.Groups.Count; i++)
            {
                string group = matcher.Groups[i + 1].Value;
                if (string.IsNullOrWhiteSpace(group))
                {
                }
                else if (group.EndsWith("H"))
                {
                    timePeriod.Hours = SafeParsePrefixedInt(group);
                }
                else if (group.EndsWith("M"))
                {
                    timePeriod.Minutes = SafeParsePrefixedInt(group);
                }
                else if (group.EndsWith("S"))
                {
                    timePeriod.Seconds = SafeParsePrefixedInt(group);
                }
            }
        }
    }
} // end of namespace
