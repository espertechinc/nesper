///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text.Json;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.json.deserializers.forge
{
    public class JsonDeserializerForgePropertyMap : JsonDeserializerForge
    {
        private readonly JsonDeserializerForge _valueForge;
        private readonly Type _valueType;

        public JsonDeserializerForgePropertyMap(
            JsonDeserializerForge valueForge,
            Type valueType)
        {
            _valueForge = valueForge;
            _valueType = valueType;
        }

        public CodegenExpression CodegenDeserialize(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenExpression elementExpr)
        {
            var mapType = typeof(IDictionary<,>).MakeGenericType(typeof(string), _valueType);
            var child = method
                .MakeChild(mapType, GetType(), classScope)
                .AddParam<JsonElement>("jsonElement");

            child
                .Block
                .MethodReturn(
                    StaticMethod(
                        typeof(JsonElementExtensions),
                        "ElementToDictionary",
                        new[] { _valueType },
                        Ref("jsonElement"),
                        new CodegenExpressionLambda(method.Block)
                            .WithParam<JsonElement>("_")
                            .WithBody(
                                _ => _
                                    .BlockReturn(
                                        Cast(
                                            _valueType,
                                            _valueForge.CodegenDeserialize(
                                                child,
                                                classScope,
                                                Ref("_")))))));

            return LocalMethod(child, elementExpr);
        }
    }
} // end of namespace