///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.table;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.table.strategy
{
    public abstract class ExprTableEvalStrategyGroupByMethodBase
        : ExprTableEvalStrategyGroupByBase
            ,
            ExprTableAccessEvalStrategy
    {
        private readonly int _index;

        public abstract object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext);

        protected ExprTableEvalStrategyGroupByMethodBase(TableAndLockProviderGrouped provider, int index)
            : base(provider)
        {
            _index = index;
        }

        protected object EvaluateInternal(object groupKey, ExprEvaluatorContext context)
        {
            var row = LockTableReadAndGet(groupKey, context);
            if (row == null)
            {
                return null;
            }
            return ExprTableEvalStrategyUtil.EvalMethodGetValue(ExprTableEvalStrategyUtil.GetRow(row), _index);
        }

        public object[] EvaluateTypableSingle(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            return null;
        }

        public ICollection<EventBean> EvaluateGetROCollectionEvents(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            return null;
        }

        public EventBean EvaluateGetEventBean(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            return null;
        }

        public ICollection<object> EvaluateGetROCollectionScalar(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            return null;
        }
    }
}
