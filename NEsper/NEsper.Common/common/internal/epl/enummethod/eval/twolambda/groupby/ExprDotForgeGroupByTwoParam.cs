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
using com.espertech.esper.common.@internal.epl.enummethodeval.twolambda.@base;
using com.espertech.esper.common.@internal.rettype;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.twolambda.groupby
{
	public class ExprDotForgeGroupByTwoParam : ExprDotForgeTwoLambda
	{

		protected override EPType ReturnType(
			EventType inputEventType,
			Type collectionComponentType)
		{
			return EPTypeHelper.SingleValue(typeof(IDictionary<object, object>));
		}

		protected override TwoLambdaThreeFormEventPlainFactory.ForgeFunction TwoParamEventPlain()
		{
			return (
				first,
				second,
				streamCountIncoming,
				typeInfo,
				services) => new EnumGroupByTwoParamEventPlain(first.BodyForge, streamCountIncoming, second.BodyForge);
		}

		protected override TwoLambdaThreeFormEventPlusFactory.ForgeFunction TwoParamEventPlus()
		{
			return (
				first,
				second,
				streamCountIncoming,
				firstType,
				secondType,
				numParameters,
				typeInfo,
				services) => new EnumGroupByTwoParamEventPlus(
				first.BodyForge,
				streamCountIncoming,
				firstType,
				second.BodyForge,
				numParameters);
		}

		protected override TwoLambdaThreeFormScalarFactory.ForgeFunction TwoParamScalar()
		{
			return (
					first,
					second,
					eventTypeFirst,
					eventTypeSecond,
					streamCountIncoming,
					numParams,
					typeInfo) =>
				new EnumGroupByTwoParamScalar(first.BodyForge, streamCountIncoming, second.BodyForge, eventTypeFirst, numParams);
		}
	}
} // end of namespace
