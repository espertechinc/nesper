///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
    public interface EventPropertyGetterMappedSPI : EventPropertyGetterMapped
    {
        CodegenExpression EventBeanGetMappedCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope,
            CodegenExpression beanExpression,
            CodegenExpression key);
    }

    public class ProxyEventPropertyGetterMappedSPI : EventPropertyGetterMappedSPI
    {
        public Func<CodegenMethodScope, CodegenClassScope, CodegenExpression, CodegenExpression, CodegenExpression>
            ProcEventBeanGetMappedCodegen;

        public Func<EventBean, string, object> ProcGet;

        public object Get(
            EventBean eventBean,
            string mapKey)
        {
            return ProcGet.Invoke(eventBean, mapKey);
        }

        public CodegenExpression EventBeanGetMappedCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope,
            CodegenExpression beanExpression,
            CodegenExpression key)
        {
            return ProcEventBeanGetMappedCodegen.Invoke(
                codegenMethodScope,
                codegenClassScope,
                beanExpression,
                key);
        }
    }
} // end of namespace