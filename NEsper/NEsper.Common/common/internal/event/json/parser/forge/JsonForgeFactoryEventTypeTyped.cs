///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.common.@internal.@event.json.parser.delegates.endvalue;
using com.espertech.esper.common.@internal.@event.json.write;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.json.parser.forge
{

    public class JsonForgeFactoryEventTypeTyped
    {
        public static JsonForgeDesc ForgeNonArray(
            String fieldName,
            JsonEventType other)
        {
            JsonDelegateForge startObject = new JsonDelegateForgeWithDelegateFactory(other.Detail.DelegateFactoryClassName);
            JsonEndValueForge end = new JsonEndValueForgeCast(other.Detail.UnderlyingClassName);
            JsonWriteForge writeForge;
            if (other.Detail.OptionalUnderlyingProvided == null) {
                writeForge = new JsonWriteForgeByMethod("WriteNested");
            }
            else {
                writeForge = new ProxyJsonWriteForge((
                        refs,
                        method,
                        classScope) =>
                    StaticMethod(
                        typeof(JsonWriteUtil),
                        "WriteNested",
                        refs.Writer,
                        refs.Field,
                        NewInstance(other.Detail.DelegateFactoryClassName)));
            }

            return new JsonForgeDesc(fieldName, startObject, null, end, writeForge);
        }

        public static JsonForgeDesc ForgeArray(
            String fieldName,
            JsonEventType other)
        {
            JsonDelegateForge startArray = new JsonDelegateForgeWithDelegateFactoryArray(
                other.Detail.DelegateFactoryClassName,
                other.UnderlyingType);
            JsonEndValueForge end = new JsonEndValueForgeCast(TypeHelper.GetArrayType(other.UnderlyingType));
            JsonWriteForge writeForge;
            if (other.Detail.OptionalUnderlyingProvided == null) {
                writeForge = new JsonWriteForgeByMethod("WriteNestedArray");
            }
            else {
                writeForge = new ProxyJsonWriteForge((
                        refs,
                        method,
                        classScope) =>
                    StaticMethod(
                        typeof(JsonWriteUtil),
                        "WriteNestedArray",
                        refs.Writer,
                        refs.Field,
                        NewInstance(other.Detail.DelegateFactoryClassName)));
            }

            return new JsonForgeDesc(fieldName, null, startArray, end, writeForge);
        }
    }
}