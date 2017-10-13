///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.epl.expression.time
{
    public interface ExprTimePeriodEvalDeltaConst : ExprTimePeriodEvalDeltaConstFactory
    {
        long DeltaAdd(long fromTime);

        long DeltaSubtract(long fromTime);

        ExprTimePeriodEvalDeltaResult DeltaAddWReference(long fromTime, long reference);

        bool EqualsTimePeriod(ExprTimePeriodEvalDeltaConst timeDeltaComputation);
    }
} // end of namespace
