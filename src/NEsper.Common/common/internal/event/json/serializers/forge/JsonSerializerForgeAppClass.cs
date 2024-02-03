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
    public class JsonSerializerForgeAppClass : JsonSerializerForge
    {
        private readonly string _factoryClassName;
        private readonly string _serializeMethodName;

        public JsonSerializerForgeAppClass(
            string factoryClassName,
            string serializeMethodName)
        {
            _factoryClassName = factoryClassName;
            _serializeMethodName = serializeMethodName;
        }

        public CodegenExpression CodegenSerialize(
            JsonSerializerForgeRefs refs,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            return StaticMethod(
                typeof(JsonSerializerUtil),
                _serializeMethodName,
                refs.Context,
                refs.Field,
                NewInstanceInner(_factoryClassName));
        }
    }
} // end of namespace