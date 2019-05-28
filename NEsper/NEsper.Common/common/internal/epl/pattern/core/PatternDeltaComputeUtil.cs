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
        public static CodegenExpression MakePatternDeltaAnonymous(
            ExprNode parameter,
            MatchedEventConvertorForge convertor,
            TimeAbacus timeAbacus,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            var compute = NewAnonymousClass(method.Block, typeof(PatternDeltaCompute));
            var computeDelta = CodegenMethod.MakeParentNode(typeof(long), typeof(PatternDeltaComputeUtil), classScope)
                .AddParam(
                    CodegenNamedParam.From(
                        typeof(MatchedEventMap), "beginState", typeof(PatternAgentInstanceContext), "context"));
            compute.AddMethod("computeDelta", computeDelta);

            if (parameter is ExprTimePeriod) {
                var timePeriod = (ExprTimePeriod) parameter;
                var time = ExprDotMethod(Ref("context"), "getTime");
                if (timePeriod.IsConstantResult) {
                    var delta = classScope.AddFieldUnshared<TimePeriodCompute>(
                        true, timePeriod.TimePeriodComputeForge.MakeEvaluator(
                            classScope.NamespaceScope.InitMethod, classScope));
                    computeDelta.Block.MethodReturn(
                        ExprDotMethod(
                            delta, "deltaAdd", time, ConstantNull(), ConstantTrue(),
                            ExprDotMethod(Ref("context"), "getAgentInstanceContext")));
                }
                else {
                    var delta = classScope.AddFieldUnshared<TimePeriodCompute>(
                        true, timePeriod.TimePeriodComputeForge.MakeEvaluator(
                            classScope.NamespaceScope.InitMethod, classScope));
                    computeDelta.Block
                        .DeclareVar(
                            typeof(EventBean[]), "events",
                            LocalMethod(convertor.Make(computeDelta, classScope), Ref("beginState")))
                        .MethodReturn(
                            ExprDotMethod(
                                delta, "deltaAdd", time, Ref("events"), ConstantTrue(),
                                ExprDotMethod(Ref("context"), "getAgentInstanceContext")));
                }
            }
            else {
                var eval = CodegenLegoMethodExpression.CodegenExpression(parameter.Forge, method, classScope);
                CodegenExpression events;
                if (parameter.Forge.ForgeConstantType.IsConstant) {
                    events = ConstantNull();
                }
                else {
                    events = LocalMethod(convertor.Make(computeDelta, classScope), Ref("beginState"));
                }

                computeDelta.Block
                    .DeclareVar(typeof(EventBean[]), "events", events)
                    .DeclareVar(
                        parameter.Forge.EvaluationType, "result",
                        LocalMethod(
                            eval, Ref("events"), ConstantTrue(),
                            ExprDotMethod(Ref("context"), "getAgentInstanceContext")));
                if (!parameter.Forge.EvaluationType.IsPrimitive) {
                    computeDelta.Block.IfRefNull("result").BlockThrow(
                        NewInstance<EPException>(Constant("Null value returned for guard expression")));
                }

                computeDelta.Block.MethodReturn(timeAbacus.DeltaForSecondsDoubleCodegen(Ref("result"), classScope));
            }

            return compute;
        }
    }
} // end of namespace