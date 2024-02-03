///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.statemgmtsettings;

namespace com.espertech.esper.compiler.client.option
{
    /// <summary>
    /// Implement this interface to provide or override the state management settings, for use with high-availability only.
    /// </summary>
    public interface StateMgmtSettingOption : StateMgmtSettingsProxy
    {
        /// <summary>
        /// Return a state management setting.
        /// </summary>
        /// <param name="env">information about the state management setting that is being determined</param>
        /// <returns>setting</returns>
        StateMgmtSettingBucket GetValue(StateMgmtSettingContext env);

        StateMgmtSettingBucket Configure(
            StatementRawInfo raw,
            AppliesTo appliesTo,
            StateMgmtSettingBucket setting)
        {
            return GetValue(new StateMgmtSettingContext(raw, appliesTo, setting));
        }
    }
}