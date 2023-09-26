///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.statemgmtsettings
{
    public class StateMgmtSettingDefault : StateMgmtSetting
    {
        public static readonly StateMgmtSettingDefault INSTANCE = new StateMgmtSettingDefault();

        public StateMgmtSettingDefault()
        {
        }

        public CodegenExpression ToExpression()
        {
            return ConstantNull();
        }
    }
} // end of namespace