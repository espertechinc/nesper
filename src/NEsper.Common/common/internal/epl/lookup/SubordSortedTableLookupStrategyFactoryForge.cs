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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.join.queryplan;
using com.espertech.esper.common.@internal.epl.lookupplan;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.lookup
{
    /// <summary>
    ///     Index lookup strategy for subqueries.
    /// </summary>
    public class SubordSortedTableLookupStrategyFactoryForge : SubordTableLookupStrategyFactoryForge
    {
        private readonly CoercionDesc _coercionDesc;
        private readonly bool _isNwOnTrigger;
        private readonly int _numStreamsOuter;
        private readonly SubordPropRangeKeyForge _rangeKey;

        public SubordSortedTableLookupStrategyFactoryForge(
            bool isNWOnTrigger,
            int numStreamsOuter,
            SubordPropRangeKeyForge rangeKey,
            CoercionDesc coercionDesc)
        {
            _isNwOnTrigger = isNWOnTrigger;
            _numStreamsOuter = numStreamsOuter;
            _rangeKey = rangeKey;
            _coercionDesc = coercionDesc;
        }

        public string ToQueryPlan()
        {
            return GetType().Name + " range " + _rangeKey.ToQueryPlan();
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var expressions = ExprNodeUtilityPrint.ToExpressionStringsMinPrecedence(_rangeKey.RangeInfo.Expressions);
            return NewInstance<SubordSortedTableLookupStrategyFactory>(
                Constant(_isNwOnTrigger),
                Constant(_numStreamsOuter),
                Constant(expressions[0]),
                _rangeKey.RangeInfo.Make(_coercionDesc.CoercionTypes[0], parent, symbols, classScope));
        }
    }
} // end of namespace