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

namespace com.espertech.esper.epl.table.strategy
{
    public class ExprTableEvalStrategyGroupByTopLevelSingle : ExprTableEvalStrategyGroupByTopLevelBase
    {
        private readonly ExprEvaluator _groupExpr;

        public ExprTableEvalStrategyGroupByTopLevelSingle(TableAndLockProviderGrouped provider, IDictionary<String, TableMetadataColumn> items, ExprEvaluator groupExpr) 
            : base(provider, items)
        {
            this._groupExpr = groupExpr;
        }

        public override object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            object groupKey = _groupExpr.Evaluate(new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext));
            return base.EvaluateInternal(groupKey, eventsPerStream, isNewData, exprEvaluatorContext);
        }

        public override object[] EvaluateTypableSingle(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            object groupKey = _groupExpr.Evaluate(new EvaluateParams(eventsPerStream, isNewData, context));
            return base.EvaluateTypableSingleInternal(groupKey, eventsPerStream, isNewData, context);
        }
    }
}
