///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.epl.datetime.calop
{
	public class CalendarOpPlusFastAddHelper
    {
	    private static bool DEBUG = false;
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

	    public static CalendarOpPlusFastAddResult ComputeNextDue(
	        long currentTime,
	        TimePeriod timePeriod,
	        DateTime reference)
        {
            if (reference.TimeInMillis() > currentTime)
            {
                return new CalendarOpPlusFastAddResult(0, reference);
            }

            // add one time period
            var work = reference;
            if (DEBUG && Log.IsDebugEnabled)
            {
                Log.Debug("Work date is " + work.Print());
            }

            work = CalendarOpPlusMinus.ActionSafeOverflow(work, 1, timePeriod);
            long inMillis = work.TimeInMillis();
            if (inMillis > currentTime)
            {
                return new CalendarOpPlusFastAddResult(1, work);
            }
            if (DEBUG && Log.IsDebugEnabled)
            {
                Log.Debug("Work date is " + work.Print());
            }

            long factor = 1;

            // determine multiplier
            long deltaCurrentToStart = currentTime - reference.TimeInMillis();
            long deltaAddedOne = work.TimeInMillis() - reference.TimeInMillis();
            double multiplierDbl = (deltaCurrentToStart/deltaAddedOne) - 1;
            long multiplierRoundedLong = (long) multiplierDbl;

            // handle integer max
            while (multiplierRoundedLong > int.MaxValue)
            {
                work = CalendarOpPlusMinus.ActionSafeOverflow(work, int.MaxValue, timePeriod);
                factor += int.MaxValue;
                multiplierRoundedLong -= int.MaxValue;
                if (DEBUG && Log.IsDebugEnabled)
                {
                    Log.Debug("Work date is " + work.Print() + " factor " + factor);
                }
            }

            // add
            int multiplierRoundedInt = (int) multiplierRoundedLong;
            work = CalendarOpPlusMinus.ActionSafeOverflow(work, multiplierRoundedInt, timePeriod);
            factor += multiplierRoundedInt;

            // if below, add more
            if (work.TimeInMillis() <= currentTime)
            {
                while (work.TimeInMillis() <= currentTime)
                {
                    work = CalendarOpPlusMinus.ActionSafeOverflow(work, 1, timePeriod);
                    factor += 1;
                    if (DEBUG && Log.IsDebugEnabled)
                    {
                        Log.Debug("Work date is " + work.Print() + " factor " + factor);
                    }
                }
                return new CalendarOpPlusFastAddResult(factor, work);
            }

            // we are over
            while (work.TimeInMillis() > currentTime)
            {
                work = CalendarOpPlusMinus.ActionSafeOverflow(work, -1, timePeriod);
                factor -= 1;
                if (DEBUG && Log.IsDebugEnabled)
                {
                    Log.Debug("Work date is " + work.Print() + " factor " + factor);
                }
            }
            work = CalendarOpPlusMinus.ActionSafeOverflow(work, 1, timePeriod);
            if (DEBUG && Log.IsDebugEnabled)
            {
                Log.Debug("Work date is " + work.Print() + " factor " + factor);
            }
            return new CalendarOpPlusFastAddResult(factor + 1, work);
        }
    }
} // end of namespace
