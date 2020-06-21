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


using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder; // exprDotMethod
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder; // newInstance

namespace com.espertech.esper.common.@internal.@event.json.parser.delegates.endvalue
{
	public class JsonEndValueForgeProvidedStringAdapter : JsonEndValueForge {
	    private readonly Type _adapterClass;

	    public JsonEndValueForgeProvidedStringAdapter(Type adapterClass) {
	        this._adapterClass = adapterClass;
	    }

	    public CodegenExpression CaptureValue(JsonEndValueRefs refs, CodegenMethod method, CodegenClassScope classScope) {
	        return ExprDotMethod(NewInstance(_adapterClass), "parse", refs.ValueString);
	    }
	}
} // end of namespace
