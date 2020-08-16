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
using com.espertech.esper.common.@internal.@event.json.parser.forge;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.json.parser.deserializers.forge
{
	public class JsonDeserializerForgeByClass : JsonDeserializerForge
	{
		private readonly Type _clazz;
		private readonly CodegenExpression[] _parameters;

		public JsonDeserializerForgeByClass(Type clazz)
		{
			_clazz = clazz;
			_parameters = new CodegenExpression[0];
		}

		public JsonDeserializerForgeByClass(
			Type clazz,
			params CodegenExpression[] @params)
		{
			_clazz = clazz;
			_parameters = @params;
		}

		public CodegenExpression CodegenDeserialize(
			JsonDeserializeRefs refs,
			CodegenMethod method,
			CodegenClassScope classScope)
		{
			throw new NotImplementedException("broken");
		}
	}
} // end of namespace
