///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Numerics;
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
    /// <para />Child nodes to this expression carry the actual parts and must return a numeric value.
    /// </summary>
    public class ExprTimePeriodForge : ExprForge
    {
        private ExprEvaluator[] evaluators;

        public ExprTimePeriodForge(
            ExprTimePeriodImpl parent,
            bool hasVariable,
            TimePeriodAdder[] adders)
        {
            ForgeRenderable = parent;
            HasVariable = hasVariable;
            Adders = adders;
        }

        public ExprForgeConstantType ForgeConstantType {
            get => ForgeRenderable.IsConstantResult
                ? ExprForgeConstantType.COMPILETIMECONST
                : ExprForgeConstantType.NONCONST;
        }

        public TimePeriodComputeForge ConstTimePeriodComputeForge()
        {
            if (!ForgeRenderable.HasMonth && !ForgeRenderable.HasYear) {
                var seconds = EvaluateAsSeconds(null, true, null);
                var msec = ForgeRenderable.TimeAbacus.DeltaForSecondsDouble(seconds);
                return new TimePeriodComputeConstGivenDeltaForge(msec);
            }
            else {
                evaluators = ExprNodeUtilityQuery.GetEvaluatorsNoCompile(ForgeRenderable.ChildNodes);
                var values = new int[Adders.Length];
                for (var i = 0; i < values.Length; i++) {
                    values[i] = evaluators[i].Evaluate(null, true, null).AsInt();
                }

                return new TimePeriodComputeConstGivenCalAddForge(Adders, values, ForgeRenderable.TimeAbacus);
            }
        }

        public TimePeriodComputeForge NonconstTimePeriodComputeForge()
        {
            if (!ForgeRenderable.HasMonth && !ForgeRenderable.HasYear) {
                return new TimePeriodComputeNCGivenTPNonCalForge(this);
            }
            else {
                return new TimePeriodComputeNCGivenTPCalForge(this);
            }
        }

        public TimeAbacus TimeAbacus {
            get => ForgeRenderable.TimeAbacus;
        }

        public ExprEvaluator ExprEvaluator {
            get {
                return new ProxyExprEvaluator() {
                    ProcEvaluate = (
                        eventsPerStream,
                        isNewData,
                        context) => {
                        throw new IllegalStateException(
                            "Time-Period expression must be evaluated via any of " +
                            typeof(ExprTimePeriod).GetSimpleName() +
                            " interface methods");
                    }
                };
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

        public Type EvaluationType => typeof(double?);

        public bool HasVariable { get; }

        public TimePeriodAdder[] Adders { get; }

        public ExprTimePeriodImpl ForgeRenderable { get; }

        ExprNodeRenderable ExprForge.ForgeRenderable => ForgeRenderable;

        public ExprEvaluator[] Evaluators {
            get {
                if (evaluators == null) {
                    evaluators = ExprNodeUtilityQuery.GetEvaluatorsNoCompile(ForgeRenderable.ChildNodes);
                }

                return evaluators;
            }
        }

        public double EvaluateAsSeconds(
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext context)
        {
            if (evaluators == null) {
                evaluators = ExprNodeUtilityQuery.GetEvaluatorsNoCompile(ForgeRenderable.ChildNodes);
            }

            double seconds = 0;
            for (int i = 0; i < Adders.Length; i++) {
                var result = Eval(evaluators[i], eventsPerStream, newData, context);
                if (result == null) {
                    throw MakeTimePeriodParamNullException(
                        ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(ForgeRenderable));
                }

                seconds += Adders[i].Compute(result.Value);
            }

            return seconds;
        }

        public CodegenExpression EvaluateAsSecondsCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            CodegenMethod methodNode = codegenMethodScope.MakeChild(
                typeof(double), typeof(ExprTimePeriodForge), codegenClassScope);

            string exprText = null;
            if (codegenClassScope.IsInstrumented) {
                exprText = ExprNodeUtilityPrint.ToExpressionStringMinPrecedence(this);
            }

            CodegenBlock block = methodNode.Block
                .Apply(InstrumentationCode.Instblock(codegenClassScope, "qExprTimePeriod", Constant(exprText)))
                .DeclareVar(typeof(double), "seconds", Constant(0))
                .DeclareVarNoInit(typeof(double?), "result");
            for (int i = 0; i < ForgeRenderable.ChildNodes.Length; i++) {
                ExprForge forge = ForgeRenderable.ChildNodes[i].Forge;
                Type evaluationType = forge.EvaluationType;
                block.AssignRef(
                    "result",
                    SimpleNumberCoercerFactory.CoercerDouble.CodegenDoubleMayNullBoxedIncludeBig(
                        forge.EvaluateCodegen(evaluationType, methodNode, exprSymbol, codegenClassScope),
                        evaluationType, methodNode, codegenClassScope));
                block.IfRefNull("result").BlockThrow(
                    StaticMethod(
                        typeof(ExprTimePeriodForge), "makeTimePeriodParamNullException",
                        Constant(ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(ForgeRenderable))));
                block.AssignRef("seconds", Op(@Ref("seconds"), "+", Adders[i].ComputeCodegen(@Ref("result"))));
            }

            block.Apply(InstrumentationCode.Instblock(codegenClassScope, "aExprTimePeriod", @Ref("seconds")))
                .MethodReturn(@Ref("seconds"));
            return LocalMethod(methodNode);
        }

        private double? Eval(
            ExprEvaluator expr,
            EventBean[] events,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            object value = expr.Evaluate(events, isNewData, exprEvaluatorContext);
            if (value == null) {
                return null;
            }

            if (value is decimal) {
                return value.AsDecimal().AsDouble();
            }

            if (value is BigInteger) {
                return value.AsBigInteger().AsDouble();
            }

            return value.AsDouble();
        }

        public TimePeriod EvaluateGetTimePeriod(
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext context)
        {
            if (evaluators == null) {
                evaluators = ExprNodeUtilityQuery.GetEvaluatorsNoCompile(ForgeRenderable.ChildNodes);
            }

            int exprCtr = 0;

            int? year = null;
            if (ForgeRenderable.HasYear) {
                year = GetInt(evaluators[exprCtr++].Evaluate(eventsPerStream, newData, context));
            }

            int? month = null;
            if (ForgeRenderable.HasMonth) {
                month = GetInt(evaluators[exprCtr++].Evaluate(eventsPerStream, newData, context));
            }

            int? week = null;
            if (ForgeRenderable.HasWeek) {
                week = GetInt(evaluators[exprCtr++].Evaluate(eventsPerStream, newData, context));
            }

            int? day = null;
            if (ForgeRenderable.HasDay) {
                day = GetInt(evaluators[exprCtr++].Evaluate(eventsPerStream, newData, context));
            }

            int? hours = null;
            if (ForgeRenderable.HasHour) {
                hours = GetInt(evaluators[exprCtr++].Evaluate(eventsPerStream, newData, context));
            }

            int? minutes = null;
            if (ForgeRenderable.HasMinute) {
                minutes = GetInt(evaluators[exprCtr++].Evaluate(eventsPerStream, newData, context));
            }

            int? seconds = null;
            if (ForgeRenderable.HasSecond) {
                seconds = GetInt(evaluators[exprCtr++].Evaluate(eventsPerStream, newData, context));
            }

            int? milliseconds = null;
            if (ForgeRenderable.HasMillisecond) {
                milliseconds = GetInt(evaluators[exprCtr++].Evaluate(eventsPerStream, newData, context));
            }

            int? microseconds = null;
            if (ForgeRenderable.HasMicrosecond) {
                microseconds = GetInt(evaluators[exprCtr].Evaluate(eventsPerStream, newData, context));
            }

            return new TimePeriod(year, month, week, day, hours, minutes, seconds, milliseconds, microseconds);
        }

        public CodegenExpression EvaluateGetTimePeriodCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            CodegenMethod methodNode = codegenMethodScope.MakeChild(
                typeof(TimePeriod), typeof(ExprTimePeriodForge), codegenClassScope);

            CodegenBlock block = methodNode.Block;
            int counter = 0;
            counter += EvaluateGetTimePeriodCodegenField(
                block, "year", ForgeRenderable.HasYear, counter, methodNode, exprSymbol, codegenClassScope);
            counter += EvaluateGetTimePeriodCodegenField(
                block, "month", ForgeRenderable.HasMonth, counter, methodNode, exprSymbol, codegenClassScope);
            counter += EvaluateGetTimePeriodCodegenField(
                block, "week", ForgeRenderable.HasWeek, counter, methodNode, exprSymbol, codegenClassScope);
            counter += EvaluateGetTimePeriodCodegenField(
                block, "day", ForgeRenderable.HasDay, counter, methodNode, exprSymbol, codegenClassScope);
            counter += EvaluateGetTimePeriodCodegenField(
                block, "hours", ForgeRenderable.HasHour, counter, methodNode, exprSymbol, codegenClassScope);
            counter += EvaluateGetTimePeriodCodegenField(
                block, "minutes", ForgeRenderable.HasMinute, counter, methodNode, exprSymbol, codegenClassScope);
            counter += EvaluateGetTimePeriodCodegenField(
                block, "seconds", ForgeRenderable.HasSecond, counter, methodNode, exprSymbol, codegenClassScope);
            counter += EvaluateGetTimePeriodCodegenField(
                block, "milliseconds", ForgeRenderable.HasMillisecond, counter, methodNode, exprSymbol,
                codegenClassScope);
            EvaluateGetTimePeriodCodegenField(
                block, "microseconds", ForgeRenderable.HasMicrosecond, counter, methodNode, exprSymbol,
                codegenClassScope);
            block.MethodReturn(
                NewInstance(
                    typeof(TimePeriod), @Ref("year"), @Ref("month"), @Ref("week"), @Ref("day"), @Ref("hours"),
                    @Ref("minutes"), @Ref("seconds"), @Ref("milliseconds"), @Ref("microseconds")));
            return LocalMethod(methodNode);
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="expressionText">text</param>
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
                block.DeclareVar(typeof(int?), variable, ConstantNull());
                return 0;
            }

            var forge = ForgeRenderable.ChildNodes[counter].Forge;
            var evaluationType = forge.EvaluationType;
            block.DeclareVar(
                typeof(int?), variable,
                SimpleNumberCoercerFactory.CoercerInt.CoerceCodegenMayNull(
                    forge.EvaluateCodegen(evaluationType, codegenMethodScope, exprSymbol, codegenClassScope),
                    forge.EvaluationType, codegenMethodScope, codegenClassScope));
            return 1;
        }

        private int? GetInt(object evaluated)
        {
            if (evaluated == null) {
                return null;
            }

            return evaluated.AsInt();
        }

        public ExprForge[] Forges => ExprNodeUtilityQuery.GetForges(ForgeRenderable.ChildNodes);
    }
} // end of namespace