///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.json.serializers.forge
{
    public class JsonSerializerForgeList : JsonSerializerForge
    {
        private readonly JsonSerializerForge _subForge;

        public JsonSerializerForgeList(JsonSerializerForge subForge)
        {
            _subForge = subForge;
        }

        public CodegenExpression CodegenSerialize(
            JsonSerializerForgeRefs refs,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            // We need to encode the _subForge into a lambda of its own
            var itemSerializer = new CodegenExpressionLambda(method.Block)
                .WithBody(block => _subForge.CodegenSerialize(refs, method, classScope));

            return StaticMethod(
                typeof(JsonSerializerUtil),
                "WriteJsonList",
                refs.Context,
                refs.Field,
                itemSerializer);
        }
    }
} // end of namespace