///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    public class AggregationGroupByRollupLevelForge
    {
        private readonly Type[] _allGroupKeyTypes;
        private readonly MultiKeyClassRef _allKeysMultikey;
        private readonly int _levelNumber;
        private readonly int _levelOffset;
        private readonly MultiKeyClassRef _subKeyMultikey;
        private readonly int[] _rollupKeys;

        public AggregationGroupByRollupLevelForge(
            int levelNumber,
            int levelOffset,
            int[] rollupKeys,
            Type[] allGroupKeyTypes,
            MultiKeyClassRef allKeysMultikey,
            MultiKeyClassRef subKeyMultikey)
        {
            _levelNumber = levelNumber;
            _levelOffset = levelOffset;
            _rollupKeys = rollupKeys;
            _allGroupKeyTypes = allGroupKeyTypes;
            _allKeysMultikey = allKeysMultikey;
            _subKeyMultikey = subKeyMultikey;
        }

        public int AggregationOffset {
            get {
                if (IsAggregationTop) {
                    throw new ArgumentException();
                }

                return _levelOffset;
            }
        }

        public bool IsAggregationTop => _levelOffset == -1;

        public int[] RollupKeys => _rollupKeys;

        public CodegenExpression Codegen(
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(AggregationGroupByRollupLevel), GetType(), classScope);

            var serde = ConstantNull();
            if (RollupKeys != null) {
                if (_allGroupKeyTypes.Length == RollupKeys.Length) {
                    serde = _allKeysMultikey.GetExprMKSerde(method, classScope);
                }
                else {
                    serde = _subKeyMultikey.GetExprMKSerde(method, classScope);
                }
            }

            // CodegenExpressionNewAnonymousClass clazz = NewAnonymousClass(
            //     method.Block,
            //     typeof(AggregationGroupByRollupLevel),
            //     Arrays.AsList(Constant(_levelNumber), Constant(_levelOffset), Constant(RollupKeys), serde));

            // var computeSubkey = CodegenMethod
            //     .MakeParentNode(typeof(object), GetType(), classScope)
            //     .AddParam(typeof(object), "groupKey");
            // clazz.AddMethod("computeSubkey", computeSubkey);

            var computeSubkey = new CodegenExpressionLambda(method.Block)
                .WithParam(typeof(object), "groupKey")
                .WithBody(
                    block => {
           
                        if (IsAggregationTop) {
                            block.BlockReturn(ConstantNull());
                        }
                        else if (_allKeysMultikey == null || _allGroupKeyTypes.Length == RollupKeys.Length) {
                            block.BlockReturn(Ref("groupKey"));
                        }
                        else {
                            if (_allKeysMultikey.ClassNameMK.Type != null) {
                                block.DeclareVar(
                                    _allKeysMultikey.ClassNameMK.Type,
                                    "mk",
                                    Cast(_allKeysMultikey.ClassNameMK.Type, Ref("groupKey")));
                            }
                            else {
                                block.DeclareVar(
                                    _allKeysMultikey.ClassNameMK.Name,
                                    "mk",
                                    Cast(_allKeysMultikey.ClassNameMK.Name, Ref("groupKey")));
                            }
                            
                            if (RollupKeys.Length == 1 && (_subKeyMultikey == null || _subKeyMultikey.ClassNameMK == null)) {
                                block.BlockReturn(ExprDotMethod(Ref("mk"), "GetKey", Constant(RollupKeys[0])));
                            }
                            else {
                                var expressions = new CodegenExpression[RollupKeys.Length];
                                for (var i = 0; i < RollupKeys.Length; i++) {
                                    var index = RollupKeys[i];
                                    var keyExpr = ExprDotMethod(Ref("mk"), "GetKey", Constant(index));
                                    var type = _allGroupKeyTypes[index];
                                    expressions[i] = type.IsNullTypeSafe() ? ConstantNull() : Cast(type, keyExpr);
                                }

                                var instance = _subKeyMultikey.ClassNameMK.Type != null
                                    ? NewInstance(_subKeyMultikey.ClassNameMK.Type, expressions)
                                    : NewInstanceNamed(_subKeyMultikey.ClassNameMK.Name, expressions);
                                
                                block.BlockReturn(instance);
                            }
                        }
                    });

            var clazz = NewInstance<ProxyAggregationGroupByRollupLevel>(
                Constant(_levelNumber),
                Constant(_levelOffset),
                Constant(RollupKeys),
                serde,
                computeSubkey);                
 
            method.Block.MethodReturn(clazz);
            return LocalMethod(method);
        }
    }
} // end of namespace