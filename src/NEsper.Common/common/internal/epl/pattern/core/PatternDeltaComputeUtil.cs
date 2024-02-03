///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.epl.expression.time.eval;
using com.espertech.esper.common.@internal.epl.expression.time.node;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.pattern.core
{
    public class PatternDeltaComputeUtil
    {
        public static CodegenExpression MakePatternDeltaLambda(
            ExprNode parameter,
            MatchedEventConvertorForge convertor,
            TimeAbacus timeAbacus,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            var computeDelta = new CodegenExpressionLambda(method.Block)
                .WithParam<MatchedEventMap>("beginState")
                .WithParam<PatternAgentInstanceContext>("context");
            
            // CodegenExpressionNewAnonymousClass compute = NewAnonymousClass(method.Block, typeof(PatternDeltaCompute));
            // var computeDelta = CodegenMethod
            //     .MakeParentNode(typeof(long), typeof(PatternDeltaComputeUtil), classScope)
            //     .AddParam(
            //         CodegenNamedParam.From(
            //             typeof(MatchedEventMap),
            //             "beginState",
            //             typeof(PatternAgentInstanceContext),
            //             "context"));
            // compute.AddMethod("computeDelta", computeDelta);

            if (parameter is ExprTimePeriod timePeriod) {
                var time = ExprDotName(Ref("context"), "Time");
                if (timePeriod.IsConstantResult) {
                    var delta = classScope.AddDefaultFieldUnshared(
                        true,
                        typeof(TimePeriodCompute),
                        timePeriod.TimePeriodComputeForge.MakeEvaluator(
                            classScope.NamespaceScope.InitMethod,
                            classScope));
                    computeDelta = computeDelta.WithBody(
                        block => block.BlockReturn(
                            ExprDotMethod(
                                delta,
                                "DeltaAdd",
                                time,
                                ConstantNull(),
                                ConstantTrue(),
                                ExprDotMethod(Ref("context"), "GetAgentInstanceContext"))));
                }
                else {
                    var delta = classScope.AddDefaultFieldUnshared(
                        true,
                        typeof(TimePeriodCompute),
                        timePeriod.TimePeriodComputeForge.MakeEvaluator(
                            classScope.NamespaceScope.InitMethod,
                            classScope));
                    computeDelta = computeDelta.WithBody(
                        block => block
                            .DeclareVar<EventBean[]>(
                                "events",
                                LocalMethod(
                                    convertor.Make(method, classScope),
                                    //convertor.Make(computeDelta, classScope),
                                    Ref("beginState")))
                            .BlockReturn(
                                ExprDotMethod(
                                    delta,
                                    "DeltaAdd",
                                    time,
                                    Ref("events"),
                                    ConstantTrue(),
                                    ExprDotMethod(Ref("context"), "GetAgentInstanceContext"))));
                }
            }
            else {
                var eval = CodegenLegoMethodExpression.CodegenExpression(
                    parameter.Forge, method, classScope);

                CodegenExpression events;
                if (parameter.Forge.ForgeConstantType.IsConstant) {
                    events = ConstantNull();
                }
                else {
                    events = LocalMethod(
                        convertor.Make(method, classScope),
                        //convertor.Make(computeDelta, classScope),
                        Ref("beginState"));
                }

                computeDelta.Block
                    .DeclareVar<EventBean[]>("events", events)
                    .DeclareVar(
                        parameter.Forge.EvaluationType.GetBoxedType(),
                        "result",
                        LocalMethod(
                            eval,
                            Ref("events"),
                            ConstantTrue(),
                            ExprDotMethod(Ref("context"), "GetAgentInstanceContext")));
                if (parameter.Forge.EvaluationType.CanBeNull()) {
                    computeDelta.Block.IfRefNull("result")
                        .BlockThrow(
                            NewInstance(typeof(EPException), Constant("Null value returned for guard expression")));
                }

                computeDelta.Block.BlockReturn(
                    timeAbacus.DeltaForSecondsDoubleCodegen(Ref("result"), classScope));
            }

            return NewInstance<PatternDeltaCompute>(computeDelta);
        }
    }
} // end of namespace