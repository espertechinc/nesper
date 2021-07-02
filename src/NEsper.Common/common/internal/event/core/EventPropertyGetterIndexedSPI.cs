///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

namespace com.espertech.esper.common.@internal.@event.core
{
    public interface EventPropertyGetterIndexedSPI : EventPropertyGetterIndexed
    {
        CodegenExpression EventBeanGetIndexedCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope,
            CodegenExpression beanExpression,
            CodegenExpression key);
    }

    public class ProxyEventPropertyGetterIndexedSPI : EventPropertyGetterIndexedSPI
    {
        public Func<CodegenMethodScope, CodegenClassScope, CodegenExpression, CodegenExpression, CodegenExpression>
            procEventBeanGetIndexedCodegen;

        public Func<EventBean, int, object> procGet;

        public object Get(
            EventBean eventBean,
            int index)
        {
            return procGet(eventBean, index);
        }

        public CodegenExpression EventBeanGetIndexedCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope,
            CodegenExpression beanExpression,
            CodegenExpression key)
        {
            return procEventBeanGetIndexedCodegen.Invoke(
                codegenMethodScope,
                codegenClassScope,
                beanExpression,
                key);
        }
    }
} // end of namespace