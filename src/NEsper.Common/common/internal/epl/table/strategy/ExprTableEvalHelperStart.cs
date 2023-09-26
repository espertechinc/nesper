///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.table.strategy
{
    public class ExprTableEvalHelperStart
    {
        public static IDictionary<int, ExprTableEvalStrategy> StartTableAccess(
            IDictionary<int, ExprTableEvalStrategyFactory> tableAccesses,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (tableAccesses == null || tableAccesses.IsEmpty()) {
                return EmptyDictionary<int, ExprTableEvalStrategy>.Instance;
            }

            IDictionary<int, ExprTableEvalStrategy> evals =
                new Dictionary<int, ExprTableEvalStrategy>(tableAccesses.Count);
            foreach (var entry in tableAccesses) {
                var table = entry.Value.Table;
                var provider = table.GetStateProvider(
                    exprEvaluatorContext.AgentInstanceId,
                    exprEvaluatorContext.IsWritesToTables);
                var strategy = entry.Value.MakeStrategy(provider);
                evals.Put(entry.Key, strategy);
            }

            return evals;
        }
    }
} // end of namespace