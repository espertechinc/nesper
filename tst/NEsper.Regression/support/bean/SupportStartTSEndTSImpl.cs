///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.datetime;

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportStartTSEndTSImpl : SupportStartTSEndTSInterface
    {
        public SupportStartTSEndTSImpl(
            string datestr,
            long duration)
        {
            StartTS = DateTimeParsingFunctions.ParseDefaultMSec(datestr);
            EndTS = StartTS + duration;
        }

        public long StartTS { get; }

        public long EndTS { get; }
    }
} // end of namespace