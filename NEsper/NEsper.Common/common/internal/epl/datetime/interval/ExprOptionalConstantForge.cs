///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.datetime.interval
{
    public class ExprOptionalConstantForge
    {
        public static readonly IntervalDeltaExprMaxForge MAXFORGE = new IntervalDeltaExprMaxForge();
        public static readonly IntervalDeltaExprMaxEval MAXEVAL = new IntervalDeltaExprMaxEval();

        public ExprOptionalConstantForge(
            IntervalDeltaExprForge forge,
            long? optionalConstant)
        {
            Forge = forge;
            OptionalConstant = optionalConstant;
        }

        public long? OptionalConstant { get; }

        public IntervalDeltaExprForge Forge { get; }

        public static ExprOptionalConstantForge Make(long maxValue)
        {
            return new ExprOptionalConstantForge(MAXFORGE, maxValue);
        }

        public ExprOptionalConstantEval MakeEval()
        {
            return new ExprOptionalConstantEval(Forge.MakeEvaluator(), OptionalConstant);
        }

        public class IntervalDeltaExprMaxForge : IntervalDeltaExprForge
        {
            public IntervalDeltaExprEvaluator MakeEvaluator()
            {
                return MAXEVAL;
            }

            public CodegenExpression Codegen(
                CodegenExpression reference,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return Constant(long.MaxValue);
            }
        }

        public class IntervalDeltaExprMaxEval : IntervalDeltaExprEvaluator
        {
            public long Evaluate(
                long reference,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext context)
            {
                return long.MaxValue;
            }
        }
    }
} // end of namespace