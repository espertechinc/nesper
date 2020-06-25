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
using com.espertech.esper.common.@internal.epl.enummethod.eval;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.rettype;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.@base
{
	public class ThreeFormScalarFactory : ThreeFormBaseFactory
	{
		private readonly ObjectArrayEventType eventType;
		private readonly int numParams;
		private readonly ForgeFunction function;

		public ThreeFormScalarFactory(
			Func<ExprDotEvalParamLambda, EPType> returnType,
			ObjectArrayEventType eventType,
			int numParams,
			ForgeFunction function)
			: base(returnType)
		{
			this.eventType = eventType;
			this.numParams = numParams;
			this.function = function;
		}

		protected override EnumForge MakeForgeWithParam(
			ExprDotEvalParamLambda lambda,
			EPType typeInfo,
			StatementCompileTimeServices services)
		{
			return function.Invoke(lambda, eventType, numParams, typeInfo, services);
		}

		public override EnumForgeLambdaDesc GetLambdaStreamTypesForParameter(int parameterNum)
		{
			return new EnumForgeLambdaDesc(new EventType[] {eventType}, new string[] {eventType.Name});
		}

		public delegate EnumForge ForgeFunction(
			ExprDotEvalParamLambda lambda,
			ObjectArrayEventType eventType,
			int numParams,
			EPType typeInfo,
			StatementCompileTimeServices services);
	}
} // end of namespace
