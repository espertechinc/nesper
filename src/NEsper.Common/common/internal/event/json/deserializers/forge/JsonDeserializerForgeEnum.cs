///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.json.deserializers.forge
{
    public class JsonDeserializerForgeEnum : JsonDeserializerForge
    {
        private readonly Type _type;

        public JsonDeserializerForgeEnum(Type type)
        {
            _type = type;
        }

        public CodegenExpression CodegenDeserialize(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenExpression elementExpr)
        {
            // Generic types not yet implemented on codegen (TODO)
            return StaticMethod(
                typeof(JsonElementExtensions),
                "GetBoxedEnum",
                new Type[] { _type },
                elementExpr);
        }
    }
} // end of namespace