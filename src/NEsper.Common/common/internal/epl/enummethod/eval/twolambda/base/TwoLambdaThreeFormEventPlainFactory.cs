///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.enummethod.eval;
using com.espertech.esper.common.@internal.rettype;

namespace com.espertech.esper.common.@internal.epl.enummethodeval.twolambda.@base
{
	public class TwoLambdaThreeFormEventPlainFactory : EnumForgeDescFactory
	{
		private readonly EventType _eventType;
		private readonly string _streamNameFirst;
		private readonly string _streamNameSecond;
		private readonly ForgeFunction _function;

		public TwoLambdaThreeFormEventPlainFactory(
			EventType eventType,
			string streamNameFirst,
			string streamNameSecond,
			ForgeFunction function)
		{
			this._eventType = eventType;
			this._streamNameFirst = streamNameFirst;
			this._streamNameSecond = streamNameSecond;
			this._function = function;
		}

		public EnumForgeLambdaDesc GetLambdaStreamTypesForParameter(int parameterNum)
		{
			return new EnumForgeLambdaDesc(
				new EventType[] {_eventType},
				new string[] {parameterNum == 0 ? _streamNameFirst : _streamNameSecond});
		}

		public EnumForgeDesc MakeEnumForgeDesc(
			IList<ExprDotEvalParam> bodiesAndParameters,
			int streamCountIncoming,
			StatementCompileTimeServices services)
		{
			var first = (ExprDotEvalParamLambda) bodiesAndParameters[0];
			var second = (ExprDotEvalParamLambda) bodiesAndParameters[1];
			return _function.Invoke(first, second, streamCountIncoming, services);
		}

		public delegate EnumForgeDesc ForgeFunction(
			ExprDotEvalParamLambda first,
			ExprDotEvalParamLambda second,
			int streamCountIncoming,
			StatementCompileTimeServices services);
	}
} // end of namespace
