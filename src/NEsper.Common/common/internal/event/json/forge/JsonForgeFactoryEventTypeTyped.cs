///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.common.@internal.@event.json.deserializers.forge;
using com.espertech.esper.common.@internal.@event.json.serializers;
using com.espertech.esper.common.@internal.@event.json.serializers.forge;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.json.forge
{
    public class JsonForgeFactoryEventTypeTyped
    {
        public static JsonForgeDesc ForgeNonArray(
            string fieldName,
            JsonEventType other)
        {
            JsonDeserializerForge deserializerForge = new JsonDeserializerForgeByClassName(
                other.Detail.DeserializerClassName);
            
            JsonSerializerForge serializerForge;
            if (other.Detail.OptionalUnderlyingProvided == null) {
                serializerForge = new JsonSerializerForgeByMethod("WriteNested");
            }
            else {
                serializerForge = new ProxyJsonSerializerForge(
                    (
                            refs,
                            method,
                            classScope) =>
                        StaticMethod(
                            typeof(JsonSerializerUtil),
                            "WriteNested",
                            refs.Context,
                            refs.Field,
                            NewInstanceNamed(other.Detail.SerializerClassName)
                            ));
                // NewInstanceInner(other.Detail.DeserializerFactoryClassName)));
            }

            return new JsonForgeDesc(deserializerForge, serializerForge);
        }

        public static JsonForgeDesc ForgeArray(
            string fieldName,
            JsonEventType other)
        {
            var deserializerForge = new JsonDeserializerForgeArray(
                new JsonDeserializerForgeByClassName(other.Detail.DeserializerClassName),
                other.UnderlyingType);

            // JsonAllocatorForge startArray = new JsonAllocatorForgeWithAllocatorFactoryArray(
            //     other.Detail.DeserializerFactoryClassName,
            //     other.UnderlyingType);
            // JsonEndValueForge end = new JsonEndValueForgeCast(TypeHelper.GetArrayType(other.UnderlyingType));
            
            JsonSerializerForge serializerForge;
            if (other.Detail.OptionalUnderlyingProvided == null) {
                serializerForge = new JsonSerializerForgeByMethod("WriteNestedArray");
            }
            else {
                serializerForge = new ProxyJsonSerializerForge(
                    (
                            refs,
                            method,
                            classScope) =>
                        StaticMethod(
                            typeof(JsonSerializerUtil),
                            "WriteNestedArray",
                            refs.Context,
                            refs.Field,
                            NewInstanceNamed(other.Detail.SerializerClassName)
                            ));
            }

            return new JsonForgeDesc(deserializerForge, serializerForge);
        }
    }
}