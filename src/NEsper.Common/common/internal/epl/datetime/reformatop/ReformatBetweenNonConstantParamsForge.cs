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
using com.espertech.esper.common.@internal.epl.join.analyze;

namespace com.espertech.esper.common.@internal.epl.datetime.reformatop
{
    public class ReformatBetweenNonConstantParamsForge : ReformatForge
    {
        private readonly ExprNode _end;
        private readonly DatetimeLongCoercer _secondCoercer;

        private readonly ExprNode _start;
        private readonly DatetimeLongCoercer _startCoercer;
        private readonly ExprForge _forgeIncludeHigh;
        private readonly ExprForge _forgeIncludeLow;

        private readonly bool _includeBoth;
        private bool? _includeHigh;
        private bool? _includeLow;

        public ExprNode End => _end;

        public DatetimeLongCoercer SecondCoercer => _secondCoercer;

        public ExprNode Start => _start;

        public DatetimeLongCoercer StartCoercer => _startCoercer;

        public ExprForge ForgeIncludeHigh => _forgeIncludeHigh;

        public ExprForge ForgeIncludeLow => _forgeIncludeLow;

        public bool IncludeBoth => _includeBoth;

        public bool? IncludeHigh => _includeHigh;

        public bool? IncludeLow => _includeLow;

        public ReformatBetweenNonConstantParamsForge(IList<ExprNode> parameters)
        {
            _start = parameters[0];
            _startCoercer = DatetimeLongCoercerFactory.GetCoercer(_start.Forge.EvaluationType);
            _end = parameters[1];
            _secondCoercer = DatetimeLongCoercerFactory.GetCoercer(_end.Forge.EvaluationType);

            if (parameters.Count == 2) {
                _includeBoth = true;
                _includeLow = true;
                _includeHigh = true;
            }
            else {
                if (parameters[2].Forge.ForgeConstantType.IsCompileTimeConstant) {
                    _includeLow = GetBooleanValue(parameters[2]);
                }
                else {
                    _forgeIncludeLow = parameters[2].Forge;
                }

                if (parameters[3].Forge.ForgeConstantType.IsCompileTimeConstant) {
                    _includeHigh = GetBooleanValue(parameters[3]);
                }
                else {
                    _forgeIncludeHigh = parameters[3].Forge;
                }

                if (_includeLow.GetValueOrDefault(false) && _includeHigh.GetValueOrDefault(false)) {
                    _includeBoth = true;
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
                this,
                inner,
                codegenMethodScope,
                exprSymbol,
                codegenClassScope);
        }

        public CodegenExpression CodegenDateTime(
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ReformatBetweenNonConstantParamsForgeOp.CodegenDateTime(
                this,
                inner,
                codegenMethodScope,
                exprSymbol,
                codegenClassScope);
        }

        public CodegenExpression CodegenDateTimeOffset(
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ReformatBetweenNonConstantParamsForgeOp.CodegenDateTimeOffset(
                this,
                inner,
                codegenMethodScope,
                exprSymbol,
                codegenClassScope);
        }

        public CodegenExpression CodegenDateTimeEx(
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ReformatBetweenNonConstantParamsForgeOp.CodegenDateTimeEx(
                this,
                inner,
                codegenMethodScope,
                exprSymbol,
                codegenClassScope);
        }

        public Type ReturnType => typeof(bool?);

        public FilterExprAnalyzerAffector GetFilterDesc(
            EventType[] typesPerStream,
            DatetimeMethodDesc currentMethod,
            IList<ExprNode> currentParameters,
            ExprDotNodeFilterAnalyzerInput inputDesc)
        {
            if (_includeLow == null || _includeHigh == null) {
                return null;
            }

            int targetStreamNum;
            string targetProperty;
            if (inputDesc is ExprDotNodeFilterAnalyzerInputStream) {
                var targetStream = (ExprDotNodeFilterAnalyzerInputStream) inputDesc;
                targetStreamNum = targetStream.StreamNum;
                var targetType = typesPerStream[targetStreamNum];
                targetProperty = targetType.StartTimestampPropertyName;
            }
            else if (inputDesc is ExprDotNodeFilterAnalyzerInputProp) {
                var targetStream = (ExprDotNodeFilterAnalyzerInputProp) inputDesc;
                targetStreamNum = targetStream.StreamNum;
                targetProperty = targetStream.PropertyName;
            }
            else {
                return null;
            }

            return new FilterExprAnalyzerDTBetweenAffector(
                typesPerStream,
                targetStreamNum,
                targetProperty,
                _start,
                _end,
                _includeLow.Value,
                _includeHigh.Value);
        }

        public ReformatOp Op => new ReformatBetweenNonConstantParamsForgeOp(
            this,
            _start.Forge.ExprEvaluator,
            _end.Forge.ExprEvaluator,
            _forgeIncludeLow?.ExprEvaluator,
            _forgeIncludeHigh?.ExprEvaluator);

        private bool GetBooleanValue(ExprNode exprNode)
        {
            var value = exprNode.Forge.ExprEvaluator.Evaluate(null, true, null);
            if (value == null) {
                throw new ExprValidationException("Date-time method 'between' requires non-null parameter values");
            }

            return (bool) value;
        }
    }
} // end of namespace