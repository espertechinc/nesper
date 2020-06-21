///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.json.parser.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.json.parser.forge
{
    public class JsonDelegateForgeWithDelegateFactoryCollection : JsonDelegateForge
    {
        private readonly string delegateFactoryClassName;

        public JsonDelegateForgeWithDelegateFactoryCollection(string delegateFactoryClassName)
        {
            this.delegateFactoryClassName = delegateFactoryClassName;
        }

        public CodegenExpression NewDelegate(
            JsonDelegateRefs fields,
            CodegenMethod parent,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(JsonDelegateCollection), typeof(JsonDelegateForgeWithDelegateFactoryCollection), classScope);
            method.Block
                .DeclareVar(typeof(JsonDelegateFactory), "factory", NewInstance(delegateFactoryClassName))
                .MethodReturn(NewInstance(typeof(JsonDelegateCollection), fields.BaseHandler, fields.This, Ref("factory")));
            return LocalMethod(method);
        }
    }
} // end of namespace