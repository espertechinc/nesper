///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.historical.lookupstrategy
{
    public class HistoricalIndexLookupStrategyMultiForge : HistoricalIndexLookupStrategyForge
    {
        private readonly int indexUsed;
        private readonly HistoricalIndexLookupStrategyForge innerLookupStrategy;

        public HistoricalIndexLookupStrategyMultiForge(
            int indexUsed,
            HistoricalIndexLookupStrategyForge innerLookupStrategy)
        {
            this.indexUsed = indexUsed;
            this.innerLookupStrategy = innerLookupStrategy;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(HistoricalIndexLookupStrategyMulti), GetType(), classScope);
            method.Block
                .DeclareVar<HistoricalIndexLookupStrategyMulti>(
                    "strat",
                    NewInstance(typeof(HistoricalIndexLookupStrategyMulti)))
                .SetProperty(Ref("strat"), "IndexUsed", Constant(indexUsed))
                .SetProperty(Ref("strat"), "InnerLookupStrategy", innerLookupStrategy.Make(method, symbols, classScope))
                .MethodReturn(Ref("strat"));
            return LocalMethod(method);
        }

        public string ToQueryPlan()
        {
            return GetType().Name + " inner: " + innerLookupStrategy.ToQueryPlan();
        }
    }
} // end of namespace