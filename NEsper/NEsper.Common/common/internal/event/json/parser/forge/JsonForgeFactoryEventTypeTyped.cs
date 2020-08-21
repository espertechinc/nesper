///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.common.@internal.@event.json.parser.deserializers.forge;
using com.espertech.esper.common.@internal.@event.json.serializers;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.json.parser.forge
{
    public class JsonForgeFactoryEventTypeTyped
    {
        public static JsonForgeDesc ForgeNonArray(
            string fieldName,
            JsonEventType other)
        {
            JsonDeserializerForge deserializerForge = null;
            
            JsonSerializerForge serializerForge;
            if (other.Detail.OptionalUnderlyingProvided == null) {
                serializerForge = new JsonSerializerForgeByMethod("WriteNested");
            }
            else {
                serializerForge = new ProxyJsonSerializerForge((
                        refs,
                        method,
                        classScope) =>
                    StaticMethod(
                        typeof(JsonSerializerUtil),
                        "WriteNested",
                        refs.Writer,
                        refs.Field,
                        NewInstanceInner(other.Detail.DeserializerFactoryClassName)));
            }

            return new JsonForgeDesc(fieldName, deserializerForge, serializerForge);
        }

        public static JsonForgeDesc ForgeArray(
            string fieldName,
            JsonEventType other)
        {
            JsonDeserializerForge deserializerForge = null;

            // JsonAllocatorForge startArray = new JsonAllocatorForgeWithAllocatorFactoryArray(
            //     other.Detail.DeserializerFactoryClassName,
            //     other.UnderlyingType);
            // JsonEndValueForge end = new JsonEndValueForgeCast(TypeHelper.GetArrayType(other.UnderlyingType));
            
            JsonSerializerForge serializerForge;
            if (other.Detail.OptionalUnderlyingProvided == null) {
                serializerForge = new JsonSerializerForgeByMethod("WriteNestedArray");
            }
            else {
                serializerForge = new ProxyJsonSerializerForge((
                        refs,
                        method,
                        classScope) =>
                    StaticMethod(
                        typeof(JsonSerializerUtil),
                        "WriteNestedArray",
                        refs.Writer,
                        refs.Field,
                        NewInstanceInner(other.Detail.DeserializerFactoryClassName)));
            }

            return new JsonForgeDesc(fieldName, deserializerForge, serializerForge);
        }
    }
}