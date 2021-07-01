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
	public class TwoLambdaThreeFormScalarFactory : EnumForgeDescFactory
	{
		private readonly ObjectArrayEventType _typeFirst;
		private readonly ObjectArrayEventType _typeSecond;
		private readonly int _numParams;
		private readonly EPType _returnType;
		private readonly TwoLambdaThreeFormScalarFactory.ForgeFunction _function;

		public TwoLambdaThreeFormScalarFactory(
			ObjectArrayEventType typeFirst,
			ObjectArrayEventType typeSecond,
			int numParams,
			EPType returnType,
			TwoLambdaThreeFormScalarFactory.ForgeFunction function)
		{
			this._typeFirst = typeFirst;
			this._typeSecond = typeSecond;
			this._numParams = numParams;
			this._returnType = returnType;
			this._function = function;
		}

		public EnumForgeLambdaDesc GetLambdaStreamTypesForParameter(int parameterNum)
		{
			return parameterNum == 0 ? MakeDesc(_typeFirst) : MakeDesc(_typeSecond);
		}

		public EnumForgeDesc MakeEnumForgeDesc(
			IList<ExprDotEvalParam> bodiesAndParameters,
			int streamCountIncoming,
			StatementCompileTimeServices services)
		{
			ExprDotEvalParamLambda first = (ExprDotEvalParamLambda) bodiesAndParameters[0];
			ExprDotEvalParamLambda second = (ExprDotEvalParamLambda) bodiesAndParameters[1];
			EnumForge forge = _function.Invoke(first, second, _typeFirst, _typeSecond, streamCountIncoming, _numParams, _returnType);
			return new EnumForgeDesc(_returnType, forge);
		}

		private static EnumForgeLambdaDesc MakeDesc(ObjectArrayEventType type)
		{
			return new EnumForgeLambdaDesc(new EventType[] {type}, new string[] {type.Name});
		}

		public delegate EnumForge ForgeFunction(
			ExprDotEvalParamLambda first,
			ExprDotEvalParamLambda second,
			ObjectArrayEventType eventTypeFirst,
			ObjectArrayEventType eventTypeSecond,
			int streamCountIncoming,
			int numParams,
			EPType typeInfo);
	}
} // end of namespace
