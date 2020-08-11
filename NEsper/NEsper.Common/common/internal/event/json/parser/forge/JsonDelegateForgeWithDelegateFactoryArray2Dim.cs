///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.json.parser.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.json.parser.forge
{
    public class JsonDelegateForgeWithDelegateFactoryArray2Dim : JsonDelegateForge
    {
        private readonly Type componentType;

        private readonly string delegateFactoryClassName;

        public JsonDelegateForgeWithDelegateFactoryArray2Dim(
            string delegateFactoryClassName,
            Type componentType)
        {
            this.delegateFactoryClassName = delegateFactoryClassName;
            this.componentType = componentType;
        }

        public CodegenExpression NewDelegate(
            JsonDelegateRefs fields,
            CodegenMethod parent,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(JsonDeserializerEventObjectArray2Dim), typeof(JsonForgeFactoryEventTypeTyped), classScope);
            method.Block
                .DeclareVar(typeof(JsonDelegateFactory), "factory", NewInstance(delegateFactoryClassName))
                .MethodReturn(NewInstance(typeof(JsonDeserializerEventObjectArray2Dim), fields.BaseHandler, fields.This, Ref("factory"), Constant(componentType)));
            return LocalMethod(method);
        }
    }
} // end of namespace