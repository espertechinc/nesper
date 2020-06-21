///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.IO;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.epl.expression.time.adder;
using com.espertech.esper.common.@internal.epl.expression.time.eval;
using com.espertech.esper.common.@internal.epl.expression.variable;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;

namespace com.espertech.esper.common.@internal.epl.expression.time.node
{
    /// <summary>
    ///     Expression representing a time period.
    ///     <para />
    ///     Child nodes to this expression carry the actual parts and must return a numeric value.
    /// </summary>
    public class ExprTimePeriodImpl : ExprNodeBase,
        ExprTimePeriod,
        TimePeriodEval
    {
        private ExprTimePeriodForge forge;

        public ExprTimePeriodImpl(
            bool hasYear,
            bool hasMonth,
            bool hasWeek,
            bool hasDay,
            bool hasHour,
            bool hasMinute,
            bool hasSecond,
            bool hasMillisecond,
            bool hasMicrosecond,
            TimeAbacus timeAbacus)
        {
            HasYear = hasYear;
            HasMonth = hasMonth;
            HasWeek = hasWeek;
            HasDay = hasDay;
            HasHour = hasHour;
            HasMinute = hasMinute;
            HasSecond = hasSecond;
            HasMillisecond = hasMillisecond;
            HasMicrosecond = hasMicrosecond;
            TimeAbacus = timeAbacus;
        }

        public ExprEvaluator ExprEvaluator {
            get {
                CheckValidated(forge);
                return forge.ExprEvaluator;
            }
        }

        public TimeAbacus TimeAbacus { get; }

        /// <summary>
        ///     Indicator whether the time period has a day part child expression.
        /// </summary>
        /// <returns>true for part present, false for not present</returns>
        public bool HasDay { get; }

        /// <summary>
        ///     Indicator whether the time period has a hour part child expression.
        /// </summary>
        /// <returns>true for part present, false for not present</returns>
        public bool HasHour { get; }

        /// <summary>
        ///     Indicator whether the time period has a minute part child expression.
        /// </summary>
        /// <returns>true for part present, false for not present</returns>
        public bool HasMinute { get; }

        /// <summary>
        ///     Indicator whether the time period has a second part child expression.
        /// </summary>
        /// <returns>true for part present, false for not present</returns>
        public bool HasSecond { get; }

        /// <summary>
        ///     Indicator whether the time period has a millisecond part child expression.
        /// </summary>
        /// <returns>true for part present, false for not present</returns>
        public bool HasMillisecond { get; }

        public bool HasMicrosecond { get; }

        /// <summary>
        ///     Indicator whether the time period has a year part child expression.
        /// </summary>
        /// <returns>true for part present, false for not present</returns>
        public bool HasYear { get; }

        /// <summary>
        ///     Indicator whether the time period has a month part child expression.
        /// </summary>
        /// <returns>true for part present, false for not present</returns>
        public bool HasMonth { get; }

        /// <summary>
        ///     Indicator whether the time period has a week part child expression.
        /// </summary>
        /// <returns>true for part present, false for not present</returns>
        public bool HasWeek { get; }

        public override ExprForge Forge {
            get {
                CheckValidated(forge);
                return forge;
            }
        }

        TimePeriodEval ExprTimePeriod.TimePeriodEval => this;

        public TimePeriodComputeForge TimePeriodComputeForge {
            get {
                CheckValidated(forge);
                if (IsConstantResult) {
                    return forge.ConstTimePeriodComputeForge();
                }

                return forge.NonconstTimePeriodComputeForge();
            }
        }

        public CodegenExpression EvaluateGetTimePeriodCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            CheckValidated(forge);
            return forge.EvaluateGetTimePeriodCodegen(codegenMethodScope, exprSymbol, codegenClassScope);
        }

        public CodegenExpression MakeTimePeriodAnonymous(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            //var timeClass = NewAnonymousClass(method.Block, typeof(TimePeriodEval));
            //var evalMethod = CodegenMethod.MakeMethod(typeof(TimePeriod), GetType(), classScope)
			//	.AddParam(PARAMS);
            //timeClass.AddMethod("timePeriodEval", evalMethod);

            var evalMethod = new CodegenExpressionLambda(method.Block).WithParams(PARAMS);
            var timeClass = NewInstance<ProxyTimePeriodEval>(evalMethod);

            var exprSymbol = new ExprForgeCodegenSymbol(true, true);
            //var exprMethod = evalMethod.MakeChildWithScope(typeof(TimePeriod), GetType(), exprSymbol, classScope);
            var exprMethod = method.MakeChild(typeof(TimePeriod), GetType(), classScope)
                .AddParam(PARAMS);

            CodegenExpression expression = forge.EvaluateGetTimePeriodCodegen(exprMethod, exprSymbol, classScope);
            //exprSymbol.DerivedSymbolsCodegen(evalMethod, exprMethod.Block, classScope);
            exprSymbol.DerivedSymbolsCodegen(exprMethod, exprMethod.Block, classScope);
            exprMethod.Block.MethodReturn(expression);

            evalMethod.Block.BlockReturn(LocalMethod(exprMethod, REF_EPS, REF_ISNEWDATA, REF_EXPREVALCONTEXT));

            return timeClass;
        }

        public CodegenExpression EvaluateAsSecondsCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            CheckValidated(forge);
            return forge.EvaluateAsSecondsCodegen(codegenMethodScope, exprSymbol, codegenClassScope);
        }

        /// <summary>
        ///     Indicator whether the time period has a variable in any of the child expressions.
        /// </summary>
        /// <value>true for variable present, false for not present</value>
        public bool HasVariable {
            get {
                CheckValidated(forge);
                return forge.HasVariable;
            }
        }

        public TimePeriod EvaluateGetTimePeriod(
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext context)
        {
            CheckValidated(forge);
            return forge.EvaluateGetTimePeriod(eventsPerStream, newData, context);
        }

        public double EvaluateAsSeconds(
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext context)
        {
            CheckValidated(forge);
            return forge.EvaluateAsSeconds(eventsPerStream, newData, context);
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            var hasVariables = false;
            foreach (var childNode in ChildNodes) {
                hasVariables |= Validate(childNode);
            }

            var list = new ArrayDeque<TimePeriodAdder>();
            if (HasYear) {
                list.Add(TimePeriodAdderYear.INSTANCE);
            }

            if (HasMonth) {
                list.Add(TimePeriodAdderMonth.INSTANCE);
            }

            if (HasWeek) {
                list.Add(TimePeriodAdderWeek.INSTANCE);
            }

            if (HasDay) {
                list.Add(TimePeriodAdderDay.INSTANCE);
            }

            if (HasHour) {
                list.Add(TimePeriodAdderHour.INSTANCE);
            }

            if (HasMinute) {
                list.Add(TimePeriodAdderMinute.INSTANCE);
            }

            if (HasSecond) {
                list.Add(TimePeriodAdderSecond.INSTANCE);
            }

            if (HasMillisecond) {
                list.Add(TimePeriodAdderMSec.INSTANCE);
            }

            if (HasMicrosecond) {
                list.Add(TimePeriodAdderUSec.INSTANCE);
            }

            var adders = list.ToArray();
            forge = new ExprTimePeriodForge(this, hasVariables, adders);
            return null;
        }

        public bool IsConstantResult {
            get {
                foreach (var child in ChildNodes) {
                    if (!child.Forge.ForgeConstantType.IsCompileTimeConstant) {
                        return false;
                    }
                }

                return true;
            }
        }

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is ExprTimePeriodImpl)) {
                return false;
            }

            var other = (ExprTimePeriodImpl) node;

            if (HasYear != other.HasYear) {
                return false;
            }

            if (HasMonth != other.HasMonth) {
                return false;
            }

            if (HasWeek != other.HasWeek) {
                return false;
            }

            if (HasDay != other.HasDay) {
                return false;
            }

            if (HasHour != other.HasHour) {
                return false;
            }

            if (HasMinute != other.HasMinute) {
                return false;
            }

            if (HasSecond != other.HasSecond) {
                return false;
            }

            if (HasMillisecond != other.HasMillisecond) {
                return false;
            }

            return HasMicrosecond == other.HasMicrosecond;
        }

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            var exprCtr = 0;
            var delimiter = "";
            if (HasYear) {
                ChildNodes[exprCtr++].ToEPL(writer, Precedence, flags);
                writer.Write(" years");
                delimiter = " ";
            }

            if (HasMonth) {
                writer.Write(delimiter);
                ChildNodes[exprCtr++].ToEPL(writer, Precedence, flags);
                writer.Write(" months");
                delimiter = " ";
            }

            if (HasWeek) {
                writer.Write(delimiter);
                ChildNodes[exprCtr++].ToEPL(writer, Precedence, flags);
                writer.Write(" weeks");
                delimiter = " ";
            }

            if (HasDay) {
                writer.Write(delimiter);
                ChildNodes[exprCtr++].ToEPL(writer, Precedence, flags);
                writer.Write(" days");
                delimiter = " ";
            }

            if (HasHour) {
                writer.Write(delimiter);
                ChildNodes[exprCtr++].ToEPL(writer, Precedence, flags);
                writer.Write(" hours");
                delimiter = " ";
            }

            if (HasMinute) {
                writer.Write(delimiter);
                ChildNodes[exprCtr++].ToEPL(writer, Precedence, flags);
                writer.Write(" minutes");
                delimiter = " ";
            }

            if (HasSecond) {
                writer.Write(delimiter);
                ChildNodes[exprCtr++].ToEPL(writer, Precedence, flags);
                writer.Write(" seconds");
                delimiter = " ";
            }

            if (HasMillisecond) {
                writer.Write(delimiter);
                ChildNodes[exprCtr++].ToEPL(writer, Precedence, flags);
                writer.Write(" milliseconds");
                delimiter = " ";
            }

            if (HasMicrosecond) {
                writer.Write(delimiter);
                ChildNodes[exprCtr].ToEPL(writer, Precedence, flags);
                writer.Write(" microseconds");
            }
        }

        public TimePeriod TimePeriodEval(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return EvaluateGetTimePeriod(eventsPerStream, isNewData, exprEvaluatorContext);
        }

        private bool Validate(ExprNode expression)
        {
            if (expression == null) {
                return false;
            }

            var returnType = expression.Forge.EvaluationType;
            if (!returnType.IsNumeric()) {
                throw new ExprValidationException("Time period expression requires a numeric parameter type");
            }

            if ((HasMonth || HasYear) && returnType.GetBoxedType() != typeof(int?)) {
                throw new ExprValidationException(
                    "Time period expressions with month or year component require integer values, received a " +
                    returnType.GetSimpleName() +
                    " value");
            }

            return expression is ExprVariableNode;
        }
    }
} // end of namespace