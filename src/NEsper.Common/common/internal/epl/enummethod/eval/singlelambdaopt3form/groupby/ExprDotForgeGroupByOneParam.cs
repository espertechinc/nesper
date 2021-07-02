///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.@base;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.groupby
{
	public class ExprDotForgeGroupByOneParam : ExprDotForgeLambdaThreeForm
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
				var key = lambda.BodyForge.EvaluationType;
				
				Type component;
				if (collectionComponentType != null) {
					component = collectionComponentType;
				} else {
					component = inputEventType.UnderlyingType;
				}

				var value = typeof(ICollection<>).MakeGenericType(component);
				var map = typeof(IDictionary<,>).MakeGenericType(key, value);
				return new EPChainableTypeClass(map);
			};
			
			return lambda => EPChainableTypeHelper.SingleValueNonNull(typeof(IDictionary<object, object>));
		}

		protected override ThreeFormEventPlainFactory.ForgeFunction SingleParamEventPlain(EnumMethodEnum enumMethod)
		{
			return (
				lambda,
				typeInfo,
				services) => new EnumGroupByOneParamEvent(lambda);
		}

		protected override ThreeFormEventPlusFactory.ForgeFunction SingleParamEventPlus(EnumMethodEnum enumMethod)
		{
			return (
				lambda,
				indexEventType,
				numParameters,
				typeInfo,
				services) => new EnumGroupByOneParamEventPlus(lambda, indexEventType, numParameters);
		}

		protected override ThreeFormScalarFactory.ForgeFunction SingleParamScalar(EnumMethodEnum enumMethod)
		{
			return (
				lambda,
				eventType,
				numParams,
				typeInfo,
				services) => new EnumGroupByOneParamScalar(lambda, eventType, numParams);
		}
	}
} // end of namespace
