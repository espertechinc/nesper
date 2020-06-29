///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.countof
{
	public class EnumCountOfNoParams : EnumForge,
		EnumEval
	{
		private readonly int _numStreams;

		public EnumCountOfNoParams(int numStreams)
		{
			this._numStreams = numStreams;
		}

		public EnumEval EnumEvaluator => this;

		public int StreamNumSize => _numStreams;

		public object EvaluateEnumMethod(
			EventBean[] eventsLambda,
			ICollection<object> enumcoll,
			bool isNewData,
			ExprEvaluatorContext context)
		{
			return enumcoll.Count;
		}

		public CodegenExpression Codegen(
			EnumForgeCodegenParams premade,
			CodegenMethodScope codegenMethodScope,
			CodegenClassScope codegenClassScope)
		{
			return ExprDotName(premade.Enumcoll, "Count");
		}
	}
} // end of namespace
