///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.util;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.pattern.observer
{
    public class TimerScheduleSpec
    {
        private long? optionalRemainder;

        public TimerScheduleSpec(
            DateTimeEx optionalDate,
            long? optionalRemainder,
            long? optionalRepeatCount,
            TimePeriod optionalTimePeriod)
        {
            OptionalDate = optionalDate;
            this.optionalRemainder = optionalRemainder;
            OptionalRepeatCount = optionalRepeatCount;
            OptionalTimePeriod = optionalTimePeriod;
        }

        public DateTimeEx OptionalDate { get; }

        public long? OptionalRepeatCount { get; }

        public TimePeriod OptionalTimePeriod { get; }

        public long? OptionalRemainder {
            get => optionalRemainder;
            set => optionalRemainder = value;
        }
    }
} // end of namespace