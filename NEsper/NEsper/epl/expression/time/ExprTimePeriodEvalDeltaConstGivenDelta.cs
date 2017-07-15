///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;

namespace com.espertech.esper.epl.expression.time
{
    public class ExprTimePeriodEvalDeltaConstGivenDelta : ExprTimePeriodEvalDeltaConst, ExprTimePeriodEvalDeltaConstFactory {
        private readonly long timeDelta;
    
        public ExprTimePeriodEvalDeltaConstGivenDelta(long timeDelta) {
            this.timeDelta = timeDelta;
        }
    
        internal static long DeltaAddWReference(long current, long reference, long msec) {
            // Example:  current c=2300, reference r=1000, interval i=500, solution s=200
            //
            // int n = ((2300 - 1000) / 500) = 2
            // r + (n + 1) * i - c = 200
            //
            // Negative example:  current c=2300, reference r=4200, interval i=500, solution s=400
            // int n = ((2300 - 4200) / 500) = -3
            // r + (n + 1) * i - c = 4200 - 3*500 - 2300 = 400
            //
            long n = (current - reference) / msec;
            if (reference > current) { // References in the future need to deduct one window
                n--;
            }
            long solution = reference + (n + 1) * msec - current;
            if (solution == 0) {
                return msec;
            }
            return solution;
        }
    
        public ExprTimePeriodEvalDeltaConst Make(string validateMsgName, string validateMsgValue, AgentInstanceContext agentInstanceContext) {
            return this;
        }
    
        public bool EqualsTimePeriod(ExprTimePeriodEvalDeltaConst otherComputation) {
            if (otherComputation is ExprTimePeriodEvalDeltaConstGivenDelta) {
                ExprTimePeriodEvalDeltaConstGivenDelta other = (ExprTimePeriodEvalDeltaConstGivenDelta) otherComputation;
                return other.timeDelta == timeDelta;
            }
            return false;
        }
    
        public long DeltaAdd(long fromTime) {
            return timeDelta;
        }
    
        public long DeltaSubtract(long fromTime) {
            return timeDelta;
        }
    
        public ExprTimePeriodEvalDeltaResult DeltaAddWReference(long fromTime, long reference) {
            return new ExprTimePeriodEvalDeltaResult(DeltaAddWReference(fromTime, reference, timeDelta), reference);
        }
    
    }
} // end of namespace
