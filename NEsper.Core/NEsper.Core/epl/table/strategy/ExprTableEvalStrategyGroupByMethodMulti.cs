///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.table.strategy
{
    public class ExprTableEvalStrategyGroupByMethodMulti : ExprTableEvalStrategyGroupByMethodBase
    {
        private readonly ExprEvaluator[] _groupExpr;

        public ExprTableEvalStrategyGroupByMethodMulti(TableAndLockProviderGrouped provider, int index, ExprEvaluator[] groupExpr)
            : base(provider, index)
        {
            _groupExpr = groupExpr;
        }
    
        public override object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            object groupKey = ExprTableEvalStrategyGroupByAccessMulti.GetKey(_groupExpr, eventsPerStream, isNewData, exprEvaluatorContext);
            return EvaluateInternal(groupKey, exprEvaluatorContext);
        }
    }
}
