///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder; // staticMethod

namespace com.espertech.esper.common.@internal.@event.json.write
{
    public class JsonWriteForgeBoolean : JsonWriteForge
    {
        public static readonly JsonWriteForgeBoolean INSTANCE = new JsonWriteForgeBoolean();

        private JsonWriteForgeBoolean()
        {
        }

        public CodegenExpression CodegenWrite(
            JsonWriteForgeRefs refs,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            return StaticMethod(typeof(JsonWriteUtil), "WriteNullableBoolean", refs.Writer, refs.Field);
        }
    }
} // end of namespace