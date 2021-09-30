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
using com.espertech.esper.common.@internal.rettype;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.@base
{
	public class ThreeFormNoParamFactory : EnumForgeDescFactory
	{
		private readonly EPChainableType _returnType;
		private readonly ForgeFunction _function;

		public ThreeFormNoParamFactory(
			EPChainableType returnType,
			ForgeFunction function)
		{
			_returnType = returnType;
			_function = function;
		}

		public EnumForgeLambdaDesc GetLambdaStreamTypesForParameter(int parameterNum)
		{
			return new EnumForgeLambdaDesc(new EventType[0], new string[0]);
		}

		public EnumForgeDesc MakeEnumForgeDesc(
			IList<ExprDotEvalParam> bodiesAndParameters,
			int streamCountIncoming,
			StatementCompileTimeServices statementCompileTimeService)
		{
			return new EnumForgeDesc(_returnType, _function.Invoke(streamCountIncoming));
		}

		public delegate EnumForge ForgeFunction(int streamCountIncoming);
	}
} // end of namespace
