///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.codegen.core;
using com.espertech.esper.codegen.model.expression;

namespace com.espertech.esper.events
{
    public interface EventPropertyGetterSPI : EventPropertyGetter
    {
        ICodegenExpression CodegenEventBeanGet(ICodegenExpression beanExpression, ICodegenContext context);
        ICodegenExpression CodegenEventBeanExists(ICodegenExpression beanExpression, ICodegenContext context);
        ICodegenExpression CodegenEventBeanFragment(ICodegenExpression beanExpression, ICodegenContext context);
        ICodegenExpression CodegenUnderlyingGet(ICodegenExpression underlyingExpression, ICodegenContext context);
        ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression, ICodegenContext context);
        ICodegenExpression CodegenUnderlyingFragment(ICodegenExpression underlyingExpression, ICodegenContext context);
    }
} // end of namespace
