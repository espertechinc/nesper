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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.schedule
{
    public class TimeProviderField : CodegenFieldSharable
    {
        public readonly static TimeProviderField INSTANCE = new TimeProviderField();

        private TimeProviderField()
        {
        }

        public Type Type()
        {
            return typeof(TimeProvider);
        }

        public CodegenExpression InitCtorScoped()
        {
            return ExprDotName(
                EPStatementInitServicesConstants.REF,
                EPStatementInitServicesConstants.TIMEPROVIDER);
        }
    }
} // end of namespace