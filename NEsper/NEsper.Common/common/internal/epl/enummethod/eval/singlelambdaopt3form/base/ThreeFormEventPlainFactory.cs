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
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.@base
{
	public class ThreeFormEventPlainFactory : ThreeFormBaseFactory
	{
		private readonly EventType eventType;
		private readonly string streamName;
		private readonly ForgeFunction function;

		public ThreeFormEventPlainFactory(
			Func<ExprDotEvalParamLambda, EPType> returnType,
			EventType eventType,
			string streamName,
			ForgeFunction function)
			: base(returnType)
		{
			this.eventType = eventType;
			this.streamName = streamName;
			this.function = function;
		}

		public override EnumForgeLambdaDesc GetLambdaStreamTypesForParameter(int parameterNum)
		{
			return new EnumForgeLambdaDesc(new EventType[] {eventType}, new string[] {streamName});
		}

		protected override EnumForge MakeForgeWithParam(
			ExprDotEvalParamLambda lambda,
			EPType typeInfo,
			StatementCompileTimeServices services)
		{
			return function.Invoke(lambda, typeInfo, services);
		}

		public delegate EnumForge ForgeFunction(
			ExprDotEvalParamLambda lambda,
			EPType typeInfo,
			StatementCompileTimeServices services);
	}
} // end of namespace
