///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.compile.stage2;

namespace com.espertech.esper.common.@internal.statemgmtsettings
{
    public class StateMgmtSettingsProviderDefault : StateMgmtSettingsProvider
    {
        public static readonly StateMgmtSettingsProviderDefault INSTANCE = new StateMgmtSettingsProviderDefault();

        private StateMgmtSettingsProviderDefault()
        {
        }

        public StateMgmtSetting GetView(
            StatementRawInfo raw,
            int streamNumber,
            bool subquery,
            bool grouped,
            AppliesTo appliesTo)
        {
            return StateMgmtSettingDefault.INSTANCE;
        }

        public StateMgmtSetting GetPattern(
            StatementRawInfo raw,
            int streamNum,
            AppliesTo appliesTo)
        {
            return StateMgmtSettingDefault.INSTANCE;
        }

        public StateMgmtSetting GetResultSet(
            StatementRawInfo raw,
            AppliesTo appliesTo)
        {
            return StateMgmtSettingDefault.INSTANCE;
        }

        public StateMgmtSetting GetContext(
            StatementRawInfo raw,
            string contextName,
            AppliesTo contextCategory)
        {
            return StateMgmtSettingDefault.INSTANCE;
        }

        public StateMgmtSetting GetAggregation(
            StatementRawInfo raw,
            AppliesTo appliesTo)
        {
            return StateMgmtSettingDefault.INSTANCE;
        }

        public StateMgmtSetting GetIndex(
            StatementRawInfo raw,
            AppliesTo appliesTo)
        {
            return StateMgmtSettingDefault.INSTANCE;
        }

        public StateMgmtSetting GetRowRecog(
            StatementRawInfo raw,
            AppliesTo appliesTo)
        {
            return StateMgmtSettingDefault.INSTANCE;
        }
    }
} // end of namespace