///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.epl.expression.time.adder;
using com.espertech.esper.common.@internal.epl.expression.time.eval;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.time.node
{
    /// <summary>
    /// Expression representing a time period.
    /// <para/>Child nodes to this expression carry the actual parts and must return a numeric value.
    /// </summary>
    public class ExprTimePeriodForge : ExprForge
    {
        private readonly ExprTimePeriodImpl parent;
        private readonly bool hasVariable;
        private readonly TimePeriodAdder[] adders;
        private ExprEvaluator[] evaluators;

        public ExprTimePeriodForge(
            ExprTimePeriodImpl parent,
            bool hasVariable,
            TimePeriodAdder[] adders)
        {
            this.parent = parent;
            this.hasVariable = hasVariable;
            this.adders = adders;
        }

        public TimePeriodComputeForge ConstTimePeriodComputeForge()
        {
            if (!parent.HasMonth && !parent.HasYear) {
                var seconds = EvaluateAsSeconds(null, true, null);
                var msec = parent.TimeAbacus.DeltaForSecondsDouble(seconds);
                return new TimePeriodComputeConstGivenDeltaForge(msec);
            }
            else {
                evaluators = ExprNodeUtilityQuery.GetEvaluatorsNoCompile(parent.ChildNodes);
                var values = new int[adders.Length];
                for (var i = 0; i < values.Length; i++) {
                    values[i] = evaluators[i].Evaluate(null, true, null).AsInt32();
                }

                return new TimePeriodComputeConstGivenCalAddForge(adders, values, parent.TimeAbacus);
            }
        }

        public TimePeriodComputeForge NonconstTimePeriodComputeForge()
        {
            if (!parent.HasMonth && !parent.HasYear) {
                return new TimePeriodComputeNCGivenTPNonCalForge(this);
            }
            else {
                return new TimePeriodComputeNCGivenTPCalForge(this);
            }
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            throw new IllegalStateException("Time period evaluator does not have a code representation");
        }

        public bool IsHasVariable => hasVariable;

        public double EvaluateAsSeconds(
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext context)
        {
            if (evaluators == null) {
                evaluators = ExprNodeUtilityQuery.GetEvaluatorsNoCompile(parent.ChildNodes);
            }

            double seconds = 0;
            for (var i = 0; i < adders.Length; i++) {
                var result = Eval(evaluators[i], eventsPerStream, newData, context);
                if (result == null) {
                    throw MakeTimePeriodParamNullException(
                        ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(parent));
                }

                seconds += adders[i].Compute(result.Value);
            }

            return seconds;
        }

        public CodegenExpression EvaluateAsSecondsCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(
                typeof(double),
                typeof(ExprTimePeriodForge),
                codegenClassScope);
            string exprText = null;
            if (codegenClassScope.IsInstrumented) {
                exprText = ExprNodeUtilityPrint.ToExpressionStringMinPrecedence(this);
            }

            var block = methodNode.Block
                .Apply(InstrumentationCode.Instblock(codegenClassScope, "qExprTimePeriod", Constant(exprText)))
                .DeclareVar<double>("seconds", Constant(0))
                .DeclareVarNoInit(typeof(double?), "result");
            for (var i = 0; i < parent.ChildNodes.Length; i++) {
                var forge = parent.ChildNodes[i].Forge;
                var evaluationType = forge.EvaluationType;
                block.AssignRef(
                    "result",
                    SimpleNumberCoercerFactory.CoercerDouble.CodegenDoubleMayNullBoxedIncludeBig(
                        forge.EvaluateCodegen(evaluationType, methodNode, exprSymbol, codegenClassScope),
                        evaluationType,
                        methodNode,
                        codegenClassScope));
                block.IfRefNull("result")
                    .BlockThrow(
                        StaticMethod(
                            typeof(ExprTimePeriodForge),
                            "MakeTimePeriodParamNullException",
                            Constant(ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(parent))));
                block.AssignRef("seconds", Op(Ref("seconds"), "+", adders[i].ComputeCodegen(Ref("result"))));
            }

            block.Apply(InstrumentationCode.Instblock(codegenClassScope, "aExprTimePeriod", Ref("seconds")))
                .MethodReturn(Ref("seconds"));
            return LocalMethod(methodNode);
        }

        private double? Eval(
            ExprEvaluator expr,
            EventBean[] events,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return expr
                .Evaluate(events, isNewData, exprEvaluatorContext)
                .AsBoxedDouble();
        }

        public TimePeriod EvaluateGetTimePeriod(
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext context)
        {
            if (evaluators == null) {
                evaluators = ExprNodeUtilityQuery.GetEvaluatorsNoCompile(parent.ChildNodes);
            }

            var exprCtr = 0;
            int? year = null;
            if (parent.HasYear) {
                year = GetInt(evaluators[exprCtr++].Evaluate(eventsPerStream, newData, context));
            }

            int? month = null;
            if (parent.HasMonth) {
                month = GetInt(evaluators[exprCtr++].Evaluate(eventsPerStream, newData, context));
            }

            int? week = null;
            if (parent.HasWeek) {
                week = GetInt(evaluators[exprCtr++].Evaluate(eventsPerStream, newData, context));
            }

            int? day = null;
            if (parent.HasDay) {
                day = GetInt(evaluators[exprCtr++].Evaluate(eventsPerStream, newData, context));
            }

            int? hours = null;
            if (parent.HasHour) {
                hours = GetInt(evaluators[exprCtr++].Evaluate(eventsPerStream, newData, context));
            }

            int? minutes = null;
            if (parent.HasMinute) {
                minutes = GetInt(evaluators[exprCtr++].Evaluate(eventsPerStream, newData, context));
            }

            int? seconds = null;
            if (parent.HasSecond) {
                seconds = GetInt(evaluators[exprCtr++].Evaluate(eventsPerStream, newData, context));
            }

            int? milliseconds = null;
            if (parent.HasMillisecond) {
                milliseconds = GetInt(evaluators[exprCtr++].Evaluate(eventsPerStream, newData, context));
            }

            int? microseconds = null;
            if (parent.HasMicrosecond) {
                microseconds = GetInt(evaluators[exprCtr].Evaluate(eventsPerStream, newData, context));
            }

            return new TimePeriod(year, month, week, day, hours, minutes, seconds, milliseconds, microseconds);
        }

        public CodegenExpression EvaluateGetTimePeriodCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(
                typeof(TimePeriod),
                typeof(ExprTimePeriodForge),
                codegenClassScope);
            var block = methodNode.Block;
            var counter = 0;
            counter += EvaluateGetTimePeriodCodegenField(
                block,
                "year",
                parent.HasYear,
                counter,
                methodNode,
                exprSymbol,
                codegenClassScope);
            counter += EvaluateGetTimePeriodCodegenField(
                block,
                "month",
                parent.HasMonth,
                counter,
                methodNode,
                exprSymbol,
                codegenClassScope);
            counter += EvaluateGetTimePeriodCodegenField(
                block,
                "week",
                parent.HasWeek,
                counter,
                methodNode,
                exprSymbol,
                codegenClassScope);
            counter += EvaluateGetTimePeriodCodegenField(
                block,
                "day",
                parent.HasDay,
                counter,
                methodNode,
                exprSymbol,
                codegenClassScope);
            counter += EvaluateGetTimePeriodCodegenField(
                block,
                "hours",
                parent.HasHour,
                counter,
                methodNode,
                exprSymbol,
                codegenClassScope);
            counter += EvaluateGetTimePeriodCodegenField(
                block,
                "minutes",
                parent.HasMinute,
                counter,
                methodNode,
                exprSymbol,
                codegenClassScope);
            counter += EvaluateGetTimePeriodCodegenField(
                block,
                "seconds",
                parent.HasSecond,
                counter,
                methodNode,
                exprSymbol,
                codegenClassScope);
            counter += EvaluateGetTimePeriodCodegenField(
                block,
                "milliseconds",
                parent.HasMillisecond,
                counter,
                methodNode,
                exprSymbol,
                codegenClassScope);
            EvaluateGetTimePeriodCodegenField(
                block,
                "microseconds",
                parent.HasMicrosecond,
                counter,
                methodNode,
                exprSymbol,
                codegenClassScope);
            block.MethodReturn(
                NewInstance(
                    typeof(TimePeriod),
                    Ref("year"),
                    Ref("month"),
                    Ref("week"),
                    Ref("day"),
                    Ref("hours"),
                    Ref("minutes"),
                    Ref("seconds"),
                    Ref("milliseconds"),
                    Ref("microseconds")));
            return LocalMethod(methodNode);
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name = "expressionText">text</param>
        /// <returns>exception</returns>
        public static EPException MakeTimePeriodParamNullException(string expressionText)
        {
            return new EPException(
                "Failed to evaluate time period, received a null value for '" + expressionText + "'");
        }

        private int EvaluateGetTimePeriodCodegenField(
            CodegenBlock block,
            string variable,
            bool present,
            int counter,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            if (!present) {
                block.DeclareVar<int?>(variable, ConstantNull());
                return 0;
            }

            var forge = parent.ChildNodes[counter].Forge;
            var evaluationType = forge.EvaluationType;
            block.DeclareVar<int?>(
                variable,
                SimpleNumberCoercerFactory.CoercerInt.CoerceCodegenMayNull(
                    forge.EvaluateCodegen(evaluationType, codegenMethodScope, exprSymbol, codegenClassScope),
                    evaluationType,
                    codegenMethodScope,
                    codegenClassScope));
            return 1;
        }

        private int? GetInt(object evaluated)
        {
            if (evaluated == null) {
                return null;
            }

            return evaluated.AsInt32();
        }

        public ExprForgeConstantType ForgeConstantType => parent.IsConstantResult
            ? ExprForgeConstantType.COMPILETIMECONST
            : ExprForgeConstantType.NONCONST;

        public TimeAbacus TimeAbacus => parent.TimeAbacus;

        public ExprEvaluator ExprEvaluator {
            get {
                return new ProxyExprEvaluator() {
                    ProcEvaluate = (
                        eventsPerStream,
                        isNewData,
                        context) => throw new IllegalStateException(
                        "Time-Period expression must be evaluated via any of " +
                        nameof(ExprTimePeriod) +
                        " interface methods")
                };
            }
        }

        public Type EvaluationType => typeof(double?);

        public TimePeriodAdder[] Adders => adders;

        public ExprNodeRenderable ExprForgeRenderable => parent;

        public ExprEvaluator[] Evaluators {
            get {
                if (evaluators == null) {
                    evaluators = ExprNodeUtilityQuery.GetEvaluatorsNoCompile(parent.ChildNodes);
                }

                return evaluators;
            }
        }

        public ExprForge[] Forges => ExprNodeUtilityQuery.GetForges(parent.ChildNodes);
    }
} // end of namespace