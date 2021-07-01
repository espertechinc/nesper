///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.@base;
using com.espertech.esper.common.@internal.rettype;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.arrayOf
{
	public class ExprDotForgeArrayOf : ExprDotForgeLambdaThreeForm
	{

		protected override EPType InitAndNoParamsReturnType(
			EventType inputEventType,
			Type collectionComponentType)
		{
			return EPTypeHelper.Array(collectionComponentType);
		}

		protected override ThreeFormNoParamFactory.ForgeFunction NoParamsForge(
			EnumMethodEnum enumMethod,
			EPType type,
			StatementCompileTimeServices services)
		{
			return streamCountIncoming => new EnumArrayOfScalarNoParams(ComponentType(type));
		}

		protected override Func<ExprDotEvalParamLambda, EPType> InitAndSingleParamReturnType(
			EventType inputEventType,
			Type collectionComponentType)
		{
			if (inputEventType != null) {
				return lambda => EPTypeHelper.Array(lambda.BodyForge.EvaluationType);
			}

			return lambda => EPTypeHelper.Array(collectionComponentType);
		}

		protected override ThreeFormEventPlainFactory.ForgeFunction SingleParamEventPlain(EnumMethodEnum enumMethod)
		{
			return (
				lambda,
				typeInfo,
				services) => new EnumArrayOfEvent(lambda, ComponentType(typeInfo));
		}

		protected override ThreeFormEventPlusFactory.ForgeFunction SingleParamEventPlus(EnumMethodEnum enumMethod)
		{
			return (
				lambda,
				indexEventType,
				numParameters,
				typeInfo,
				services) => new EnumArrayOfEventPlus(lambda, indexEventType, numParameters, ComponentType(typeInfo));

		}

		protected override ThreeFormScalarFactory.ForgeFunction SingleParamScalar(EnumMethodEnum enumMethod)
		{
			return (
				lambda,
				fieldType,
				numParams,
				typeInfo,
				services) => new EnumArrayOfScalar(lambda, fieldType, numParams, ComponentType(typeInfo));
		}

		private Type ComponentType(EPType type)
		{
			ClassMultiValuedEPType mv = (ClassMultiValuedEPType) type;
			return mv.Component;
		}
	}
} // end of namespace
