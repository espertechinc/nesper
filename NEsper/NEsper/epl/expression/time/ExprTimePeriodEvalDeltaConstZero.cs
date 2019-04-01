///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.core.context.util;

namespace com.espertech.esper.epl.expression.time
{
    public class ExprTimePeriodEvalDeltaConstZero : ExprTimePeriodEvalDeltaConst
    {
        public static readonly ExprTimePeriodEvalDeltaConstZero INSTANCE = new ExprTimePeriodEvalDeltaConstZero();

        private ExprTimePeriodEvalDeltaConstZero()
        {
        }

        public ExprTimePeriodEvalDeltaConst Make(string validateMsgName, string validateMsgValue, AgentInstanceContext agentInstanceContext)
        {
            return this;
        }

        public bool EqualsTimePeriod(ExprTimePeriodEvalDeltaConst otherComputation)
        {
            return otherComputation is ExprTimePeriodEvalDeltaConstZero;
        }

        public long DeltaAdd(long fromTime)
        {
            return 0;
        }

        public long DeltaSubtract(long fromTime)
        {
            return 0;
        }

        public ExprTimePeriodEvalDeltaResult DeltaAddWReference(long fromTime, long reference)
        {
            return new ExprTimePeriodEvalDeltaResult(ExprTimePeriodEvalDeltaConstGivenDelta.DeltaAddWReference(fromTime, reference, 0), reference);
        }
    }
} // end of namespace