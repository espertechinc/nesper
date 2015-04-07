///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.compat;

namespace com.espertech.esper.support.bean
{
    public class SupportTimeStartEndA
    {
        public SupportTimeStartEndA(String key,
                                    long? msecdateStart,
                                    DateTime? utildateStart,
                                    DateTime? caldateStart,
                                    long? msecdateEnd,
                                    DateTime? utildateEnd,
                                    DateTime? caldateEnd)
        {
            Key = key;
            MsecdateStart = msecdateStart;
            UtildateStart = utildateStart;
            CaldateStart = caldateStart;
            MsecdateEnd = msecdateEnd;
            UtildateEnd = utildateEnd;
            CaldateEnd = caldateEnd;
        }

        public long? MsecdateStart { get; private set; }

        public DateTime? UtildateStart { get; private set; }

        public DateTime? CaldateStart { get; private set; }

        public long? MsecdateEnd { get; private set; }

        public DateTime? UtildateEnd { get; private set; }

        public DateTime? CaldateEnd { get; private set; }

        public string Key { get; set; }

        public static SupportTimeStartEndA Make(String key, String datestr, long duration)
        {
            if (datestr == null)
            {
                return new SupportTimeStartEndA(key, null, null, null, null, null, null);
            }
            // expected : 2002-05-30T09:00:00.000
            long start = DateTimeHelper.ParseDefaultMSec(datestr);
            long end = start + duration;

            return new SupportTimeStartEndA(key, start, SupportDateTime.ToDate(start), SupportDateTime.ToCalendar(start),
                                            end, SupportDateTime.ToDate(end), SupportDateTime.ToCalendar(end));
        }
    }
}