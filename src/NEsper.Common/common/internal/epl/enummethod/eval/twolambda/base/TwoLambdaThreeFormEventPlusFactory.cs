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
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.rettype;

namespace com.espertech.esper.common.@internal.epl.enummethodeval.twolambda.@base
{
	public class TwoLambdaThreeFormEventPlusFactory : EnumForgeDescFactory
	{
		private readonly EventType _inputEventType;
		private readonly string _streamNameFirst;
		private readonly string _streamNameSecond;
		private readonly ObjectArrayEventType _typeKey;
		private readonly ObjectArrayEventType _typeValue;
		private readonly int _numParams;
		private readonly ForgeFunction _function;

		public TwoLambdaThreeFormEventPlusFactory(
			EventType inputEventType,
			string streamNameFirst,
			string streamNameSecond,
			ObjectArrayEventType typeKey,
			ObjectArrayEventType typeValue,
			int numParams,
			ForgeFunction function)
		{
			this._inputEventType = inputEventType;
			this._streamNameFirst = streamNameFirst;
			this._streamNameSecond = streamNameSecond;
			this._typeKey = typeKey;
			this._typeValue = typeValue;
			this._numParams = numParams;
			this._function = function;
		}

		public EnumForgeLambdaDesc GetLambdaStreamTypesForParameter(int parameterNum)
		{
			return parameterNum == 0 ? MakeDesc(_typeKey, _streamNameFirst) : MakeDesc(_typeValue, _streamNameSecond);
		}

		public EnumForgeDesc MakeEnumForgeDesc(
			IList<ExprDotEvalParam> bodiesAndParameters,
			int streamCountIncoming,
			StatementCompileTimeServices services)
		{
			var key = (ExprDotEvalParamLambda) bodiesAndParameters[0];
			var value = (ExprDotEvalParamLambda) bodiesAndParameters[1];
			return _function.Invoke(key, value, streamCountIncoming, _typeKey, _typeValue, _numParams, services);
		}

		private EnumForgeLambdaDesc MakeDesc(
			ObjectArrayEventType type,
			string streamName)
		{
			return new EnumForgeLambdaDesc(new EventType[] {_inputEventType, type}, new string[] {streamName, type.Name});
		}

		public delegate EnumForgeDesc ForgeFunction(
			ExprDotEvalParamLambda first,
			ExprDotEvalParamLambda second,
			int streamCountIncoming,
			ObjectArrayEventType firstType,
			ObjectArrayEventType secondType,
			int numParameters,
			StatementCompileTimeServices services);
	}
} // end of namespace
