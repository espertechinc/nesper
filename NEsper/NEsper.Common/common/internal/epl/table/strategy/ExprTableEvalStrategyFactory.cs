///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.table.strategy
{
    public class ExprTableEvalStrategyFactory
    {
        public ExprTableEvalStrategyEnum StrategyEnum { get; set; }

        public Table Table { get; set; }

        public ExprEvaluator GroupKeyEval { get; set; }

        public int AggColumnNum { get; set; } = -1;

        public int PropertyIndex { get; set; }

        public ExprEnumerationGivenEvent OptionalEnumEval { get; set; }

        public AggregationMultiFunctionTableReader AccessAggReader { get; set; }

        public ExprTableEvalStrategy MakeStrategy(TableAndLockProvider provider)
        {
            switch (StrategyEnum) {
                case ExprTableEvalStrategyEnum.UNGROUPED_TOP:
                    return new ExprTableEvalStrategyUngroupedTopLevel((TableAndLockProviderUngrouped) provider, this);

                case ExprTableEvalStrategyEnum.GROUPED_TOP:
                    return new ExprTableEvalStrategyGroupedTopLevel((TableAndLockProviderGrouped) provider, this);

                case ExprTableEvalStrategyEnum.UNGROUPED_AGG_SIMPLE:
                    return new ExprTableEvalStrategyUngroupedAggSimple((TableAndLockProviderUngrouped) provider, this);

                case ExprTableEvalStrategyEnum.GROUPED_AGG_SIMPLE:
                    return new ExprTableEvalStrategyGroupedAggSimple((TableAndLockProviderGrouped) provider, this);

                case ExprTableEvalStrategyEnum.UNGROUPED_PLAINCOL:
                    return new ExprTableEvalStrategyUngroupedProp((TableAndLockProviderUngrouped) provider, this);

                case ExprTableEvalStrategyEnum.GROUPED_PLAINCOL:
                    return new ExprTableEvalStrategyGroupedProp((TableAndLockProviderGrouped) provider, this);

                case ExprTableEvalStrategyEnum.UNGROUPED_AGG_ACCESSREAD:
                    return new ExprTableEvalStrategyUngroupedAggAccessRead(
                        (TableAndLockProviderUngrouped) provider, this);

                case ExprTableEvalStrategyEnum.GROUPED_AGG_ACCESSREAD:
                    return new ExprTableEvalStrategyGroupedAggAccessRead((TableAndLockProviderGrouped) provider, this);

                case ExprTableEvalStrategyEnum.KEYS:
                    return new ExprTableEvalStrategyGroupedKeys((TableAndLockProviderGrouped) provider, this);

                default:
                    throw new IllegalStateException("Unrecognized strategy " + StrategyEnum);
            }
        }
    }
} // end of namespace