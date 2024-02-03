///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.statemgmtsettings
{
    public class StateMgmtSettingBucketNone : StateMgmtSettingBucket
    {
        public static readonly StateMgmtSettingBucketNone INSTANCE = new StateMgmtSettingBucketNone();

        private StateMgmtSettingBucketNone()
        {
        }

        public CodegenExpression ToExpression()
        {
            return EnumValue(typeof(StateMgmtSettingBucketNone), "INSTANCE");
        }
    }
} // end of namespace