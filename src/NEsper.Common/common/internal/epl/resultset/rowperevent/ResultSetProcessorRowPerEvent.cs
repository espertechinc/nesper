///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.resultset.select.core;

namespace com.espertech.esper.common.@internal.epl.resultset.rowperevent
{
    public interface ResultSetProcessorRowPerEvent : ResultSetProcessor
    {
        SelectExprProcessor SelectExprProcessor { get; }

        bool HasHavingClause { get; }

        bool EvaluateHavingClause(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext);
    }
} // end of namespace