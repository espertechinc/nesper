///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.epl.expression.time
{
    public interface ExprTimePeriodEvalDeltaConst
    {
        long DeltaMillisecondsAdd(long fromTime);
        long DeltaMillisecondsSubtract(long fromTime);
        ExprTimePeriodEvalDeltaResult DeltaMillisecondsAddWReference(long fromTime, long reference);
        bool EqualsTimePeriod(ExprTimePeriodEvalDeltaConst timeDeltaComputation);
    }
}
