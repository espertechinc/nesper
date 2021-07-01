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
    /// <summary>
    ///     Full table scan strategy for a poll-based cache result.
    /// </summary>
    public class HistoricalIndexLookupStrategyNoIndexForge : HistoricalIndexLookupStrategyForge
    {
        public static readonly HistoricalIndexLookupStrategyNoIndexForge INSTANCE =
            new HistoricalIndexLookupStrategyNoIndexForge();

        private HistoricalIndexLookupStrategyNoIndexForge()
        {
        }

        public CodegenExpression Make(
            CodegenMethodScope method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            return PublicConstValue(typeof(HistoricalIndexLookupStrategyNoIndex), "INSTANCE");
        }

        public string ToQueryPlan()
        {
            return GetType().Name;
        }
    }
} // end of namespace