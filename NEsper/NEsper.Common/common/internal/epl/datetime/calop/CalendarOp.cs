///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.datetime.calop
{
    public interface CalendarOp
    {
        DateTimeEx Evaluate(
            DateTimeEx dateTimeEx,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context);

        DateTimeOffset Evaluate(
            DateTimeOffset dateTimeOffset,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context);

        DateTime Evaluate(
            DateTime dateTime,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context);
    }
} // end of namespace