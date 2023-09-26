///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public class ExprEventEvaluatorForgeFromProp : ExprEventEvaluatorForge
    {
        private readonly EventPropertyValueGetterForge _getter;

        public ExprEventEvaluatorForgeFromProp(EventPropertyValueGetterForge getter)
        {
            _getter = getter;
        }

        public CodegenExpression EventBeanWithCtxGet(
            CodegenExpression beanExpression,
            CodegenExpression ctxExpression,
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            return _getter.EventBeanGetCodegen(beanExpression, parent, classScope);
        }
    }
} // end of namespace