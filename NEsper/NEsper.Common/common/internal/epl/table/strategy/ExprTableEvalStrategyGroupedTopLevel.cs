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
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.epl.table.strategy
{
    public class ExprTableEvalStrategyGroupedTopLevel : ExprTableEvalStrategyGroupedBase
    {
        public ExprTableEvalStrategyGroupedTopLevel(TableAndLockProviderGrouped provider, ExprTableEvalStrategyFactory factory)
             : base(provider, factory)
        {
        }

        public override object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            object groupKey = factory.GroupKeyEval.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
            ObjectArrayBackedEventBean row = LockTableReadAndGet(groupKey, exprEvaluatorContext);
            if (row == null)
            {
                return null;
            }
            return ExprTableEvalStrategyUtil.EvalMap(row, ExprTableEvalStrategyUtil.GetRow(row), factory.Table.MetaData.Columns, eventsPerStream, isNewData, exprEvaluatorContext);
        }

        public override object[] EvaluateTypableSingle(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            object groupKey = factory.GroupKeyEval.Evaluate(eventsPerStream, isNewData, context);
            ObjectArrayBackedEventBean row = LockTableReadAndGet(groupKey, context);
            if (row == null)
            {
                return null;
            }
            return ExprTableEvalStrategyUtil.EvalTypable(row, ExprTableEvalStrategyUtil.GetRow(row), factory.Table.MetaData.Columns, eventsPerStream, isNewData, context);
        }

        public override ICollection<EventBean> EvaluateGetROCollectionEvents(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            return null;
        }

        public override EventBean EvaluateGetEventBean(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            return null;
        }

        public override ICollection<object> EvaluateGetROCollectionScalar(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            return null;
        }
    }
} // end of namespace