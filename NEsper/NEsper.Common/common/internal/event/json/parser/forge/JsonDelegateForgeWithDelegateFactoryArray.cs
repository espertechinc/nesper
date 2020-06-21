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
    public class JsonDelegateForgeWithDelegateFactoryArray : JsonDelegateForge
    {
        private readonly string delegateFactoryClassName;
        private readonly Type underlyingType;

        public JsonDelegateForgeWithDelegateFactoryArray(
            string delegateFactoryClassName,
            Type underlyingType)
        {
            this.delegateFactoryClassName = delegateFactoryClassName;
            this.underlyingType = underlyingType;
        }

        public CodegenExpression NewDelegate(
            JsonDelegateRefs fields,
            CodegenMethod parent,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(JsonDelegateEventObjectArray), typeof(JsonForgeFactoryEventTypeTyped), classScope);
            method.Block
                .DeclareVar(typeof(JsonDelegateFactory), "factory", NewInstance(delegateFactoryClassName))
                .MethodReturn(NewInstance(typeof(JsonDelegateEventObjectArray), fields.BaseHandler, fields.This, Ref("factory"), Constant(underlyingType)));
            return LocalMethod(method);
        }
    }
} // end of namespace