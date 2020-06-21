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
    public class JsonEndValueForgeShort : JsonEndValueForge
    {
        public static readonly JsonEndValueForgeShort INSTANCE = new JsonEndValueForgeShort();

        private JsonEndValueForgeShort()
        {
        }

        public CodegenExpression CaptureValue(
            JsonEndValueRefs refs,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            return StaticMethod(typeof(JsonEndValueForgeShort), "JsonToShort", refs.ValueString, refs.Name);
        }

        public static short? JsonToShort(
            string value,
            string name)
        {
            if (value == null) {
                return null;
            }

            return JsonToShortNonNull(value, name);
        }

        public static short JsonToShortNonNull(
            string stringValue,
            string name)
        {
            try {
                return Int16.Parse(stringValue);
            }
            catch (FormatException ex) {
                throw HandleNumberException(name, typeof(short), stringValue, ex);
            }
        }
    }
} // end of namespace