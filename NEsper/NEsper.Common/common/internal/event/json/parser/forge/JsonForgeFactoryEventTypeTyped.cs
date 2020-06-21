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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

package com.espertech.esper.common.@internal.@event.json.parser.forge;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder; // newInstance
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder; // staticMethod

public class JsonForgeFactoryEventTypeTyped {
    public static JsonForgeDesc forgeNonArray(String fieldName, JsonEventType other) {
        JsonDelegateForge startObject = new JsonDelegateForgeWithDelegateFactory(other.getDetail().getDelegateFactoryClassName());
        JsonEndValueForge end = new JsonEndValueForgeCast(other.getDetail().getUnderlyingClassName());
        JsonWriteForge writeForge;
        if (other.getDetail().getOptionalUnderlyingProvided() == null) {
            writeForge = new JsonWriteForgeByMethod("writeNested");
        } else {
            writeForge = (refs, method, classScope) -> staticMethod(JsonWriteUtil.class, "writeNested", refs.getWriter(), refs.getField(), newInstance(other.getDetail().getDelegateFactoryClassName()));
        }
        return new JsonForgeDesc(fieldName, startObject, null, end, writeForge);
    }

    public static JsonForgeDesc forgeArray(String fieldName, JsonEventType other) {
        JsonDelegateForge startArray = new JsonDelegateForgeWithDelegateFactoryArray(other.getDetail().getDelegateFactoryClassName(), other.getUnderlyingType());
        JsonEndValueForge end = new JsonEndValueForgeCast(JavaClassHelper.getArrayType(other.getUnderlyingType()));
        JsonWriteForge writeForge;
        if (other.getDetail().getOptionalUnderlyingProvided() == null) {
            writeForge = new JsonWriteForgeByMethod("writeNestedArray");
        } else {
            writeForge = (refs, method, classScope) -> staticMethod(JsonWriteUtil.class, "writeNestedArray", refs.getWriter(), refs.getField(), newInstance(other.getDetail().getDelegateFactoryClassName()));
        }
        return new JsonForgeDesc(fieldName, null, startArray, end, writeForge);
    }
}
