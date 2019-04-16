///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.@join.querygraph;
using com.espertech.esper.common.@internal.epl.lookupplan;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.lookup
{
    /// <summary>
    ///     Index lookup strategy for subqueries.
    /// </summary>
    public class SubordCompositeTableLookupStrategyFactoryForge : SubordTableLookupStrategyFactoryForge
    {
        private readonly Type[] _coercionRangeTypes;
        private readonly IList<SubordPropHashKeyForge> _hashKeys;
        private readonly Type[] _hashTypes;
        private readonly bool isNWOnTrigger;
        private readonly int _numStreams;
        private readonly IList<SubordPropRangeKeyForge> _rangeProps;

        public SubordCompositeTableLookupStrategyFactoryForge(
            bool isNWOnTrigger,
            int numStreams,
            IList<SubordPropHashKeyForge> keyExpr,
            Type[] coercionKeyTypes,
            IList<SubordPropRangeKeyForge> rangeProps,
            Type[] coercionRangeTypes)
        {
            this.isNWOnTrigger = isNWOnTrigger;
            this._numStreams = numStreams;
            _hashKeys = keyExpr;
            _hashTypes = coercionKeyTypes;
            this._rangeProps = rangeProps;
            this._coercionRangeTypes = coercionRangeTypes;
        }

        public string ToQueryPlan()
        {
            return GetType().Name;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(SubordCompositeTableLookupStrategyFactory), GetType(), classScope);

            IList<string> expressions = new List<string>();
            var hashEval = ConstantNull();
            if (_hashKeys != null && !_hashKeys.IsEmpty()) {
                var forges = new ExprForge[_hashKeys.Count];
                for (var i = 0; i < _hashKeys.Count; i++) {
                    forges[i] = _hashKeys[i].HashKey.KeyExpr.Forge;
                }

                expressions.AddAll(ExprNodeUtilityPrint.ToExpressionStringsMinPrecedence(forges));
                hashEval = ExprNodeUtilityCodegen.CodegenEvaluatorMayMultiKeyWCoerce(
                    forges, _hashTypes, method, GetType(), classScope);
            }

            method.Block.DeclareVar(
                typeof(QueryGraphValueEntryRange[]), "rangeEvals",
                NewArrayByLength(typeof(QueryGraphValueEntryRange), Constant(_rangeProps.Count)));
            for (var i = 0; i < _rangeProps.Count; i++) {
                CodegenExpression rangeEval = _rangeProps[i].RangeInfo.Make(
                    _coercionRangeTypes[i], parent, symbols, classScope);
                method.Block.AssignArrayElement(Ref("rangeEvals"), Constant(i), rangeEval);
            }

            method.Block.MethodReturn(
                NewInstance(
                    typeof(SubordCompositeTableLookupStrategyFactory),
                    Constant(isNWOnTrigger), Constant(_numStreams), Constant(expressions.ToArray()),
                    hashEval, Ref("rangeEvals")));
            return LocalMethod(method);
        }
    }
} // end of namespace