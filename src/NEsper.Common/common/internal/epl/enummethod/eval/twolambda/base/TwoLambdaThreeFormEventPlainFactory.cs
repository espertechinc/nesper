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
		private readonly EventType eventType;
		private readonly string streamNameFirst;
		private readonly string streamNameSecond;
		private readonly EPType returnType;
		private readonly TwoLambdaThreeFormEventPlainFactory.ForgeFunction function;

		public TwoLambdaThreeFormEventPlainFactory(
			EventType eventType,
			string streamNameFirst,
			string streamNameSecond,
			EPType returnType,
			ForgeFunction function)
		{
			this.eventType = eventType;
			this.streamNameFirst = streamNameFirst;
			this.streamNameSecond = streamNameSecond;
			this.returnType = returnType;
			this.function = function;
		}

		public EnumForgeLambdaDesc GetLambdaStreamTypesForParameter(int parameterNum)
		{
			return new EnumForgeLambdaDesc(
				new EventType[] {eventType},
				new string[] {parameterNum == 0 ? streamNameFirst : streamNameSecond});
		}

		public EnumForgeDesc MakeEnumForgeDesc(
			IList<ExprDotEvalParam> bodiesAndParameters,
			int streamCountIncoming,
			StatementCompileTimeServices services)
		{
			ExprDotEvalParamLambda first = (ExprDotEvalParamLambda) bodiesAndParameters[0];
			ExprDotEvalParamLambda second = (ExprDotEvalParamLambda) bodiesAndParameters[1];
			EnumForge forge = function.Invoke(first, second, streamCountIncoming, returnType, services);
			return new EnumForgeDesc(returnType, forge);
		}

		public delegate EnumForge ForgeFunction(
			ExprDotEvalParamLambda first,
			ExprDotEvalParamLambda second,
			int streamCountIncoming,
			EPType typeInfo,
			StatementCompileTimeServices services);
	}
} // end of namespace
