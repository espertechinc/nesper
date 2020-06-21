///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder; // staticMethod

// handleNumberException

namespace com.espertech.esper.common.@internal.@event.json.deserializers.forge
{
	public class JsonDeserializerForgeDouble : JsonDeserializerForge
	{
		public static readonly JsonDeserializerForgeDouble INSTANCE = new JsonDeserializerForgeDouble();

		private JsonDeserializerForgeDouble()
		{
		}
		
		public CodegenExpression CodegenDeserialize(
			CodegenMethod method,
			CodegenClassScope classScope,
			CodegenExpression elementExpr)
		{
			return ExprDotMethod(elementExpr, "GetBoxedDouble");
		}
	}
} // end of namespace
