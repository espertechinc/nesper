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
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    public class AggregationServiceCodegenUtil
    {
        public static readonly Consumer<CodegenMethod> NIL_METHOD_CONSUMER = method => { };
        public static readonly IList<CodegenNamedParam> NIL_NAMED_PARAM = Collections.GetEmptyList<CodegenNamedParam>();

        public static CodegenMethod ComputeMultiKeyCodegen(
            int idNumber,
            ExprForge[] partitionForges,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            var exprSymbol = new ExprForgeCodegenSymbol(true, null);
            Consumer<CodegenMethod> code = method => {
                if (partitionForges.Length == 1) {
                    CodegenExpression expression = partitionForges[0]
                        .EvaluateCodegen(
                            typeof(object),
                            method,
                            exprSymbol,
                            classScope);
                    exprSymbol.DerivedSymbolsCodegen(method, method.Block, classScope);
                    method.Block.MethodReturn(expression);
                }
                else {
                    var expressions = new CodegenExpression[partitionForges.Length];
                    for (var i = 0; i < partitionForges.Length; i++) {
                        expressions[i] = partitionForges[i]
                            .EvaluateCodegen(
                                typeof(object),
                                method,
                                exprSymbol,
                                classScope);
                    }

                    exprSymbol.DerivedSymbolsCodegen(method, method.Block, classScope);
                    method.Block.DeclareVar<object[]>(
                        "keys",
                        NewArrayByLength(typeof(object), Constant(partitionForges.Length)));
                    for (var i = 0; i < expressions.Length; i++) {
                        method.Block.AssignArrayElement("keys", Constant(i), expressions[i]);
                    }

                    method.Block.MethodReturn(NewInstance<HashableMultiKey>(Ref("keys")));
                }
            };

            return namedMethods.AddMethodWithSymbols(
                typeof(object),
                "ComputeKeyArrayCodegen_" + idNumber,
                CodegenNamedParam.From(
                    typeof(EventBean[]),
                    NAME_EPS,
                    typeof(bool),
                    NAME_ISNEWDATA,
                    typeof(ExprEvaluatorContext),
                    NAME_EXPREVALCONTEXT),
                typeof(AggregationServiceCodegenUtil),
                classScope,
                code,
                exprSymbol);
        }

        public static void GenerateIncidentals(
            bool hasRefcount,
            bool hasLastUpdTime,
            AggregationRowCtorDesc rowCtorDesc)
        {
            var namedMethods = rowCtorDesc.NamedMethods;
            var classScope = rowCtorDesc.ClassScope;
            IList<CodegenTypedParam> rowMembers = rowCtorDesc.RowMembers;

            if (hasRefcount) {
                rowMembers.Add(new CodegenTypedParam(typeof(int), "refcount").WithFinal(false));
            }

            namedMethods.AddMethod(
                typeof(void),
                "IncreaseRefcount",
                NIL_NAMED_PARAM,
                typeof(AggregationServiceCodegenUtil),
                classScope,
                hasRefcount ? method => method.Block.Increment(Ref("refcount")) : NIL_METHOD_CONSUMER);
            namedMethods.AddMethod(
                typeof(void),
                "DecreaseRefcount",
                NIL_NAMED_PARAM,
                typeof(AggregationServiceCodegenUtil),
                classScope,
                hasRefcount ? method => method.Block.Decrement(Ref("refcount")) : NIL_METHOD_CONSUMER);
            namedMethods.AddMethod(
                typeof(long),
                "GetRefcount",
                NIL_NAMED_PARAM,
                typeof(AggregationServiceCodegenUtil),
                classScope,
                hasRefcount
                    ? new Consumer<CodegenMethod>(method => method.Block.MethodReturn(Ref("refcount")))
                    : new Consumer<CodegenMethod>(method => method.Block.MethodReturn(Constant(1))));

            if (hasLastUpdTime) {
                rowMembers.Add(new CodegenTypedParam(typeof(long), "lastUpd").WithFinal(false));
            }

            namedMethods.AddMethod(
                typeof(void),
                "SetLastUpdateTime",
                CodegenNamedParam.From(typeof(long), "time"),
                typeof(AggregationServiceCodegenUtil),
                classScope,
                hasLastUpdTime
                    ? new Consumer<CodegenMethod>(method => method.Block.AssignRef("lastUpd", Ref("time")))
                    : new Consumer<CodegenMethod>(method => method.Block.MethodThrowUnsupported()));

            namedMethods.AddMethod(
                typeof(long),
                "GetLastUpdateTime",
                NIL_NAMED_PARAM,
                typeof(AggregationServiceCodegenUtil),
                classScope,
                hasLastUpdTime
                    ? new Consumer<CodegenMethod>(method => method.Block.MethodReturn(Ref("lastUpd")))
                    : new Consumer<CodegenMethod>(method => method.Block.MethodThrowUnsupported()));
        }
    }
} // end of namespace