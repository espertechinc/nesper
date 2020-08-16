///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.json.serializers
{
	public class JsonSerializerForgeString : JsonSerializerForge
	{

		public static readonly JsonSerializerForgeString INSTANCE = new JsonSerializerForgeString();

		private JsonSerializerForgeString()
		{
		}

		public CodegenExpression CodegenSerialize(
			JsonSerializerForgeRefs refs,
			CodegenMethod method,
			CodegenClassScope classScope)
		{
			return StaticMethod(typeof(JsonSerializerUtil), "WriteNullableString", refs.Writer, refs.Field);
		}
	}
} // end of namespace
