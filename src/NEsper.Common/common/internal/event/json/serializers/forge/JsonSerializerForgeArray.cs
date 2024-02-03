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
using com.espertech.esper.common.@internal.@event.json.serde;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.json.serializers.forge
{
    public class JsonSerializerForgeArray : JsonSerializerForge
    {
        private readonly JsonSerializerForge _subForge;
        private readonly Type _componentType;

        public JsonSerializerForgeArray(
            JsonSerializerForge subForge,
            Type componentType)
        {
            _subForge = subForge;
            _componentType = componentType;
        }

        public CodegenExpression CodegenSerialize(
            JsonSerializerForgeRefs refs,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            var subRefs = new JsonSerializerForgeRefs(
                Ref("_context"),
                Ref("_arrayItem"),
                Ref("_name"));

            var serializationExpr = _subForge.CodegenSerialize(subRefs, method, classScope);
            var itemSerializer = new CodegenExpressionLambda(method.Block)
                .WithParam<JsonSerializationContext>("_context")
                .WithParam(_componentType, "_arrayItem")
                .WithBody(block => block.Expression(serializationExpr));

            return StaticMethod(
                typeof(JsonSerializerUtil),
                "WriteNestedArray",
                refs.Context,
                refs.Field,
                itemSerializer);
        }
    }
} // end of namespace