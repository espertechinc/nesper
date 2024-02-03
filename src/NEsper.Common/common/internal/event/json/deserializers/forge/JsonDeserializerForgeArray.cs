///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text.Json;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.json.deserializers.forge
{
    public class JsonDeserializerForgeArray : JsonDeserializerForge
    {
        private readonly JsonDeserializerForge _subForge;
        private readonly Type _subForgeComponentType;

        public JsonDeserializerForgeArray(
            JsonDeserializerForge subForge,
            Type subForgeComponentType)
        {
            _subForge = subForge;
            _subForgeComponentType = subForgeComponentType;
        }

        public CodegenExpression CodegenDeserialize(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenExpression elementExpr)
        {
            var arrayType = TypeHelper.GetArrayType(_subForgeComponentType);
            var child = method
                .MakeChild(arrayType, GetType(), classScope)
                .AddParam<JsonElement>("jsonElement");

            child
                .Block
                .MethodReturn(
                    StaticMethod(
                        typeof(JsonElementExtensions),
                        "ElementToArray",
                        new[] { _subForgeComponentType },
                        Ref("jsonElement"),
                        new CodegenExpressionLambda(method.Block)
                            .WithParam<JsonElement>("_")
                            .WithBody(
                                _ => _.BlockReturn(
                                    Cast(
                                        _subForgeComponentType,
                                        _subForge.CodegenDeserialize(
                                            child,
                                            classScope,
                                            Ref("_")))))));

            return LocalMethod(child, elementExpr);

#if false
			method
				.Block
				.StaticMethod(
					typeof(JsonElementExtensions),
					"ElementToArray",
					Ref("jsonElement"),
					new CodegenExpressionLambda(method.Block)
						.WithParam<JsonElement>("_")
						.WithBody(_ => _subForge.CodegenDeserialize(method, classScope, Ref("_"))));
#endif
        }
    }
} // end of namespace