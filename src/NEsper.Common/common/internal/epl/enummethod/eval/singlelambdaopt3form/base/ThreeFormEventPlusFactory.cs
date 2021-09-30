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
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.rettype;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.@base
{
	public class ThreeFormEventPlusFactory : ThreeFormBaseFactory
	{
		private readonly EventType _eventType;
		private readonly string _streamName;
		private readonly ObjectArrayEventType _fieldType;
		private readonly int _numParameters;
		private readonly ForgeFunction _function;

		public ThreeFormEventPlusFactory(
			ThreeFormInitFunction returnType,
			EventType eventType,
			string streamName,
			ObjectArrayEventType fieldType,
			int numParameters,
			ForgeFunction function)
			: base(returnType)
		{
			this._eventType = eventType;
			this._streamName = streamName;
			this._fieldType = fieldType;
			this._numParameters = numParameters;
			this._function = function;
		}

		public override EnumForgeLambdaDesc GetLambdaStreamTypesForParameter(int parameterNum)
		{
			return new EnumForgeLambdaDesc(new EventType[] {_eventType, _fieldType}, new string[] {_streamName, _fieldType.Name});
		}

		protected override EnumForge MakeForgeWithParam(
			ExprDotEvalParamLambda lambda,
			EPChainableType typeInfo,
			StatementCompileTimeServices services)
		{
			return _function.Invoke(lambda, _fieldType, _numParameters, typeInfo, services);
		}

		public delegate EnumForge ForgeFunction(
			ExprDotEvalParamLambda lambda,
			ObjectArrayEventType fieldType,
			int numParameters,
			EPChainableType typeInfo,
			StatementCompileTimeServices services);
	}
} // end of namespace
