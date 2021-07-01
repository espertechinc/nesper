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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.metrics.instrumentation
{
    public class InstrumentationField : CodegenFieldSharable
    {
        public static readonly InstrumentationField INSTANCE = new InstrumentationField();

        private InstrumentationField()
        {
        }

        public Type Type()
        {
            return typeof(void);
        }

        public CodegenExpression InitCtorScoped()
        {
            return StaticMethod(InstrumentationConstants.RUNTIME_HELPER_CLASS, "Get");
        }
    }
} // end of namespace