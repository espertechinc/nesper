///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using static
    com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder; // staticMethod

// handleNumberException

namespace com.espertech.esper.common.@internal.@event.json.deserializers.forge
{
    public class JsonDeserializerForgeInt64 : JsonDeserializerForge
    {
        public static readonly JsonDeserializerForgeInt64 INSTANCE = new JsonDeserializerForgeInt64();

        private JsonDeserializerForgeInt64()
        {
        }

        public CodegenExpression CodegenDeserialize(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenExpression elementExpr)
        {
            return StaticMethod(typeof(JsonElementExtensions), "GetBoxedInt64", elementExpr);
        }
    }
} // end of namespace