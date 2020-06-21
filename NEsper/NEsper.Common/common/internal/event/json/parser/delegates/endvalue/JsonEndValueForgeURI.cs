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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder; // staticMethod
using static com.espertech.esper.common.@internal.@event.json.parser.delegates.endvalue.JsonEndValueForgeUtil; // handleParseException

namespace com.espertech.esper.common.@internal.@event.json.parser.delegates.endvalue
{
    public class JsonEndValueForgeURI : JsonEndValueForge
    {
        public static readonly JsonEndValueForgeURI INSTANCE = new JsonEndValueForgeURI();

        private JsonEndValueForgeURI()
        {
        }

        public CodegenExpression CaptureValue(
            JsonEndValueRefs refs,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            return StaticMethod(typeof(JsonEndValueForgeURI), "JsonToURI", refs.ValueString, refs.Name);
        }

        public static Uri JsonToUri(
            string value,
            string name)
        {
            return value == null ? null : JsonToUriNonNull(value, name);
        }

        public static Uri JsonToUriNonNull(
            string stringValue,
            string name)
        {
            try {
                return new Uri(stringValue);
            }
            catch (UriFormatException ex) {
                throw HandleParseException(name, typeof(Uri), stringValue, ex);
            }
        }
    }
} // end of namespace