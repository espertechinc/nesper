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
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.table;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.table.strategy
{
    public class ExprTableEvalStrategyUngroupedTopLevel
        : ExprTableEvalStrategyUngroupedBase
        , ExprTableAccessEvalStrategy
    {
        private readonly IDictionary<string, TableMetadataColumn> items;

        public ExprTableEvalStrategyUngroupedTopLevel(
            TableAndLockProviderUngrouped provider,
            IDictionary<string, TableMetadataColumn> items)
            : base(provider)
        {
            this.items = items;
        }

        public object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            var @event = LockTableReadAndGet(context);
            if (@event == null)
            {
                return null;
            }
            var row = ExprTableEvalStrategyUtil.GetRow(@event);
            return ExprTableEvalStrategyUtil.EvalMap(@event, row, items, eventsPerStream, isNewData, context);
        }

        public object[] EvaluateTypableSingle(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            var @event = LockTableReadAndGet(context);
            if (@event == null)
            {
                return null;
            }
            var row = ExprTableEvalStrategyUtil.GetRow(@event);
            return ExprTableEvalStrategyUtil.EvalTypable(@event, row, items, eventsPerStream, isNewData, context);
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
    }
} // end of namespace
