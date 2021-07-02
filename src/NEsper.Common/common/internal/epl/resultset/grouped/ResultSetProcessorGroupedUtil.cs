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
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.codegen;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.resultset.rowperevent;
using com.espertech.esper.common.@internal.epl.resultset.rowpergroup;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;
using static com.espertech.esper.common.@internal.epl.resultset.codegen.ResultSetProcessorCodegenNames;
using static com.espertech.esper.common.@internal.metrics.instrumentation.InstrumentationCode;

namespace com.espertech.esper.common.@internal.epl.resultset.grouped
{
    public class ResultSetProcessorGroupedUtil
    {
        public const string METHOD_APPLYAGGVIEWRESULTKEYEDVIEW = "ApplyAggViewResultKeyedView";
        public const string METHOD_APPLYAGGJOINRESULTKEYEDJOIN = "ApplyAggJoinResultKeyedJoin";

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="aggregationService">aggs</param>
        /// <param name="exprEvaluatorContext">ctx</param>
        /// <param name="newData">new data</param>
        /// <param name="newDataMultiKey">new data keys</param>
        /// <param name="oldData">old data</param>
        /// <param name="oldDataMultiKey">old data keys</param>
        /// <param name="eventsPerStream">event buffer, transient buffer</param>
        public static void ApplyAggViewResultKeyedView(
            AggregationService aggregationService,
            ExprEvaluatorContext exprEvaluatorContext,
            EventBean[] newData,
            object[] newDataMultiKey,
            EventBean[] oldData,
            object[] oldDataMultiKey,
            EventBean[] eventsPerStream)
        {
            // update aggregates
            if (newData != null) {
                // apply new data to aggregates
                for (var i = 0; i < newData.Length; i++) {
                    eventsPerStream[0] = newData[i];
                    aggregationService.ApplyEnter(eventsPerStream, newDataMultiKey[i], exprEvaluatorContext);
                }
            }

            if (oldData != null) {
                // apply old data to aggregates
                for (var i = 0; i < oldData.Length; i++) {
                    eventsPerStream[0] = oldData[i];
                    aggregationService.ApplyLeave(eventsPerStream, oldDataMultiKey[i], exprEvaluatorContext);
                }
            }
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="aggregationService">aggs</param>
        /// <param name="exprEvaluatorContext">ctx</param>
        /// <param name="newEvents">new data</param>
        /// <param name="newDataMultiKey">new data keys</param>
        /// <param name="oldEvents">old data</param>
        /// <param name="oldDataMultiKey">old data keys</param>
        public static void ApplyAggJoinResultKeyedJoin(
            AggregationService aggregationService,
            ExprEvaluatorContext exprEvaluatorContext,
            ISet<MultiKeyArrayOfKeys<EventBean>> newEvents,
            object[] newDataMultiKey,
            ISet<MultiKeyArrayOfKeys<EventBean>> oldEvents,
            object[] oldDataMultiKey)
        {
            // update aggregates
            if (!newEvents.IsEmpty()) {
                // apply old data to aggregates
                var count = 0;
                foreach (var eventsPerStream in newEvents) {
                    aggregationService.ApplyEnter(eventsPerStream.Array, newDataMultiKey[count], exprEvaluatorContext);
                    count++;
                }
            }

            if (oldEvents != null && !oldEvents.IsEmpty()) {
                // apply old data to aggregates
                var count = 0;
                foreach (var eventsPerStream in oldEvents) {
                    aggregationService.ApplyLeave(eventsPerStream.Array, oldDataMultiKey[count], exprEvaluatorContext);
                    count++;
                }
            }
        }

        public static CodegenMethod GenerateGroupKeySingleCodegen(
            ExprNode[] groupKeyExpressions,
            MultiKeyClassRef optionalMultiKeyClasses,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            Consumer<CodegenMethod> code = methodNode => {
                string[] expressions = null;
                if (classScope.IsInstrumented) {
                    expressions = ExprNodeUtilityPrint.ToExpressionStringsMinPrecedence(groupKeyExpressions);
                }

                methodNode.Block.Apply(
                    Instblock(
                        classScope,
                        "qResultSetProcessComputeGroupKeys",
                        ExprForgeCodegenNames.REF_ISNEWDATA,
                        Constant(expressions),
                        REF_EPS));


                if (optionalMultiKeyClasses != null && optionalMultiKeyClasses.ClassNameMK != null) {
                    var method = MultiKeyCodegen.CodegenMethod(groupKeyExpressions, optionalMultiKeyClasses, methodNode, classScope);
                    methodNode
                        .Block
                        .DeclareVar<object>("key", LocalMethod(method, REF_EPS, ExprForgeCodegenNames.REF_ISNEWDATA, MEMBER_EXPREVALCONTEXT))
                        .Apply(Instblock(classScope, "aResultSetProcessComputeGroupKeys", ExprForgeCodegenNames.REF_ISNEWDATA, Ref("key")))
                        .MethodReturn(Ref("key"));
                    return;
                }

                if (groupKeyExpressions.Length > 1) {
                    throw new IllegalStateException("Multiple group-by expression and no multikey");
                }

                var expression = CodegenLegoMethodExpression.CodegenExpression(groupKeyExpressions[0].Forge, methodNode, classScope);
                methodNode
                    .Block
                    .DeclareVar<object>("key", LocalMethod(expression, REF_EPS, ExprForgeCodegenNames.REF_ISNEWDATA, MEMBER_EXPREVALCONTEXT))
                    .Apply(Instblock(classScope, "aResultSetProcessComputeGroupKeys", ExprForgeCodegenNames.REF_ISNEWDATA, Ref("key")))
                    .MethodReturn(Ref("key"));
            };

            return instance.Methods.AddMethod(
                typeof(object),
                "GenerateGroupKeySingle",
                CodegenNamedParam.From(
                    typeof(EventBean[]),
                    NAME_EPS,
                    typeof(bool),
                    ExprForgeCodegenNames.NAME_ISNEWDATA),
                typeof(ResultSetProcessorUtil),
                classScope,
                code);
        }

        public static CodegenMethod GenerateGroupKeyArrayViewCodegen(
            CodegenMethod generateGroupKeySingle,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            Consumer<CodegenMethod> code = method => {
                method.Block.IfRefNullReturnNull("events")
                    .DeclareVar<EventBean[]>(
                        "eventsPerStream",
                        NewArrayByLength(typeof(EventBean), Constant(1)))
                    .DeclareVar<object[]>("keys", NewArrayByLength(typeof(object), ArrayLength(Ref("events"))));
                {
                    var forLoop = method.Block.ForLoopIntSimple("i", ArrayLength(Ref("events")));
                    forLoop.AssignArrayElement("eventsPerStream", Constant(0), ArrayAtIndex(Ref("events"), Ref("i")))
                        .AssignArrayElement(
                            "keys",
                            Ref("i"),
                            LocalMethod(
                                generateGroupKeySingle,
                                Ref("eventsPerStream"),
                                ExprForgeCodegenNames.REF_ISNEWDATA));
                }
                method.Block.MethodReturn(Ref("keys"));
            };
            return instance.Methods.AddMethod(
                typeof(object[]),
                "GenerateGroupKeyArrayView",
                CodegenNamedParam.From(
                    typeof(EventBean[]),
                    "events",
                    typeof(bool),
                    ExprForgeCodegenNames.NAME_ISNEWDATA),
                typeof(ResultSetProcessorRowPerGroup),
                classScope,
                code);
        }

        public static CodegenMethod GenerateGroupKeyArrayJoinCodegen(
            CodegenMethod generateGroupKeySingle,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            Consumer<CodegenMethod> code = method => {
                method.Block.IfCondition(ExprDotMethod(Ref("resultSet"), "IsEmpty"))
                    .BlockReturn(ConstantNull())
                    .DeclareVar<object[]>(
                        "keys",
                        NewArrayByLength(typeof(object), ExprDotName(Ref("resultSet"), "Count")))
                    .DeclareVar<int>("count", Constant(0))
                    .ForEach(typeof(MultiKeyArrayOfKeys<EventBean>), "eventsPerStream", Ref("resultSet"))
                    .AssignArrayElement(
                        "keys",
                        Ref("count"),
                        LocalMethod(
                            generateGroupKeySingle,
                            ExprDotName(Ref("eventsPerStream"), "Array"),
                            ExprForgeCodegenNames.REF_ISNEWDATA))
                    .IncrementRef("count")
                    .BlockEnd()
                    .MethodReturn(Ref("keys"));
            };
            return instance.Methods.AddMethod(
                typeof(object[]),
                "GenerateGroupKeyArrayJoin",
                CodegenNamedParam.From(typeof(ISet<MultiKeyArrayOfKeys<EventBean>>), "resultSet", typeof(bool), "isNewData"),
                typeof(ResultSetProcessorRowPerEventImpl),
                classScope,
                code);
        }
    }
} // end of namespace