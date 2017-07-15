///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.time;

namespace com.espertech.esper.epl.datetime.calop
{
    public class CalendarOpPlusFastAddHelper
    {
        private const bool DEBUG = false;

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static CalendarOpPlusFastAddResult ComputeNextDue(
            long currentTime,
            TimePeriod timePeriod,
            DateTimeEx reference,
            TimeAbacus timeAbacus,
            long remainder)
        {
            if (timeAbacus.CalendarGet(reference, remainder) > currentTime)
            {
                return new CalendarOpPlusFastAddResult(0, reference);
            }

            // add one time period
            DateTimeEx work = reference.Clone();
            if (DEBUG && Log.IsDebugEnabled)
            {
                Log.Debug("Work date is " + work);
            }

            CalendarOpPlusMinus.ActionSafeOverflow(work, 1, timePeriod);
            long inMillis = timeAbacus.CalendarGet(work, remainder);
            if (inMillis > currentTime)
            {
                return new CalendarOpPlusFastAddResult(1, work);
            }
            if (DEBUG && Log.IsDebugEnabled)
            {
                Log.Debug("Work date is {0}", work);
            }

            long factor = 1;

            // determine multiplier
            long refTime = timeAbacus.CalendarGet(reference, remainder);
            long deltaCurrentToStart = currentTime - refTime;
            long deltaAddedOne = timeAbacus.CalendarGet(work, remainder) - refTime;
            double multiplierDbl = (deltaCurrentToStart/deltaAddedOne) - 1;
            var multiplierRoundedLong = (long) multiplierDbl;

            // handle integer max
            while (multiplierRoundedLong > Int32.MaxValue)
            {
                CalendarOpPlusMinus.ActionSafeOverflow(work, Int32.MaxValue, timePeriod);
                factor += Int32.MaxValue;
                multiplierRoundedLong -= Int32.MaxValue;
                if (DEBUG && Log.IsDebugEnabled)
                {
                    Log.Debug("Work date is {0} factor {1}", work, factor);
                }
            }

            // add
            var multiplierRoundedInt = (int) multiplierRoundedLong;
            CalendarOpPlusMinus.ActionSafeOverflow(work, multiplierRoundedInt, timePeriod);
            factor += multiplierRoundedInt;

            // if below, add more
            if (timeAbacus.CalendarGet(work, remainder) <= currentTime)
            {
                while (timeAbacus.CalendarGet(work, remainder) <= currentTime)
                {
                    CalendarOpPlusMinus.ActionSafeOverflow(work, 1, timePeriod);
                    factor += 1;
                    if (DEBUG && Log.IsDebugEnabled)
                    {
                        Log.Debug("Work date is {0} factor {1}", work, factor);
                    }
                }
                return new CalendarOpPlusFastAddResult(factor, work);
            }

            // we are over
            while (timeAbacus.CalendarGet(work, remainder) > currentTime)
            {
                CalendarOpPlusMinus.ActionSafeOverflow(work, -1, timePeriod);
                factor -= 1;
                if (DEBUG && Log.IsDebugEnabled)
                {
                    Log.Debug("Work date is {0} factor {1}", work, factor);
                }
            }
            CalendarOpPlusMinus.ActionSafeOverflow(work, 1, timePeriod);
            if (DEBUG && Log.IsDebugEnabled)
            {
                Log.Debug("Work date is {0} factor {1}", work, factor);
            }
            return new CalendarOpPlusFastAddResult(factor + 1, work);
        }
    }
} // end of namespace