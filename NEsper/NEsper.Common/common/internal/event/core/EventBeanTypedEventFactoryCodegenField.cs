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
using com.espertech.esper.common.@internal.context.module;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.core
{
    public class EventBeanTypedEventFactoryCodegenField : CodegenFieldSharable
    {
        public static readonly EventBeanTypedEventFactoryCodegenField INSTANCE =
            new EventBeanTypedEventFactoryCodegenField();

        private EventBeanTypedEventFactoryCodegenField()
        {
        }

        public Type Type()
        {
            return typeof(EventBeanTypedEventFactory);
        }

        public CodegenExpression InitCtorScoped()
        {
            return ExprDotMethod(
                EPStatementInitServicesConstants.REF,
                EPStatementInitServicesConstants.GETEVENTBEANTYPEDEVENTFACTORY);
        }
    }
} // end of namespace