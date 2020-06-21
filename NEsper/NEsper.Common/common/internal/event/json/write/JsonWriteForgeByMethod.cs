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
	public class JsonWriteForgeByMethod : JsonWriteForge {

	    private readonly string methodName;

	    public JsonWriteForgeByMethod(string methodName) {
	        this.methodName = methodName;
	    }

	    public CodegenExpression CodegenWrite(JsonWriteForgeRefs refs, CodegenMethod method, CodegenClassScope classScope) {
	        if (methodName.Equals("writeJsonValue") || methodName.Equals("writeJsonArray")) {
	            return StaticMethod(typeof(JsonWriteUtil), methodName, refs.Writer, refs.Name, refs.Field);
	        } else {
	            return StaticMethod(typeof(JsonWriteUtil), methodName, refs.Writer, refs.Field);
	        }
	    }
	}
} // end of namespace
