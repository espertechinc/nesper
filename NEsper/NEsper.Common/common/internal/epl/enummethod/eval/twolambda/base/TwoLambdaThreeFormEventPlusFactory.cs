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
using com.espertech.esper.common.@internal.epl.enummethod.eval;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.rettype;

namespace com.espertech.esper.common.@internal.epl.enummethodeval.twolambda.@base
{
	public class TwoLambdaThreeFormEventPlusFactory : EnumForgeDescFactory
	{
		private readonly EventType inputEventType;
		private readonly string streamNameFirst;
		private readonly string streamNameSecond;
		private readonly ObjectArrayEventType typeKey;
		private readonly ObjectArrayEventType typeValue;
		private readonly int numParams;
		private readonly EPType returnType;
		private readonly TwoLambdaThreeFormEventPlusFactory.ForgeFunction function;

		public TwoLambdaThreeFormEventPlusFactory(
			EventType inputEventType,
			string streamNameFirst,
			string streamNameSecond,
			ObjectArrayEventType typeKey,
			ObjectArrayEventType typeValue,
			int numParams,
			EPType returnType,
			ForgeFunction function)
		{
			this.inputEventType = inputEventType;
			this.streamNameFirst = streamNameFirst;
			this.streamNameSecond = streamNameSecond;
			this.typeKey = typeKey;
			this.typeValue = typeValue;
			this.numParams = numParams;
			this.returnType = returnType;
			this.function = function;
		}

		public EnumForgeLambdaDesc GetLambdaStreamTypesForParameter(int parameterNum)
		{
			return parameterNum == 0 ? MakeDesc(typeKey, streamNameFirst) : MakeDesc(typeValue, streamNameSecond);
		}

		public EnumForgeDesc MakeEnumForgeDesc(
			IList<ExprDotEvalParam> bodiesAndParameters,
			int streamCountIncoming,
			StatementCompileTimeServices services)
		{
			ExprDotEvalParamLambda key = (ExprDotEvalParamLambda) bodiesAndParameters[0];
			ExprDotEvalParamLambda value = (ExprDotEvalParamLambda) bodiesAndParameters[1];
			EnumForge forge = function.Invoke(key, value, streamCountIncoming, typeKey, typeValue, numParams, returnType, services);
			return new EnumForgeDesc(returnType, forge);
		}

		private EnumForgeLambdaDesc MakeDesc(
			ObjectArrayEventType type,
			string streamName)
		{
			return new EnumForgeLambdaDesc(new EventType[] {inputEventType, type}, new string[] {streamName, type.Name});
		}

		public delegate EnumForge ForgeFunction(
			ExprDotEvalParamLambda first,
			ExprDotEvalParamLambda second,
			int streamCountIncoming,
			ObjectArrayEventType firstType,
			ObjectArrayEventType secondType,
			int numParameters,
			EPType typeInfo,
			StatementCompileTimeServices services);
	}
} // end of namespace
