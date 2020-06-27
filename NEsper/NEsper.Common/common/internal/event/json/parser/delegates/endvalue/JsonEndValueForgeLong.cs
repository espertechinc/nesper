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
using static com.espertech.esper.common.@internal.@event.json.parser.delegates.endvalue.JsonEndValueForgeUtil; // handleNumberException

namespace com.espertech.esper.common.@internal.@event.json.parser.delegates.endvalue
{
    public class JsonEndValueForgeLong : JsonEndValueForge
    {
        public static readonly JsonEndValueForgeLong INSTANCE = new JsonEndValueForgeLong();

        private JsonEndValueForgeLong()
        {
        }

        public CodegenExpression CaptureValue(
            JsonEndValueRefs refs,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            return StaticMethod(typeof(JsonEndValueForgeLong), "JsonToLong", refs.ValueString, refs.Name);
        }

        public static long? JsonToLong(
            string value,
            string name)
        {
            if (value == null) {
                return null;
            }

            return JsonToLongNonNull(value, name);
        }

        public static long JsonToLongNonNull(
            string stringValue,
            string name)
        {
            try {
                return Int64.Parse(stringValue);
            }
            catch (FormatException ex) {
                throw HandleNumberException(name, typeof(long), stringValue, ex);
            }
        }
    }
} // end of namespace