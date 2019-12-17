///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

namespace com.espertech.esper.common.@internal.@event.core
{
    public interface EventPropertyValueGetterForge
    {
        CodegenExpression EventBeanGetCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope);
    }

    public class ProxyEventPropertyValueGetterForge : EventPropertyValueGetterForge
    {
        public Func<CodegenExpression, CodegenMethodScope, CodegenClassScope, CodegenExpression>
            ProcEventBeanGetCodegen;

        public ProxyEventPropertyValueGetterForge(
            Func<CodegenExpression, CodegenMethodScope, CodegenClassScope, CodegenExpression> procEventBeanGetCodegen)
        {
            ProcEventBeanGetCodegen = procEventBeanGetCodegen;
        }

        public ProxyEventPropertyValueGetterForge()
        {
        }

        public CodegenExpression EventBeanGetCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ProcEventBeanGetCodegen(beanExpression, codegenMethodScope, codegenClassScope);
        }
    }
} // end of namespace