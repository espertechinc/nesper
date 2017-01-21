///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.table;
using com.espertech.esper.epl.table.mgmt;

namespace com.espertech.esper.epl.table.strategy
{
    public class ExprTableEvalStrategyGroupByKeys
        : ExprTableEvalStrategyGroupByBase,
            ExprTableAccessEvalStrategy
    {
        public ExprTableEvalStrategyGroupByKeys(TableAndLockProviderGrouped provider)
            : base(provider)
        {
        }

        public object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            TableStateInstanceGrouped grouped = LockTableRead(context);
            ICollection<object> keys = grouped.GroupKeys;
            return keys.ToArray();
        }

        public ICollection<EventBean> EvaluateGetROCollectionEvents(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return null;
        }

        public EventBean EvaluateGetEventBean(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            return null;
        }

        public ICollection<object> EvaluateGetROCollectionScalar(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return null;
        }

        public object[] EvaluateTypableSingle(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            return null;
        }
    }
} // end of namespace
