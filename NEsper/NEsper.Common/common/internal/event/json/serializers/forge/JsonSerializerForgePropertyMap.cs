///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.json.serde;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.json.serializers.forge
{
	public class JsonSerializerForgePropertyMap : JsonSerializerForge
	{
		private readonly JsonSerializerForge _valueForge;
		private readonly Type _valueType;
		
		public JsonSerializerForgePropertyMap(JsonSerializerForge valueForge, Type valueType)
		{
			_valueForge = valueForge;
			_valueType = valueType;
		}

		public CodegenExpression CodegenSerialize(
			JsonSerializerForgeRefs refs,
			CodegenMethod method,
			CodegenClassScope classScope)
		{
			var subRefs = new JsonSerializerForgeRefs(
				Ref("_context"),
				Ref("_value"),
				Ref("_name"));
			
			var serializationExpr = _valueForge.CodegenSerialize(subRefs, method, classScope);
			var itemSerializer = new CodegenExpressionLambda(method.Block)
				.WithParam(typeof(JsonSerializationContext), "_context")
				.WithParam(_valueType, "_value")
				.WithBody(block => block.Expression(serializationExpr));

			return StaticMethod(
				typeof(JsonSerializerUtil),
				"WriteJsonMap",
				new [] { _valueType },
				refs.Context,
				refs.Field,
				itemSerializer);
		}
	}
} // end of namespace
