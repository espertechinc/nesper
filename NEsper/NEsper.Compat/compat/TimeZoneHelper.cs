///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.compat
{
    public class TimeZoneHelper
    {
        private static readonly ILockable TimeZoneInfoLock = new MonitorLock(60000);
        private static readonly IDictionary<string, TimeZoneInfo> TimeZoneInfoDictionary =
            new Dictionary<string, TimeZoneInfo>();

        public static TimeZoneInfo GetTimeZoneInfoOrDefault(string specOrId)
        {
            if (string.IsNullOrWhiteSpace(specOrId)) {
                return TimeZoneInfo.Utc;
            } else if (specOrId == "Z") {
                return TimeZoneInfo.Utc;
            }
            else {
                return GetTimeZoneInfo(specOrId);
            }
        }

        public static TimeZoneInfo GetTimeZoneInfo(string specOrId)
        {
            using (TimeZoneInfoLock.Acquire())
            {
                var timeZoneInfo = TimeZoneInfoDictionary.Get(specOrId);
                if (timeZoneInfo == null)
                {
                    try
                    {
                        timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(specOrId);
                    }
                    catch (TimeZoneNotFoundException)
                    {
                        var regex = new Regex("^(GMT|)([+-])(\\d{1,2}):(\\d{2})$");
                        var match = regex.Match(specOrId);
                        if (match == Match.Empty)
                        {
                            throw;
                        }

                        var prefix = match.Groups[1].Value;
                        if (string.IsNullOrWhiteSpace(prefix)) {
                            specOrId = "GMT" + specOrId;
                        }

                        var multiplier = match.Groups[2].Value == "+" ? 1 : -1;
                        var offset = new TimeSpan(
                            multiplier*int.Parse(match.Groups[3].Value), // hours
                            multiplier*int.Parse(match.Groups[4].Value),
                            0);

                        timeZoneInfo = TimeZoneInfo.CreateCustomTimeZone(specOrId, offset, specOrId, specOrId);
                    }

                    TimeZoneInfoDictionary[specOrId] = timeZoneInfo;
                }

                return timeZoneInfo;
            }
        }
    }
}
