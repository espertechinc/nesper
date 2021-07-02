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
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.minmaxby
{
	public class ExprDotForgeMinByMaxBy : ExprDotForgeLambdaThreeForm
	{

		protected override EPChainableType InitAndNoParamsReturnType(
			EventType inputEventType,
			Type collectionComponentType)
		{
			throw new IllegalStateException();
		}

		protected override ThreeFormNoParamFactory.ForgeFunction NoParamsForge(
			EnumMethodEnum enumMethod,
			EPChainableType type,
			StatementCompileTimeServices services)
		{
			throw new IllegalStateException();
		}

		protected override ThreeFormInitFunction InitAndSingleParamReturnType(
			EventType inputEventType,
			Type collectionComponentType)
		{
			return lambda => {
				ValidateNonNull(lambda.BodyForge.EvaluationType);
				return EPChainableTypeHelper.SingleEvent(inputEventType);
			};
		}

		protected override ThreeFormEventPlainFactory.ForgeFunction SingleParamEventPlain(EnumMethodEnum enumMethod)
		{
			return (
				lambda,
				typeInfo,
				services) => new EnumMinMaxByEvents(lambda, enumMethod == EnumMethodEnum.MAXBY);
		}

		protected override ThreeFormEventPlusFactory.ForgeFunction SingleParamEventPlus(EnumMethodEnum enumMethod)
		{
			return (
				lambda,
				fieldType,
				numParameters,
				typeInfo,
				services) => new EnumMinMaxByEventsPlus(lambda, fieldType, numParameters, enumMethod == EnumMethodEnum.MAXBY);
		}

		protected override ThreeFormScalarFactory.ForgeFunction SingleParamScalar(EnumMethodEnum enumMethod)
		{
			return (
				lambda,
				eventType,
				numParams,
				typeInfo,
				services) => new EnumMinMaxByScalar(lambda, eventType, numParams, enumMethod == EnumMethodEnum.MAXBY, typeInfo);
		}
	}
} // end of namespace
