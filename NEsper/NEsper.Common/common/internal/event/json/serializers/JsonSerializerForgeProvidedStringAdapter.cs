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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder; // exprDotMethod

// newInstance

namespace com.espertech.esper.common.@internal.@event.json.serializers
{
	public class JsonSerializerForgeProvidedStringAdapter : JsonSerializerForge
	{
		private readonly Type adapterClass;

		public JsonSerializerForgeProvidedStringAdapter(Type adapterClass)
		{
			this.adapterClass = adapterClass;
		}

		public CodegenExpression CodegenSerialize(
			JsonSerializerForgeRefs refs,
			CodegenMethod method,
			CodegenClassScope classScope)
		{
			return ExprDotMethod(NewInstance(adapterClass), "Write", refs.Field, refs.Writer);
		}
	}
} // end of namespace
