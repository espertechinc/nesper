///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.json.compiletime;
using com.espertech.esper.common.@internal.@event.json.parser.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;


using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.json.writer
{
	public class JsonEventBeanPropertyWriterMapProp : JsonEventBeanPropertyWriter {

	    private readonly string key;

	    public JsonEventBeanPropertyWriterMapProp(JsonDelegateFactory delegateFactory, JsonUnderlyingField field, string key) : base(delegateFactory, field) {
	        this.key = key;
	    }

	    public override void Write(object value, object und) {
	        JsonWriteMapProp(value, delegateFactory.GetValue(field.PropertyNumber, und), key);
	    }

	    public override CodegenExpression WriteCodegen(CodegenExpression assigned, CodegenExpression und, CodegenExpression target, CodegenMethodScope parent, CodegenClassScope classScope) {
	        return StaticMethod(typeof(JsonEventBeanPropertyWriterMapProp), "jsonWriteMapProp", assigned, ExprDotName(und, field.FieldName), Constant(key));
	    }

	    /// <summary>
	    /// NOTE: Code-generation-invoked method, method name and parameter order matters
	    /// </summary>
	    /// <param name="value">value</param>
	    /// <param name="mapEntry">map entry</param>
	    /// <param name="key">key</param>
	    public static void JsonWriteMapProp(object value, object mapEntry, string key) {
		    if (mapEntry is IDictionary<string, object> map) {
			    map.Put(key, value);
		    }
	    }
	}
} // end of namespace
