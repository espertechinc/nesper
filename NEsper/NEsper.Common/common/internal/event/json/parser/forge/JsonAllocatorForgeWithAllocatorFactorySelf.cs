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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder; // localMethod

// newInstance

namespace com.espertech.esper.common.@internal.@event.json.parser.forge
{
    public class JsonAllocatorForgeWithAllocatorFactorySelf : JsonAllocatorForge
    {
        private readonly Type beanClassName;

        private readonly string delegateClassName;

        public JsonAllocatorForgeWithAllocatorFactorySelf(
            string delegateClassName,
            Type beanClassName)
        {
            this.delegateClassName = delegateClassName;
            this.beanClassName = beanClassName;
        }

        public CodegenExpression NewDelegate(
            JsonDelegateRefs fields,
            CodegenMethod parent,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(JsonDeserializerBase), typeof(JsonForgeFactoryEventTypeTyped), classScope);
            method.Block
                .MethodReturn(NewInstanceInner(delegateClassName, fields.BaseHandler, fields.This, NewInstance(beanClassName)));
            return LocalMethod(method);
        }
    }
} // end of namespace