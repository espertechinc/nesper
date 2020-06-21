///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Numerics;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.@event.json.parser.delegates.endvalue.JsonEndValueForgeUtil;

namespace com.espertech.esper.common.@internal.@event.json.parser.delegates.endvalue
{
    public class JsonEndValueForgeBigInteger : JsonEndValueForge
    {
        public static readonly JsonEndValueForgeBigInteger INSTANCE = new JsonEndValueForgeBigInteger();

        private JsonEndValueForgeBigInteger()
        {
        }

        public CodegenExpression CaptureValue(
            JsonEndValueRefs refs,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            return StaticMethod(typeof(JsonEndValueForgeBigInteger), "JsonToBigInteger", refs.ValueString, refs.Name);
        }

        public static BigInteger? JsonToBigInteger(
            string value,
            string name)
        {
            if (value == null) {
                return null;
            }

            return JsonToBigIntegerNonNull(value, name);
        }

        public static BigInteger JsonToBigIntegerNonNull(
            string stringValue,
            string name)
        {
            try {
                return BigInteger.Parse(stringValue);
            }
            catch (FormatException ex) {
                throw HandleNumberException(name, typeof(BigInteger), stringValue, ex);
            }
        }
    }
} // end of namespace