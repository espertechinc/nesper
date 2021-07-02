///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.datetime.calop
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
            if (timeAbacus.DateTimeGet(reference, remainder) > currentTime) {
                return new CalendarOpPlusFastAddResult(0, reference);
            }

            // add one time period
            DateTimeEx work = reference.Clone();
            if (DEBUG && Log.IsDebugEnabled) {
                Log.Debug("Work date is " + work);
            }

            CalendarPlusMinusForgeOp.ActionSafeOverflow(work, 1, timePeriod);
            long inMillis = timeAbacus.DateTimeGet(work, remainder);
            if (inMillis > currentTime) {
                return new CalendarOpPlusFastAddResult(1, work);
            }

            if (DEBUG && Log.IsDebugEnabled) {
                Log.Debug("Work date is {0}", work);
            }

            long factor = 1;

            // determine multiplier
            long refTime = timeAbacus.DateTimeGet(reference, remainder);
            long deltaCurrentToStart = currentTime - refTime;
            long deltaAddedOne = timeAbacus.DateTimeGet(work, remainder) - refTime;
            double multiplierDbl = (deltaCurrentToStart / deltaAddedOne) - 1;
            var multiplierRoundedLong = (long) multiplierDbl;

            // handle integer max
            while (multiplierRoundedLong > int.MaxValue) {
                CalendarPlusMinusForgeOp.ActionSafeOverflow(work, int.MaxValue, timePeriod);
                factor += int.MaxValue;
                multiplierRoundedLong -= int.MaxValue;
                if (DEBUG && Log.IsDebugEnabled) {
                    Log.Debug("Work date is {0} factor {1}", work, factor);
                }
            }

            // add
            var multiplierRoundedInt = (int) multiplierRoundedLong;
            CalendarPlusMinusForgeOp.ActionSafeOverflow(work, multiplierRoundedInt, timePeriod);
            factor += multiplierRoundedInt;

            // if below, add more
            if (timeAbacus.DateTimeGet(work, remainder) <= currentTime) {
                while (timeAbacus.DateTimeGet(work, remainder) <= currentTime) {
                    CalendarPlusMinusForgeOp.ActionSafeOverflow(work, 1, timePeriod);
                    factor += 1;
                    if (DEBUG && Log.IsDebugEnabled) {
                        Log.Debug("Work date is {0} factor {1}", work, factor);
                    }
                }

                return new CalendarOpPlusFastAddResult(factor, work);
            }

            // we are over
            while (timeAbacus.DateTimeGet(work, remainder) > currentTime) {
                CalendarPlusMinusForgeOp.ActionSafeOverflow(work, -1, timePeriod);
                factor -= 1;
                if (DEBUG && Log.IsDebugEnabled) {
                    Log.Debug("Work date is {0} factor {1}", work, factor);
                }
            }

            CalendarPlusMinusForgeOp.ActionSafeOverflow(work, 1, timePeriod);
            if (DEBUG && Log.IsDebugEnabled) {
                Log.Debug("Work date is {0} factor {1}", work, factor);
            }

            return new CalendarOpPlusFastAddResult(factor + 1, work);
        }
    }
} // end of namespace