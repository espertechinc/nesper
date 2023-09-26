///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    public class JsonSerializerForgeByMethod : JsonSerializerForge
    {
        private readonly string methodName;

        public JsonSerializerForgeByMethod(string methodName)
        {
            this.methodName = methodName;
        }

        public CodegenExpression CodegenSerialize(
            JsonSerializerForgeRefs refs,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            if (methodName.Equals("WriteJsonValue") || methodName.Equals("WriteJsonArray")) {
                return StaticMethod(typeof(JsonSerializerUtil), methodName, refs.Context, refs.Name, refs.Field);
            }
            else {
                return StaticMethod(typeof(JsonSerializerUtil), methodName, refs.Context, refs.Field);
            }
        }
    }
} // end of namespace