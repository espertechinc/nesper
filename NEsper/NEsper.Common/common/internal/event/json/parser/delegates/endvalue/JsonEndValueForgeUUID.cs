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
    public class JsonEndValueForgeUUID : JsonEndValueForge
    {
        public static readonly JsonEndValueForgeUUID INSTANCE = new JsonEndValueForgeUUID();

        private JsonEndValueForgeUUID()
        {
        }

        public CodegenExpression CaptureValue(
            JsonEndValueRefs refs,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            return StaticMethod(typeof(JsonEndValueForgeUUID), "JsonToUUID", refs.ValueString, refs.Name);
        }

        public static Guid? JsonToUuid(
            string value,
            string name)
        {
            if (value == null) {
                return null;
            }

            return JsonToUuidNonNull(value, name);
        }

        public static Guid JsonToUuidNonNull(
            string stringValue,
            string name)
        {
            try {
                return Guid.Parse(stringValue);
            }
            catch (FormatException ex) {
                throw HandleParseException(name, typeof(Guid), stringValue, ex);
            }
        }
    }
} // end of namespace