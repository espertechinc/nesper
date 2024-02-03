///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.expression.time.eval
{
    public interface TimePeriodProvide
    {
        long DeltaAdd(
            long fromTime,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context);

        long DeltaSubtract(
            long fromTime,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context);

        TimePeriodDeltaResult DeltaAddWReference(
            long fromTime,
            long reference,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context);
    }
} // end of namespace