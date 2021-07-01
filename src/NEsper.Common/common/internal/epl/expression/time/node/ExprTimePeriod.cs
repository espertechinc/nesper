///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.time.eval;

namespace com.espertech.esper.common.@internal.epl.expression.time.node
{
    /// <summary>
    ///     Expression representing a time period.
    ///     <para />
    ///     Child nodes to this expression carry the actual parts and must return a numeric value.
    /// </summary>
    public interface ExprTimePeriod : ExprNode
    {
        bool HasVariable { get; }

        TimePeriodComputeForge TimePeriodComputeForge { get; }

        TimePeriodEval TimePeriodEval { get; }

        /// <summary>
        ///     Indicator whether the time period has a day part child expression.
        /// </summary>
        /// <value>true for part present, false for not present</value>
        bool HasDay { get; }

        /// <summary>
        ///     Indicator whether the time period has a hour part child expression.
        /// </summary>
        /// <value>true for part present, false for not present</value>
        bool HasHour { get; }

        /// <summary>
        ///     Indicator whether the time period has a minute part child expression.
        /// </summary>
        /// <value>true for part present, false for not present</value>
        bool HasMinute { get; }

        /// <summary>
        ///     Indicator whether the time period has a second part child expression.
        /// </summary>
        /// <value>true for part present, false for not present</value>
        bool HasSecond { get; }

        /// <summary>
        ///     Indicator whether the time period has a millisecond part child expression.
        /// </summary>
        /// <value>true for part present, false for not present</value>
        bool HasMillisecond { get; }

        /// <summary>
        ///     Indicator whether the time period has a microsecond part child expression.
        /// </summary>
        /// <value>true for part present, false for not present</value>
        bool HasMicrosecond { get; }

        /// <summary>
        ///     Indicator whether the time period has a year part child expression.
        /// </summary>
        /// <value>true for part present, false for not present</value>
        bool HasYear { get; }

        /// <summary>
        ///     Indicator whether the time period has a month part child expression.
        /// </summary>
        /// <value>true for part present, false for not present</value>
        bool HasMonth { get; }

        /// <summary>
        ///     Indicator whether the time period has a week part child expression.
        /// </summary>
        /// <value>true for part present, false for not present</value>
        bool HasWeek { get; }

        bool IsConstantResult { get; }

        double EvaluateAsSeconds(
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext context);

        TimePeriod EvaluateGetTimePeriod(
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext context);

        CodegenExpression EvaluateGetTimePeriodCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope);

        CodegenExpression EvaluateAsSecondsCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope);

        CodegenExpression MakeTimePeriodAnonymous(
            CodegenMethod method,
            CodegenClassScope classScope);
    }
} // end of namespace