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
    public interface StateMgmtSettingsProvider
    {
        StateMgmtSetting GetView(
            StatementRawInfo raw,
            int streamNumber,
            bool subquery,
            bool grouped,
            AppliesTo appliesTo);

        StateMgmtSetting GetPattern(
            StatementRawInfo raw,
            int streamNum,
            AppliesTo appliesTo);

        StateMgmtSetting GetResultSet(
            StatementRawInfo raw,
            AppliesTo appliesTo);

        StateMgmtSetting GetContext(
            StatementRawInfo raw,
            string contextName,
            AppliesTo appliesTo);

        StateMgmtSetting GetAggregation(
            StatementRawInfo raw,
            AppliesTo appliesTo);

        StateMgmtSetting GetIndex(
            StatementRawInfo raw,
            AppliesTo appliesTo);

        StateMgmtSetting GetRowRecog(
            StatementRawInfo raw,
            AppliesTo appliesTo);
    }
} // end of namespace