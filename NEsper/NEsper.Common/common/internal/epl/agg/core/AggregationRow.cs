///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
            int column,
            object value);

        void LeaveAgg(
            int column,
            object value);

        void EnterAccess(
            int column,
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext);

        void LeaveAccess(
            int column,
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext);

        object GetAccessState(int column);

        void Clear();

        object GetValue(
            int column,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext);

        ICollection<EventBean> GetCollectionOfEvents(
            int column,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context);

        EventBean GetEventBean(
            int column,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context);

        ICollection<object> GetCollectionScalar(
            int column,
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