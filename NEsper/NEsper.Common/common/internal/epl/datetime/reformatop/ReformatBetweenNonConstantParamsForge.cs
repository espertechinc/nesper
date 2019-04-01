///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.datetime.eval;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.epl.@join.analyze;

namespace com.espertech.esper.common.@internal.epl.datetime.reformatop
{
    public class ReformatBetweenNonConstantParamsForge : ReformatForge
    {
        protected internal readonly ExprNode end;
        protected internal readonly DatetimeLongCoercer secondCoercer;

        protected internal readonly ExprNode start;
        protected internal readonly DatetimeLongCoercer startCoercer;
        protected internal ExprForge forgeIncludeHigh;
        protected internal ExprForge forgeIncludeLow;

        protected internal bool includeBoth;
        protected internal bool? includeHigh;
        protected internal bool? includeLow;

        public ReformatBetweenNonConstantParamsForge(IList<ExprNode> parameters)
        {
            start = parameters[0];
            startCoercer = DatetimeLongCoercerFactory.GetCoercer(start.Forge.EvaluationType);
            end = parameters[1];
            secondCoercer = DatetimeLongCoercerFactory.GetCoercer(end.Forge.EvaluationType);

            if (parameters.Count == 2)
            {
                includeBoth = true;
                includeLow = true;
                includeHigh = true;
            }
            else
            {
                if (parameters[2].Forge.ForgeConstantType.IsCompileTimeConstant)
                {
                    includeLow = GetBooleanValue(parameters[2]);
                }
                else
                {
                    forgeIncludeLow = parameters[2].Forge;
                }

                if (parameters[3].Forge.ForgeConstantType.IsCompileTimeConstant)
                {
                    includeHigh = GetBooleanValue(parameters[3]);
                }
                else
                {
                    forgeIncludeHigh = parameters[3].Forge;
                }

                if (includeLow.GetValueOrDefault(false) && includeHigh.GetValueOrDefault(false))
                {
                    includeBoth = true;
                }
            }
        }

        public CodegenExpression CodegenLong(
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ReformatBetweenNonConstantParamsForgeOp.CodegenLong(
                this, inner, codegenMethodScope, exprSymbol, codegenClassScope);
        }

        public CodegenExpression CodegenDateTime(
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ReformatBetweenNonConstantParamsForgeOp.CodegenDateTime(
                this, inner, codegenMethodScope, exprSymbol, codegenClassScope);
        }

        public CodegenExpression CodegenDateTimeOffset(
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ReformatBetweenNonConstantParamsForgeOp.CodegenDateTimeOffset(
                this, inner, codegenMethodScope, exprSymbol, codegenClassScope);
        }

        public CodegenExpression CodegenDateTimeEx(
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ReformatBetweenNonConstantParamsForgeOp.CodegenDateTimeEx(
                this, inner, codegenMethodScope, exprSymbol, codegenClassScope);
        }

        public Type ReturnType => typeof(bool?);

        public FilterExprAnalyzerAffector GetFilterDesc(
            EventType[] typesPerStream,
            DatetimeMethodEnum currentMethod,
            IList<ExprNode> currentParameters,
            ExprDotNodeFilterAnalyzerInput inputDesc)
        {
            if (includeLow == null || includeHigh == null)
            {
                return null;
            }

            int targetStreamNum;
            string targetProperty;
            if (inputDesc is ExprDotNodeFilterAnalyzerInputStream)
            {
                var targetStream = (ExprDotNodeFilterAnalyzerInputStream) inputDesc;
                targetStreamNum = targetStream.StreamNum;
                var targetType = typesPerStream[targetStreamNum];
                targetProperty = targetType.StartTimestampPropertyName;
            }
            else if (inputDesc is ExprDotNodeFilterAnalyzerInputProp)
            {
                var targetStream = (ExprDotNodeFilterAnalyzerInputProp) inputDesc;
                targetStreamNum = targetStream.StreamNum;
                targetProperty = targetStream.PropertyName;
            }
            else
            {
                return null;
            }

            return new FilterExprAnalyzerDTBetweenAffector(
                typesPerStream, targetStreamNum, targetProperty, start, end, includeLow.Value, includeHigh.Value);
        }

        public ReformatOp Op => new ReformatBetweenNonConstantParamsForgeOp(
            this,
            start.Forge.ExprEvaluator,
            end.Forge.ExprEvaluator,
            forgeIncludeLow?.ExprEvaluator,
            forgeIncludeHigh?.ExprEvaluator);

        private bool GetBooleanValue(ExprNode exprNode)
        {
            var value = exprNode.Forge.ExprEvaluator.Evaluate(null, true, null);
            if (value == null)
            {
                throw new ExprValidationException("Date-time method 'between' requires non-null parameter values");
            }

            return (bool) value;
        }
    }
} // end of namespace