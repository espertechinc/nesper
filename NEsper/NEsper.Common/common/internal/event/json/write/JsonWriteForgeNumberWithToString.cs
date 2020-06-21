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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;


using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.json.write
{
	public class JsonWriteForgeNumberWithToString : JsonWriteForge {

	    public readonly static JsonWriteForgeNumberWithToString INSTANCE = new JsonWriteForgeNumberWithToString();

	    private JsonWriteForgeNumberWithToString() {
	    }

	    public CodegenExpression CodegenWrite(JsonWriteForgeRefs refs, CodegenMethod method, CodegenClassScope classScope) {
	        return StaticMethod(typeof(JsonWriteUtil), "writeNullableNumber", refs.Writer, refs.Field);
	    }
	}
} // end of namespace
