///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.historical.indexingstrategy
{
    /// <summary>
    /// Strategy of indexing that simply builds an unindexed table of poll results.
    /// <para />For use when caching is disabled or when no proper index could be build because no where-clause or on-clause exists or
    /// these clauses don't yield indexable columns on analysis.
    /// </summary>
    public class PollResultIndexingStrategyNoIndexForge : PollResultIndexingStrategyForge
    {
        public static readonly PollResultIndexingStrategyNoIndexForge INSTANCE =
            new PollResultIndexingStrategyNoIndexForge();

        private PollResultIndexingStrategyNoIndexForge()
        {
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            return PublicConstValue(typeof(PollResultIndexingStrategyNoIndex), "INSTANCE");
        }

        public string ToQueryPlan()
        {
            return GetType().Name;
        }
    }
} // end of namespace