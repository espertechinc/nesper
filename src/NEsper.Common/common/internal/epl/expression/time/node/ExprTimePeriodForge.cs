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
        private readonly ExprTimePeriodImpl _parent;
        private readonly bool _hasVariable;
        private readonly TimePeriodAdder[] _adders;
        private ExprEvaluator[] _evaluators;

        public ExprTimePeriodForge(
            ExprTimePeriodImpl parent,
            bool hasVariable,
            TimePeriodAdder[] adders)
        {
            _parent = parent;
            _hasVariable = hasVariable;
            _adders = adders;
        }

        public TimePeriodComputeForge ConstTimePeriodComputeForge()
        {
            if (!_parent.HasMonth && !_parent.HasYear) {
                var seconds = EvaluateAsSeconds(null, true, null);
                var msec = _parent.TimeAbacus.DeltaForSecondsDouble(seconds);
                return new TimePeriodComputeConstGivenDeltaForge(msec);
            }
            else {
                _evaluators = ExprNodeUtilityQuery.GetEvaluatorsNoCompile(_parent.ChildNodes);
                var values = new int[_adders.Length];
                for (var i = 0; i < values.Length; i++) {
                    values[i] = _evaluators[i].Evaluate(null, true, null).AsInt32();
                }

                return new TimePeriodComputeConstGivenCalAddForge(_adders, values, _parent.TimeAbacus);
            }
        }

        public TimePeriodComputeForge NonconstTimePeriodComputeForge()
        {
            if (!_parent.HasMonth && !_parent.HasYear) {
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

        public bool HasVariable => _hasVariable;

        public double EvaluateAsSeconds(
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext context)
        {
            if (_evaluators == null) {
                _evaluators = ExprNodeUtilityQuery.GetEvaluatorsNoCompile(_parent.ChildNodes);
            }

            double seconds = 0;
            for (var i = 0; i < _adders.Length; i++) {
                var result = Eval(_evaluators[i], eventsPerStream, newData, context);
                if (result == null) {
                    throw MakeTimePeriodParamNullException(
                        ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(_parent));
                }

                seconds += _adders[i].Compute(result.Value);
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
            
            for (var i = 0; i < _parent.ChildNodes.Length; i++) {
                var forge = _parent.ChildNodes[i].Forge;
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
                            Constant(ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(_parent))));
                block.AssignRef("seconds", Op(
                    Ref("seconds"), "+",
                    _adders[i].ComputeCodegen(Unbox(Ref("result")))));
            }

            block
                .Apply(InstrumentationCode.Instblock(codegenClassScope, "aExprTimePeriod", Ref("seconds")))
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
            if (_evaluators == null) {
                _evaluators = ExprNodeUtilityQuery.GetEvaluatorsNoCompile(_parent.ChildNodes);
            }

            var exprCtr = 0;
            int? year = null;
            if (_parent.HasYear) {
                year = GetInt(_evaluators[exprCtr++].Evaluate(eventsPerStream, newData, context));
            }

            int? month = null;
            if (_parent.HasMonth) {
                month = GetInt(_evaluators[exprCtr++].Evaluate(eventsPerStream, newData, context));
            }

            int? week = null;
            if (_parent.HasWeek) {
                week = GetInt(_evaluators[exprCtr++].Evaluate(eventsPerStream, newData, context));
            }

            int? day = null;
            if (_parent.HasDay) {
                day = GetInt(_evaluators[exprCtr++].Evaluate(eventsPerStream, newData, context));
            }

            int? hours = null;
            if (_parent.HasHour) {
                hours = GetInt(_evaluators[exprCtr++].Evaluate(eventsPerStream, newData, context));
            }

            int? minutes = null;
            if (_parent.HasMinute) {
                minutes = GetInt(_evaluators[exprCtr++].Evaluate(eventsPerStream, newData, context));
            }

            int? seconds = null;
            if (_parent.HasSecond) {
                seconds = GetInt(_evaluators[exprCtr++].Evaluate(eventsPerStream, newData, context));
            }

            int? milliseconds = null;
            if (_parent.HasMillisecond) {
                milliseconds = GetInt(_evaluators[exprCtr++].Evaluate(eventsPerStream, newData, context));
            }

            int? microseconds = null;
            if (_parent.HasMicrosecond) {
                microseconds = GetInt(_evaluators[exprCtr].Evaluate(eventsPerStream, newData, context));
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
                _parent.HasYear,
                counter,
                methodNode,
                exprSymbol,
                codegenClassScope);
            counter += EvaluateGetTimePeriodCodegenField(
                block,
                "month",
                _parent.HasMonth,
                counter,
                methodNode,
                exprSymbol,
                codegenClassScope);
            counter += EvaluateGetTimePeriodCodegenField(
                block,
                "week",
                _parent.HasWeek,
                counter,
                methodNode,
                exprSymbol,
                codegenClassScope);
            counter += EvaluateGetTimePeriodCodegenField(
                block,
                "day",
                _parent.HasDay,
                counter,
                methodNode,
                exprSymbol,
                codegenClassScope);
            counter += EvaluateGetTimePeriodCodegenField(
                block,
                "hours",
                _parent.HasHour,
                counter,
                methodNode,
                exprSymbol,
                codegenClassScope);
            counter += EvaluateGetTimePeriodCodegenField(
                block,
                "minutes",
                _parent.HasMinute,
                counter,
                methodNode,
                exprSymbol,
                codegenClassScope);
            counter += EvaluateGetTimePeriodCodegenField(
                block,
                "seconds",
                _parent.HasSecond,
                counter,
                methodNode,
                exprSymbol,
                codegenClassScope);
            counter += EvaluateGetTimePeriodCodegenField(
                block,
                "milliseconds",
                _parent.HasMillisecond,
                counter,
                methodNode,
                exprSymbol,
                codegenClassScope);
            EvaluateGetTimePeriodCodegenField(
                block,
                "microseconds",
                _parent.HasMicrosecond,
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

            var forge = _parent.ChildNodes[counter].Forge;
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

        public ExprForgeConstantType ForgeConstantType => _parent.IsConstantResult
            ? ExprForgeConstantType.COMPILETIMECONST
            : ExprForgeConstantType.NONCONST;

        public TimeAbacus TimeAbacus => _parent.TimeAbacus;

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

        public TimePeriodAdder[] Adders => _adders;

        public ExprNodeRenderable ExprForgeRenderable => _parent;

        public ExprEvaluator[] Evaluators {
            get {
                if (_evaluators == null) {
                    _evaluators = ExprNodeUtilityQuery.GetEvaluatorsNoCompile(_parent.ChildNodes);
                }

                return _evaluators;
            }
        }

        public ExprForge[] Forges => ExprNodeUtilityQuery.GetForges(_parent.ChildNodes);
    }
} // end of namespace