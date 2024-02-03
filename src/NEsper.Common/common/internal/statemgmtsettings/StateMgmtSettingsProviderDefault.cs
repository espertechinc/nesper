///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.artifact;
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
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.statemgmtsettings
{
    public class StateMgmtSettingsProviderDefault : StateMgmtSettingsProvider
    {
        public static readonly StateMgmtSettingsProviderDefault INSTANCE = new StateMgmtSettingsProviderDefault();

        private StateMgmtSettingsProviderDefault()
        {
        }

        public FabricCharge NewCharge()
        {
            return FabricChargeNonHA.INSTANCE;
        }

        public StateMgmtSetting View(
            FabricCharge fabricCharge,
            int[] grouping,
            ViewForgeEnv viewForgeEnv,
            ViewFactoryForge forge)
        {
            return StateMgmtSettingDefault.INSTANCE;
        }

        public StateMgmtSettingsProviderContext Context => StateMgmtSettingsProviderContextDefault.INSTANCE;

        public StateMgmtSettingsProviderResultSet ResultSet => StateMgmtSettingsProviderResultSetDefault.INSTANCE;

        public StateMgmtSettingsProviderIndex Index => StateMgmtSettingsProviderIndexDefault.INSTANCE;

        public StateMgmtSetting Aggregation(
            FabricCharge fabricCharge,
            AggregationAttributionKey attributionKey,
            StatementRawInfo raw,
            AggregationServiceFactoryForge forge)
        {
            return StateMgmtSettingDefault.INSTANCE;
        }

        public StateMgmtSetting Previous(
            FabricCharge fabricCharge,
            StatementRawInfo raw,
            int stream,
            int? subqueryNumber,
            EventType eventType)
        {
            return StateMgmtSettingDefault.INSTANCE;
        }

        public StateMgmtSetting Prior(
            FabricCharge fabricCharge,
            StatementRawInfo raw,
            int streamNum,
            int? subqueryNumber,
            bool unbound,
            EventType eventType,
            ISet<int> priorRequesteds)
        {
            return StateMgmtSettingDefault.INSTANCE;
        }

        public StateMgmtSetting RowRecogPartitionState(
            FabricCharge fabricCharge,
            StatementRawInfo raw,
            RowRecogDescForge forge,
            MatchRecognizeSpec spec)
        {
            return StateMgmtSettingDefault.INSTANCE;
        }

        public StateMgmtSetting RowRecogScheduleState(
            FabricCharge fabricCharge,
            StatementRawInfo raw,
            RowRecogDescForge forge,
            MatchRecognizeSpec spec)
        {
            return StateMgmtSettingDefault.INSTANCE;
        }

        public StateMgmtSetting TableUnkeyed(
            FabricCharge fabricCharge,
            string tableName,
            TableAccessAnalysisResult tableInternalType,
            StatementRawInfo statementRawInfo)
        {
            return StateMgmtSettingDefault.INSTANCE;
        }

        public void Spec(
            IList<FabricStatement> formatStatements,
            ModuleCompileTimeServices compileTimeServices,
            ICollection<IArtifact> artifacts)
        {
            throw new IllegalStateException("Not implemented for non-HA compile");
        }

        public FabricStatement Statement(
            int statementNumber,
            ContextCompileTimeDescriptor context,
            FabricCharge fabricCharge)
        {
            return null;
        }

        public void FilterViewable(
            FabricCharge fabricCharge,
            int stream,
            bool isCanIterateUnbound,
            StatementRawInfo statementRawInfo,
            EventType eventType)
        {
            // no action
        }

        public void FilterNonContext(
            FabricCharge fabricCharge,
            FilterSpecTracked spec)
        {
            // no action
        }

        public void NamedWindow(
            FabricCharge fabricCharge,
            StatementRawInfo statementRawInfo,
            NamedWindowMetaData metaData,
            EventType eventType)
        {
            // no action
        }

        public void Table(
            FabricCharge fabricCharge,
            string tableName,
            TableAccessAnalysisResult plan,
            StatementRawInfo statementRawInfo)
        {
            // no action
        }

        public void Pattern(
            FabricCharge fabricCharge,
            PatternAttributionKey attributionKey,
            PatternStreamSpecCompiled patternStreamSpec,
            StatementRawInfo raw)
        {
            // no action
        }

        public void InlinedClassesLocal(
            FabricCharge fabricCharge,
            ClassProvidedPrecompileResult classesInlined)
        {
            // no action
        }

        public void InlinedClasses(
            FabricCharge fabricCharge,
            ClassProvided classProvided)
        {
            // no action
        }

        public void FilterSubtypes(
            FabricCharge fabricCharge,
            IList<FilterSpecTracked> provider,
            ContextCompileTimeDescriptor contextDescriptor,
            StatementSpecCompiled compiled)
        {
            // no action
        }

        public void HistoricalExpiryTime(
            FabricCharge fabricCharge,
            int streamNum)
        {
            // no action
        }

        public void Schedules(
            FabricCharge fabricCharge,
            IList<ScheduleHandleTracked> trackeds)
        {
            // no action
        }
    }
} // end of namespace