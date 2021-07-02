///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.util;
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
        StateMgmtSetting GetValue(StateMgmtSettingContext env);

        // default implementation
        //
        // StateMgmtSetting Configure(
        //     StatementRawInfo raw,
        //     AppliesTo appliesTo,
        //     StateMgmtSetting setting)
        // {
        //     return getValue(new StateMgmtSettingContext(raw, appliesTo, setting));
        // }
    }
}