///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text.Json;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.json.deserializers.forge
{
    public class JsonDeserializerForgeByClass : JsonDeserializerForge
    {
        private readonly Type _clazz;
        private readonly CodegenExpression[] _parameters;

        public JsonDeserializerForgeByClass(Type clazz)
        {
            _clazz = clazz;
            _parameters = Array.Empty<CodegenExpression>();
        }

        public JsonDeserializerForgeByClass(
            Type clazz,
            params CodegenExpression[] @params)
        {
            _clazz = clazz;
            _parameters = @params;
        }

        public CodegenExpression CodegenDeserialize(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenExpression elementExpr)
        {
            var child = method
                .MakeChild(typeof(object), GetType(), classScope)
                .AddParam<JsonElement>("jsonElement");

            child
                .Block
                .DeclareVar<IJsonDeserializer>("deserializer", NewInstance(_clazz, _parameters))
                .MethodReturn(
                    ExprDotMethod(
                        Ref("deserializer"),
                        "Deserialize",
                        Ref("jsonElement")));

            return LocalMethod(child, elementExpr);
        }
    }
} // end of namespace