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
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.events;
using com.espertech.esper.events.arr;

namespace com.espertech.esper.epl.table.strategy
{
    public class ExprTableEvalStrategyGroupByTopLevelMulti : ExprTableEvalStrategyGroupByTopLevelBase
    {
        private readonly ExprEvaluator[] _groupExpr;

        public ExprTableEvalStrategyGroupByTopLevelMulti(ILockable @lock, IDictionary<Object, ObjectArrayBackedEventBean> aggregationState, IDictionary<String, TableMetadataColumn> items, ExprEvaluator[] groupExpr)
            : base(@lock, aggregationState, items)
        {
            _groupExpr = groupExpr;
        }
    
        public override object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            object groupKey = ExprTableEvalStrategyGroupByAccessMulti.GetKey(_groupExpr, eventsPerStream, isNewData, exprEvaluatorContext);
            return base.EvaluateInternal(groupKey, eventsPerStream, isNewData, exprEvaluatorContext);
        }

        public override object[] EvaluateTypableSingle(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            object groupKey = ExprTableEvalStrategyGroupByAccessMulti.GetKey(_groupExpr, eventsPerStream, isNewData, context);
            return base.EvaluateTypableSingleInternal(groupKey, eventsPerStream, isNewData, context);
        }
    }
}
