///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.epl.expression.time.adder;

namespace com.espertech.esper.common.@internal.epl.expression.time.node
{
    public class ExprTimePeriodUtil
    {
        public static bool ValidateTime(
            object timeInSeconds,
            TimeAbacus timeAbacus)
        {
            return timeInSeconds != null && timeAbacus.DeltaForSecondsNumber(timeInSeconds) >= 1;
        }

        public static string GetTimeInvalidMsg(
            string validateMsgName,
            string validateMsgValue,
            object timeInSeconds)
        {
            return validateMsgName + " " + validateMsgValue + " requires a size of at least 1 msec but received " +
                   timeInSeconds;
        }

        public static int FindIndexMicroseconds(TimePeriodAdder[] adders)
        {
            var indexMicros = -1;
            for (var i = 0; i < adders.Length; i++) {
                if (adders[i].IsMicroseconds) {
                    indexMicros = i;
                    break;
                }
            }

            return indexMicros;
        }
    }
} // end of namespace