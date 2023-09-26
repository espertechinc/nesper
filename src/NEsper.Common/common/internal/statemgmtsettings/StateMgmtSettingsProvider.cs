///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.classprovided.compiletime;
using com.espertech.esper.common.@internal.epl.classprovided.core;
using com.espertech.esper.common.@internal.epl.namedwindow.path;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.epl.rowrecog.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.fabric;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.common.@internal.statemgmtsettings
{
    public interface StateMgmtSettingsProvider
    {
        StateMgmtSettingsProviderContext Context { get; }
        StateMgmtSettingsProviderResultSet ResultSet { get; }
        StateMgmtSettingsProviderIndex Index { get; }
        FabricCharge NewCharge();

        void Spec(
            IList<FabricStatement> formatStatements,
            ModuleCompileTimeServices compileTimeServices,
            IDictionary<string, byte[]> moduleBytes);

        FabricStatement Statement(
            int statementNumber,
            ContextCompileTimeDescriptor context,
            FabricCharge fabricCharge);

        StateMgmtSetting View(
            FabricCharge fabricCharge,
            int[] grouping,
            ViewForgeEnv viewForgeEnv,
            ViewFactoryForge forge);

        StateMgmtSetting Aggregation(
            FabricCharge fabricCharge,
            AggregationAttributionKey attributionKey,
            StatementRawInfo raw,
            AggregationServiceFactoryForge forge);

        StateMgmtSetting RowRecogPartitionState(
            FabricCharge fabricCharge,
            StatementRawInfo raw,
            RowRecogDescForge forge,
            MatchRecognizeSpec spec);

        StateMgmtSetting RowRecogScheduleState(
            FabricCharge fabricCharge,
            StatementRawInfo raw,
            RowRecogDescForge forge,
            MatchRecognizeSpec spec);

        StateMgmtSetting Previous(
            FabricCharge fabricCharge,
            StatementRawInfo raw,
            int stream,
            int? subqueryNumber,
            EventType eventType);

        StateMgmtSetting Prior(
            FabricCharge fabricCharge,
            StatementRawInfo raw,
            int streamNum,
            int? subqueryNumber,
            bool unbound,
            EventType eventType,
            ISet<int> priorRequesteds);

        StateMgmtSetting TableUnkeyed(
            FabricCharge fabricCharge,
            string tableName,
            TableAccessAnalysisResult tableInternalType,
            StatementRawInfo statementRawInfo);

        void FilterViewable(
            FabricCharge fabricCharge,
            int stream,
            bool isCanIterateUnbound,
            StatementRawInfo statementRawInfo,
            EventType eventType);

        void FilterNonContext(
            FabricCharge fabricCharge,
            FilterSpecTracked spec);

        void FilterSubtypes(
            FabricCharge fabricCharge,
            IList<FilterSpecTracked> provider,
            ContextCompileTimeDescriptor contextDescriptor,
            StatementSpecCompiled compiled);

        void Pattern(
            FabricCharge fabricCharge,
            PatternAttributionKey attributionKey,
            PatternStreamSpecCompiled patternStreamSpec,
            StatementRawInfo raw);

        void NamedWindow(
            FabricCharge fabricCharge,
            StatementRawInfo statementRawInfo,
            NamedWindowMetaData metaData,
            EventType eventType);

        void Table(
            FabricCharge fabricCharge,
            string tableName,
            TableAccessAnalysisResult plan,
            StatementRawInfo statementRawInfo);

        void InlinedClassesLocal(
            FabricCharge fabricCharge,
            ClassProvidedPrecompileResult classesInlined);

        void InlinedClasses(
            FabricCharge fabricCharge,
            ClassProvided classProvided);

        void HistoricalExpiryTime(
            FabricCharge fabricCharge,
            int streamNum);

        void Schedules(
            FabricCharge fabricCharge,
            IList<ScheduleHandleTracked> trackeds);
    }
} // end of namespace