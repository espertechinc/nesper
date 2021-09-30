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
using com.espertech.esper.common.@internal.rettype;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.@base
{
	public class ThreeFormEventPlainFactory : ThreeFormBaseFactory
	{
		private readonly EventType _eventType;
		private readonly string _streamName;
		private readonly ForgeFunction _function;

		public ThreeFormEventPlainFactory(
			ThreeFormInitFunction returnType,
			EventType eventType,
			string streamName,
			ForgeFunction function)
			: base(returnType)
		{
			this._eventType = eventType;
			this._streamName = streamName;
			this._function = function;
		}

		public override EnumForgeLambdaDesc GetLambdaStreamTypesForParameter(int parameterNum)
		{
			return new EnumForgeLambdaDesc(new EventType[] {_eventType}, new string[] {_streamName});
		}

		protected override EnumForge MakeForgeWithParam(
			ExprDotEvalParamLambda lambda,
			EPChainableType typeInfo,
			StatementCompileTimeServices services)
		{
			return _function.Invoke(lambda, typeInfo, services);
		}

		public delegate EnumForge ForgeFunction(
			ExprDotEvalParamLambda lambda,
			EPChainableType typeInfo,
			StatementCompileTimeServices services);
	}
} // end of namespace
