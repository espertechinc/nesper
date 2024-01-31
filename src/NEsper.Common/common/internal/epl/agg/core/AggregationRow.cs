///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    public interface AggregationRow
    {
        void ApplyEnter(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext);

        void ApplyLeave(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext);

        void EnterAgg(
            int scol,
            object value);

        void LeaveAgg(
            int scol,
            object value);

        void EnterAccess(
            int scol,
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext);

        void LeaveAccess(
            int scol,
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext);

        object GetAccessState(int scol);

        void Clear();

        object GetValue(
            int vcol,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext);

        ICollection<EventBean> GetCollectionOfEvents(
            int vcol,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context);

        EventBean GetEventBean(
            int vcol,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context);

        ICollection<object> GetCollectionScalar(
            int vcol,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context);

        void IncreaseRefcount();

        void DecreaseRefcount();

        long GetRefcount();

        long GetLastUpdateTime();

        void SetLastUpdateTime(long currentTime);

        void Reset(int column);
    }
} // end of namespace