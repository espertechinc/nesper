///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.epl.resultset.agggrouped;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.resultset.rowforall;
using com.espertech.esper.common.@internal.epl.resultset.rowperevent;
using com.espertech.esper.common.@internal.epl.resultset.rowpergroup;
using com.espertech.esper.common.@internal.epl.resultset.rowpergrouprollup;
using com.espertech.esper.common.@internal.epl.resultset.simple;
using com.espertech.esper.common.@internal.fabric;

namespace com.espertech.esper.common.@internal.statemgmtsettings
{
    public class StateMgmtSettingsProviderResultSetDefault : StateMgmtSettingsProviderResultSet
    {
        public static readonly StateMgmtSettingsProviderResultSetDefault INSTANCE =
            new StateMgmtSettingsProviderResultSetDefault();

        private StateMgmtSettingsProviderResultSetDefault()
        {
        }

        public StateMgmtSetting OutputLimited(
            FabricCharge fabricCharge,
            StatementRawInfo raw,
            EventType[] eventTypes,
            EventType resultEventType)
        {
            return StateMgmtSettingDefault.INSTANCE;
        }

        public StateMgmtSetting OutputCount(FabricCharge fabricCharge)
        {
            return StateMgmtSettingDefault.INSTANCE;
        }

        public StateMgmtSetting OutputTime(FabricCharge fabricCharge)
        {
            return StateMgmtSettingDefault.INSTANCE;
        }

        public StateMgmtSetting OutputExpression(FabricCharge fabricCharge)
        {
            return StateMgmtSettingDefault.INSTANCE;
        }

        public StateMgmtSetting OutputFirst(
            FabricCharge fabricCharge,
            ResultSetProcessorType resultSetProcessorType,
            EventType[] typesPerStream)
        {
            return StateMgmtSettingDefault.INSTANCE;
        }

        public StateMgmtSetting OutputAfter(FabricCharge fabricCharge)
        {
            return StateMgmtSettingDefault.INSTANCE;
        }

        public StateMgmtSetting RowForAllOutputAll(
            FabricCharge fabricCharge,
            StatementRawInfo raw,
            ResultSetProcessorRowForAllForge forge)
        {
            return StateMgmtSettingDefault.INSTANCE;
        }

        public StateMgmtSetting RowForAllOutputLast(
            FabricCharge fabricCharge,
            StatementRawInfo raw,
            ResultSetProcessorRowForAllForge forge)
        {
            return StateMgmtSettingDefault.INSTANCE;
        }

        public StateMgmtSetting AggGroupedOutputFirst(
            FabricCharge fabricCharge,
            StatementRawInfo raw,
            ResultSetProcessorAggregateGroupedForge forge)
        {
            return StateMgmtSettingDefault.INSTANCE;
        }

        public StateMgmtSetting AggGroupedOutputAllOpt(
            FabricCharge fabricCharge,
            StatementRawInfo raw,
            ResultSetProcessorAggregateGroupedForge forge)
        {
            return StateMgmtSettingDefault.INSTANCE;
        }

        public StateMgmtSetting AggGroupedOutputAll(
            FabricCharge fabricCharge,
            StatementRawInfo raw,
            ResultSetProcessorAggregateGroupedForge forge)
        {
            return StateMgmtSettingDefault.INSTANCE;
        }

        public StateMgmtSetting AggGroupedOutputLast(
            FabricCharge fabricCharge,
            StatementRawInfo raw,
            ResultSetProcessorAggregateGroupedForge forge)
        {
            return StateMgmtSettingDefault.INSTANCE;
        }

        public StateMgmtSetting RowPerEventOutputAll(
            FabricCharge fabricCharge,
            StatementRawInfo raw,
            ResultSetProcessorRowPerEventForge forge)
        {
            return StateMgmtSettingDefault.INSTANCE;
        }

        public StateMgmtSetting RowPerEventOutputLast(
            FabricCharge fabricCharge,
            StatementRawInfo raw,
            ResultSetProcessorRowPerEventForge forge)
        {
            return StateMgmtSettingDefault.INSTANCE;
        }

        public StateMgmtSetting SimpleOutputAll(
            FabricCharge fabricCharge,
            StatementRawInfo raw,
            ResultSetProcessorSimpleForge forge)
        {
            return StateMgmtSettingDefault.INSTANCE;
        }

        public StateMgmtSetting SimpleOutputLast(
            FabricCharge fabricCharge,
            StatementRawInfo raw,
            ResultSetProcessorSimpleForge forge)
        {
            return StateMgmtSettingDefault.INSTANCE;
        }

        public StateMgmtSetting RollupOutputLast(
            FabricCharge fabricCharge,
            StatementRawInfo raw,
            ResultSetProcessorRowPerGroupRollupForge forge)
        {
            return StateMgmtSettingDefault.INSTANCE;
        }

        public StateMgmtSetting RollupOutputFirst(
            FabricCharge fabricCharge,
            StatementRawInfo raw,
            ResultSetProcessorRowPerGroupRollupForge forge)
        {
            return StateMgmtSettingDefault.INSTANCE;
        }

        public StateMgmtSetting RollupOutputAll(
            FabricCharge fabricCharge,
            StatementRawInfo raw,
            ResultSetProcessorRowPerGroupRollupForge forge)
        {
            return StateMgmtSettingDefault.INSTANCE;
        }

        public StateMgmtSetting RollupOutputSnapshot(
            FabricCharge fabricCharge,
            StatementRawInfo raw,
            ResultSetProcessorRowPerGroupRollupForge forge)
        {
            return StateMgmtSettingDefault.INSTANCE;
        }

        public StateMgmtSetting RowPerGroupOutputFirst(
            FabricCharge fabricCharge,
            StatementRawInfo raw,
            ResultSetProcessorRowPerGroupForge forge)
        {
            return StateMgmtSettingDefault.INSTANCE;
        }

        public StateMgmtSetting RowPerGroupOutputAllOpt(
            FabricCharge fabricCharge,
            StatementRawInfo raw,
            ResultSetProcessorRowPerGroupForge forge)
        {
            return StateMgmtSettingDefault.INSTANCE;
        }

        public StateMgmtSetting RowPerGroupOutputAll(
            FabricCharge fabricCharge,
            StatementRawInfo raw,
            ResultSetProcessorRowPerGroupForge forge)
        {
            return StateMgmtSettingDefault.INSTANCE;
        }

        public StateMgmtSetting RowPerGroupOutputLast(
            FabricCharge fabricCharge,
            StatementRawInfo raw,
            ResultSetProcessorRowPerGroupForge forge)
        {
            return StateMgmtSettingDefault.INSTANCE;
        }

        public StateMgmtSetting RowPerGroupUnbound(
            FabricCharge fabricCharge,
            StatementRawInfo raw,
            ResultSetProcessorRowPerGroupForge forge)
        {
            return StateMgmtSettingDefault.INSTANCE;
        }
    }
} // end of namespace