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

namespace com.espertech.esper.common.@internal.settings
{
    public class RuntimeSettingsTimeZoneField : CodegenFieldSharable
    {
        public static readonly RuntimeSettingsTimeZoneField INSTANCE = new RuntimeSettingsTimeZoneField();

        private RuntimeSettingsTimeZoneField()
        {
        }

        public Type Type()
        {
            return typeof(TimeZoneInfo);
        }

        public CodegenExpression InitCtorScoped()
        {
            return ExprDotMethodChain(EPStatementInitServicesConstants.REF)
                .Get(EPStatementInitServicesConstants.IMPORTSERVICERUNTIME)
                .Get("TimeZone");
        }
    }
} // end of namespace