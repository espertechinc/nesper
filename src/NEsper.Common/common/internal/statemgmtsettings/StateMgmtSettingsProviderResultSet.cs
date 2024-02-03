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
    public interface StateMgmtSettingsProviderResultSet
    {
        StateMgmtSetting SimpleOutputAll(
            FabricCharge fabricCharge,
            StatementRawInfo raw,
            ResultSetProcessorSimpleForge forge);

        StateMgmtSetting SimpleOutputLast(
            FabricCharge fabricCharge,
            StatementRawInfo raw,
            ResultSetProcessorSimpleForge forge);

        StateMgmtSetting RowForAllOutputAll(
            FabricCharge fabricCharge,
            StatementRawInfo raw,
            ResultSetProcessorRowForAllForge forge);

        StateMgmtSetting RowForAllOutputLast(
            FabricCharge fabricCharge,
            StatementRawInfo raw,
            ResultSetProcessorRowForAllForge forge);

        StateMgmtSetting AggGroupedOutputFirst(
            FabricCharge fabricCharge,
            StatementRawInfo raw,
            ResultSetProcessorAggregateGroupedForge forge);

        StateMgmtSetting AggGroupedOutputAllOpt(
            FabricCharge fabricCharge,
            StatementRawInfo raw,
            ResultSetProcessorAggregateGroupedForge forge);

        StateMgmtSetting AggGroupedOutputAll(
            FabricCharge fabricCharge,
            StatementRawInfo raw,
            ResultSetProcessorAggregateGroupedForge forge);

        StateMgmtSetting AggGroupedOutputLast(
            FabricCharge fabricCharge,
            StatementRawInfo raw,
            ResultSetProcessorAggregateGroupedForge forge);

        StateMgmtSetting RowPerEventOutputAll(
            FabricCharge fabricCharge,
            StatementRawInfo raw,
            ResultSetProcessorRowPerEventForge forge);

        StateMgmtSetting RowPerEventOutputLast(
            FabricCharge fabricCharge,
            StatementRawInfo raw,
            ResultSetProcessorRowPerEventForge forge);

        StateMgmtSetting RowPerGroupOutputFirst(
            FabricCharge fabricCharge,
            StatementRawInfo raw,
            ResultSetProcessorRowPerGroupForge forge);

        StateMgmtSetting RowPerGroupOutputAllOpt(
            FabricCharge fabricCharge,
            StatementRawInfo raw,
            ResultSetProcessorRowPerGroupForge forge);

        StateMgmtSetting RowPerGroupOutputAll(
            FabricCharge fabricCharge,
            StatementRawInfo raw,
            ResultSetProcessorRowPerGroupForge forge);

        StateMgmtSetting RowPerGroupOutputLast(
            FabricCharge fabricCharge,
            StatementRawInfo raw,
            ResultSetProcessorRowPerGroupForge forge);

        StateMgmtSetting RowPerGroupUnbound(
            FabricCharge fabricCharge,
            StatementRawInfo raw,
            ResultSetProcessorRowPerGroupForge forge);

        StateMgmtSetting RollupOutputLast(
            FabricCharge fabricCharge,
            StatementRawInfo raw,
            ResultSetProcessorRowPerGroupRollupForge forge);

        StateMgmtSetting RollupOutputAll(
            FabricCharge fabricCharge,
            StatementRawInfo raw,
            ResultSetProcessorRowPerGroupRollupForge forge);

        StateMgmtSetting RollupOutputSnapshot(
            FabricCharge fabricCharge,
            StatementRawInfo raw,
            ResultSetProcessorRowPerGroupRollupForge forge);

        StateMgmtSetting RollupOutputFirst(
            FabricCharge fabricCharge,
            StatementRawInfo raw,
            ResultSetProcessorRowPerGroupRollupForge forge);

        StateMgmtSetting OutputLimited(
            FabricCharge fabricCharge,
            StatementRawInfo raw,
            EventType[] eventTypes,
            EventType resultEventType);

        StateMgmtSetting OutputCount(FabricCharge fabricCharge);
        StateMgmtSetting OutputTime(FabricCharge fabricCharge);
        StateMgmtSetting OutputExpression(FabricCharge fabricCharge);

        StateMgmtSetting OutputFirst(
            FabricCharge fabricCharge,
            ResultSetProcessorType resultSetProcessorType,
            EventType[] typesPerStream);

        StateMgmtSetting OutputAfter(FabricCharge fabricCharge);
    }
} // end of namespace