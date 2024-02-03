///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.datetimemethod;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.datetime.eval;
using com.espertech.esper.common.@internal.epl.datetime.reformatop;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.epl.join.analyze;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.epl.datetime.plugin.DTMPluginUtil;

namespace com.espertech.esper.common.@internal.epl.datetime.plugin
{
    public class DTMPluginReformatForge : ReformatForge
    {
        private readonly DateTimeMethodOpsReformat _reformatOp;
        private readonly IList<ExprNode> _reformatOpParams;

        public DTMPluginReformatForge(
            Type inputType,
            DateTimeMethodOpsReformat reformatOp,
            IList<ExprNode> reformatOpParams)
        {
			_reformatOp = reformatOp;
			_reformatOpParams = reformatOpParams;
            ValidateDTMStaticMethodAllowNull(inputType, reformatOp.LongOp, typeof(long), reformatOpParams);
			ValidateDTMStaticMethodAllowNull(inputType, reformatOp.DateTimeExOp, typeof(DateTimeEx), reformatOpParams);
			ValidateDTMStaticMethodAllowNull(inputType, reformatOp.DateTimeOffsetOp, typeof(DateTimeOffset), reformatOpParams);
			ValidateDTMStaticMethodAllowNull(inputType, reformatOp.DateTimeOp, typeof(DateTime), reformatOpParams);
			
			if (reformatOp.ReturnType == null || reformatOp.ReturnType == typeof(void)) {
                throw new ExprValidationException(
                    "Invalid return type for reformat operation, return type is " + reformatOp.ReturnType);
            }
        }

		public ReformatOp Op => throw new UnsupportedOperationException("Evaluation not available at compile-time");

		public Type ReturnType => _reformatOp.ReturnType;

        public FilterExprAnalyzerAffector GetFilterDesc(
            EventType[] typesPerStream,
            DatetimeMethodDesc currentMethod,
            IList<ExprNode> currentParameters,
            ExprDotNodeFilterAnalyzerInput inputDesc)
        {
            return null;
        }

        public CodegenExpression CodegenLong(
            CodegenExpression inner,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope)
        {
            return CodegenPluginDTM(
                _reformatOp.LongOp,
                ReturnType,
                typeof(long),
                inner,
                _reformatOpParams,
                parent,
                symbols,
                classScope);
        }

		public CodegenExpression CodegenDateTimeEx(
            CodegenExpression inner,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope)
        {
            return CodegenPluginDTM(
				_reformatOp.DateTimeExOp, ReturnType, typeof(DateTimeEx), inner, _reformatOpParams, parent, symbols, classScope);
        }

		public CodegenExpression CodegenDateTimeOffset(
            CodegenExpression inner,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope)
        {
            return CodegenPluginDTM(
				_reformatOp.DateTimeOffsetOp, ReturnType, typeof(DateTimeOffset), inner, _reformatOpParams, parent, symbols, classScope);
        }

		public CodegenExpression CodegenDateTime(
            CodegenExpression inner,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope)
        {
            return CodegenPluginDTM(
				_reformatOp.DateTimeOp, ReturnType, typeof(DateTime), inner, _reformatOpParams, parent, symbols, classScope);
        }
    }
} // end of namespace