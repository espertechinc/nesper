///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.hook.datetimemethod;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.datetime.calop;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.epl.datetime.plugin.DTMPluginUtil;

namespace com.espertech.esper.common.@internal.epl.datetime.plugin
{
	public class DTMPluginValueChangeForge : CalendarForge
	{

		private readonly DateTimeMethodOpsModify _transformOp;
		private readonly IList<ExprNode> _transformOpParams;

		public DTMPluginValueChangeForge(
			Type inputType,
			DateTimeMethodOpsModify transformOp,
			IList<ExprNode> transformOpParams)
		{
			_transformOp = transformOp;
			_transformOpParams = transformOpParams;
			ValidateDTMStaticMethodAllowNull(inputType, transformOp.DateTimeExOp, typeof(DateTimeEx), transformOpParams);
			ValidateDTMStaticMethodAllowNull(inputType, transformOp.DateTimeOffsetOp, typeof(DateTimeOffset), transformOpParams);
			ValidateDTMStaticMethodAllowNull(inputType, transformOp.DateTimeOp, typeof(DateTime), transformOpParams);
		}

		public CalendarOp EvalOp {
			get { throw new UnsupportedOperationException("Evaluation not available at compile-time"); }
		}

		public CodegenExpression CodegenDateTimeEx(
			CodegenExpression dateTimeEx,
			CodegenMethodScope parent,
			ExprForgeCodegenSymbol symbols,
			CodegenClassScope classScope)
		{
			return CodegenPluginDTM(
				_transformOp.DateTimeExOp,
				typeof(DateTimeEx),
				typeof(DateTimeEx),
				dateTimeEx,
				_transformOpParams,
				parent,
				symbols,
				classScope);
		}

		public CodegenExpression CodegenDateTimeOffset(
			CodegenExpression dateTimeOffset,
			CodegenMethodScope parent,
			ExprForgeCodegenSymbol symbols,
			CodegenClassScope classScope)
		{
			return CodegenPluginDTM(
				_transformOp.DateTimeOffsetOp,
				typeof(DateTimeOffset),
				typeof(DateTimeOffset),
				dateTimeOffset,
				_transformOpParams,
				parent,
				symbols,
				classScope);
		}

		public CodegenExpression CodegenDateTime(
			CodegenExpression dateTime,
			CodegenMethodScope parent,
			ExprForgeCodegenSymbol symbols,
			CodegenClassScope classScope)
		{
			return CodegenPluginDTM(
				_transformOp.DateTimeOp,
				typeof(DateTime),
				typeof(DateTime),
				dateTime,
				_transformOpParams,
				parent,
				symbols,
				classScope);
		}
	}
} // end of namespace
