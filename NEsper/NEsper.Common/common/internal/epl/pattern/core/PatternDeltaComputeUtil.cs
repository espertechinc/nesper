///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

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
            var compute = NewInstance<PatternDeltaCompute>(computeDelta);

            //var compute = NewAnonymousClass(method.Block, typeof(PatternDeltaCompute));
            //var computeDelta = CodegenMethod.MakeMethod(typeof(long), typeof(PatternDeltaComputeUtil), classScope)
            //    .AddParam(
            //        CodegenNamedParam.From(
            //            typeof(MatchedEventMap),
            //            "beginState",
            //            typeof(PatternAgentInstanceContext),
            //            "context"));
            //compute.AddMethod("ComputeDelta", computeDelta);

            if (parameter is ExprTimePeriod) {
                var timePeriod = (ExprTimePeriod) parameter;
                var time = ExprDotName(Ref("context"), "Time");
                if (timePeriod.IsConstantResult) {
                    var delta = classScope.AddFieldUnshared<TimePeriodCompute>(
                        true,
                        timePeriod.TimePeriodComputeForge.MakeEvaluator(
                            classScope.NamespaceScope.InitMethod,
                            classScope));
                    computeDelta.Block
                        .ReturnMethodOrBlock(
                            ExprDotMethod(
                                delta,
                                "DeltaAdd",
                                time,
                                ConstantNull(),
                                ConstantTrue(),
                                ExprDotName(Ref("context"), "AgentInstanceContext")));
                }
                else {
                    var delta = classScope.AddFieldUnshared<TimePeriodCompute>(
                        true,
                        timePeriod.TimePeriodComputeForge.MakeEvaluator(
                            classScope.NamespaceScope.InitMethod,
                            classScope));
                    computeDelta.Block
                        .DeclareVar<EventBean[]>(
                            "events",
                            LocalMethod(
                                convertor.Make(method, classScope),
                                //convertor.Make(computeDelta, classScope),
                                Ref("beginState")))
                        .ReturnMethodOrBlock(
                            ExprDotMethod(
                                delta,
                                "DeltaAdd",
                                time,
                                Ref("events"),
                                ConstantTrue(),
                                ExprDotMethod(Ref("context"), "AgentInstanceContext")));
                }
            }
            else {
                var eval = CodegenLegoMethodExpression.CodegenExpression(parameter.Forge, method, classScope);
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
                        parameter.Forge.EvaluationType,
                        "result",
                        LocalMethod(
                            eval,
                            Ref("events"),
                            ConstantTrue(),
                            ExprDotName(Ref("context"), "AgentInstanceContext")));
                if (!parameter.Forge.EvaluationType.IsPrimitive) {
                    computeDelta.Block.IfRefNull("result")
                        .BlockThrow(
                            NewInstance<EPException>(Constant("Null value returned for guard expression")));
                }

                computeDelta.Block.ReturnMethodOrBlock(
                    timeAbacus.DeltaForSecondsDoubleCodegen(Ref("result"), classScope));
            }

            return compute;
        }
    }
} // end of namespace