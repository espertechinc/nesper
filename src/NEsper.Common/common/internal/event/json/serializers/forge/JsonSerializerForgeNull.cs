///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder; // constant

// exprDotMethod

namespace com.espertech.esper.common.@internal.@event.json.serializers.forge
{
	public class JsonSerializerForgeNull : JsonSerializerForge
	{
		public static readonly JsonSerializerForgeNull INSTANCE = new JsonSerializerForgeNull();

		private JsonSerializerForgeNull()
		{
		}

		public CodegenExpression CodegenSerialize(
			JsonSerializerForgeRefs refs,
			CodegenMethod method,
			CodegenClassScope classScope)
		{
			return ExprDotMethod(refs.Context, "WriteNullValue");
		}
	}
} // end of namespace
