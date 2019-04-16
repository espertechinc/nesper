///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.@join.queryplan;
using com.espertech.esper.common.@internal.epl.lookupplan;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.lookup
{
    /// <summary>
    ///     Index lookup strategy for subqueries.
    /// </summary>
    public class SubordHashedTableLookupStrategyFactoryForge : SubordTableLookupStrategyFactoryForge
    {
        private readonly CoercionDesc _hashKeyCoercionTypes;
        private readonly IList<SubordPropHashKeyForge> _hashKeys;
        private readonly string[] _hashStrictKeys;
        private readonly bool isNWOnTrigger;
        private readonly bool _isStrictKeys;
        private readonly int[] _keyStreamNumbers;
        private readonly int _numStreamsOuter;
        private readonly IList<EventType> _outerStreamTypesZeroIndexed;

        public SubordHashedTableLookupStrategyFactoryForge(
            bool isNWOnTrigger,
            int numStreamsOuter,
            IList<SubordPropHashKeyForge> hashKeys,
            CoercionDesc hashKeyCoercionTypes,
            bool isStrictKeys,
            string[] hashStrictKeys,
            int[] keyStreamNumbers,
            IList<EventType> outerStreamTypesZeroIndexed)
        {
            this.isNWOnTrigger = isNWOnTrigger;
            this._numStreamsOuter = numStreamsOuter;
            this._hashKeys = hashKeys;
            this._hashKeyCoercionTypes = hashKeyCoercionTypes;
            this._isStrictKeys = isStrictKeys;
            this._hashStrictKeys = hashStrictKeys;
            this._keyStreamNumbers = keyStreamNumbers;
            this._outerStreamTypesZeroIndexed = outerStreamTypesZeroIndexed;
        }

        private string[] Expressions {
            get {
                var expressions = new string[_hashKeys.Count];
                for (var i = 0; i < _hashKeys.Count; i++) {
                    expressions[i] =
                        ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(_hashKeys[i].HashKey.KeyExpr);
                }

                return expressions;
            }
        }

        public string ToQueryPlan()
        {
            return GetType().Name + " expressions " + CompatExtensions.Render(Expressions);
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var methodNode = parent.MakeChild(typeof(SubordTableLookupStrategyFactory), GetType(), classScope);
            if (_isStrictKeys) {
                int[] keyStreamNums = IntArrayUtil.Copy(_keyStreamNumbers);
                var keyStreamTypes = _outerStreamTypesZeroIndexed;
                if (isNWOnTrigger) {
                    keyStreamTypes = EventTypeUtility.ShiftRight(_outerStreamTypesZeroIndexed);
                    for (var i = 0; i < keyStreamNums.Length; i++) {
                        keyStreamNums[i] = keyStreamNums[i] + 1;
                    }
                }

                var eval = ExprNodeUtilityCodegen.CodegenEvaluatorMayMultiKeyPropPerStream(
                    keyStreamTypes, _hashStrictKeys, _hashKeyCoercionTypes.CoercionTypes, keyStreamNums,
                    methodNode, GetType(), classScope);
                methodNode.Block.MethodReturn(
                    NewInstance(
                        typeof(SubordHashedTableLookupStrategyPropFactory), Constant(_hashStrictKeys),
                        Constant(keyStreamNums), eval));
                return LocalMethod(methodNode);
            }
            else {
                var forges = new ExprForge[_hashKeys.Count];
                for (var i = 0; i < _hashKeys.Count; i++) {
                    forges[i] = _hashKeys[i].HashKey.KeyExpr.Forge;
                }

                var expressions = ExprNodeUtilityPrint.ToExpressionStringsMinPrecedence(forges);
                CodegenExpression eval = ExprNodeUtilityCodegen.CodegenEvaluatorMayMultiKeyWCoerce(
                    forges, _hashKeyCoercionTypes.CoercionTypes, methodNode, GetType(), classScope);
                methodNode.Block.MethodReturn(
                    NewInstance(
                        typeof(SubordHashedTableLookupStrategyExprFactory), Constant(expressions), eval,
                        Constant(isNWOnTrigger), Constant(_numStreamsOuter)));
                return LocalMethod(methodNode);
            }
        }
    }
} // end of namespace