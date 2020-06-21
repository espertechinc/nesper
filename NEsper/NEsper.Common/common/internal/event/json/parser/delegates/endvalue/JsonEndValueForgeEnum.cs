///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.json.parser.delegates.endvalue
{
    public class JsonEndValueForgeEnum : JsonEndValueForge
    {
        private readonly Type _type;

        public JsonEndValueForgeEnum(Type type)
        {
            this._type = type;
        }

        public CodegenExpression CaptureValue(
            JsonEndValueRefs refs,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            return Conditional(
                EqualsNull(refs.ValueString),
                ConstantNull(),
                StaticMethod(typeof(EnumHelper), "Parse", new[] {_type}, refs.ValueString));
        }

        public static object JsonToEnum(
            string stringValue,
            Type enumType)
        {
            if (stringValue == null) {
                return null;
            }

            try {
                return Enum.Parse(enumType, stringValue, true);
            }
            catch (Exception ex) {
                throw new EPException("Failed to invoke enum-type valueOf method: " + ex.Message, ex);
            }
        }
    }
} // end of namespace