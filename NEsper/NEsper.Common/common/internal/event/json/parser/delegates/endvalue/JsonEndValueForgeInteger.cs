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
    public class JsonEndValueForgeInteger : JsonEndValueForge
    {
        public static readonly JsonEndValueForgeInteger INSTANCE = new JsonEndValueForgeInteger();

        private JsonEndValueForgeInteger()
        {
        }

        public CodegenExpression CaptureValue(
            JsonEndValueRefs refs,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            return StaticMethod(typeof(JsonEndValueForgeInteger), "JsonToInteger", refs.ValueString, refs.Name);
        }

        public static int? JsonToInteger(
            string value,
            string name)
        {
            if (value == null) {
                return null;
            }

            return JsonToIntegerNonNull(value, name);
        }

        public static int JsonToIntegerNonNull(
            string stringValue,
            string name)
        {
            try {
                return int.Parse(stringValue);
            }
            catch (FormatException ex) {
                throw HandleNumberException(name, typeof(int), stringValue, ex);
            }
        }
    }
} // end of namespace