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
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.datetime.plugin
{
	public class DTMPluginUtil
	{
		public static void ValidateDTMStaticMethodAllowNull(
			Type inputType,
			DateTimeMethodMode mode,
			Type firstParameter,
			IList<ExprNode> paramExpressions)
		{
			if (mode == null) {
				if (inputType == firstParameter) {
					throw new ExprValidationException("Plugin datetime method does not provide a forge for input type " + inputType.CleanName());
				}

				return;
			}

			if (!(mode is DateTimeMethodModeStaticMethod)) {
				throw new ExprValidationException("Unexpected plug-in datetime method mode implementation " + mode.GetType());
			}

			var staticMethod = (DateTimeMethodModeStaticMethod) mode;
			var @params = new Type[paramExpressions.Count + 1];
			@params[0] = firstParameter;
			for (var i = 0; i < paramExpressions.Count; i++) {
				@params[i + 1] = paramExpressions[i].Forge.EvaluationType;
			}

			try {
				MethodResolver.ResolveMethod(
					staticMethod.Clazz,
					staticMethod.MethodName,
					@params,
					false,
					new bool[@params.Length],
					new bool[@params.Length]);
			}
			catch (MethodResolverNoSuchMethodException ex) {
				throw new ExprValidationException("Failed to find static method for date-time method extension: " + ex.Message, ex);
			}
		}

		public static CodegenExpression CodegenPluginDTM(
			DateTimeMethodMode mode,
			Type returnedClass,
			Type firstParameterClass,
			CodegenExpression firstParameterExpression,
			IList<ExprNode> paramExpressions,
			CodegenMethodScope parent,
			ExprForgeCodegenSymbol symbols,
			CodegenClassScope classScope)
		{
			var staticMethod = (DateTimeMethodModeStaticMethod) mode;
			var method = parent.MakeChild(returnedClass, typeof(DTMPluginValueChangeForge), classScope).AddParam(firstParameterClass, "dt");
			var @params = new CodegenExpression[paramExpressions.Count + 1];
			@params[0] = Ref("dt");
			for (var i = 0; i < paramExpressions.Count; i++) {
				var forge = paramExpressions[i].Forge;
				@params[i + 1] = forge.EvaluateCodegen(forge.EvaluationType, method, symbols, classScope);
			}

			var callStatic = StaticMethod(staticMethod.Clazz, staticMethod.MethodName, @params);
			if (returnedClass == typeof(void)) {
				method.Block.Expression(callStatic);
			}
			else {
				method.Block.MethodReturn(callStatic);
			}

			return LocalMethod(method, firstParameterExpression);
		}
	}
} // end of namespace
