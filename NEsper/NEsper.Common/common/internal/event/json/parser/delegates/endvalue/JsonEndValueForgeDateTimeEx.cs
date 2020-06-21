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
using com.espertech.esper.compat.datetime;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder; // staticMethod
using static com.espertech.esper.common.@internal.@event.json.parser.delegates.endvalue.JsonEndValueForgeUtil; // handleParseException

namespace com.espertech.esper.common.@internal.@event.json.parser.delegates.endvalue
{
    public class JsonEndValueForgeDateTimeEx : JsonEndValueForge
    {
        public static readonly JsonEndValueForgeDateTimeEx INSTANCE = new JsonEndValueForgeDateTimeEx();

        private JsonEndValueForgeDateTimeEx()
        {
        }

        public CodegenExpression CaptureValue(
            JsonEndValueRefs refs,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            return StaticMethod(typeof(JsonEndValueForgeDateTime), "JsonToDateTimeEx", refs.ValueString, refs.Name);
        }

        public static DateTimeEx JsonToDateTimeEx(
            string value,
            string name)
        {
            if (value == null) {
                return null;
            }

            try {
                return DateTimeParsingFunctions.ParseDefaultEx(value);
            }
            catch (ArgumentException ex) {
                throw HandleParseException(name, typeof(DateTime), value, ex);
            }
        }
    }
} // end of namespace