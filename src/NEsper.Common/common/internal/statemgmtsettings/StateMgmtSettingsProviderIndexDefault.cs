///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.epl.index.advanced.index.service;
using com.espertech.esper.common.@internal.epl.join.queryplan;
using com.espertech.esper.common.@internal.fabric;

namespace com.espertech.esper.common.@internal.statemgmtsettings
{
    public class StateMgmtSettingsProviderIndexDefault : StateMgmtSettingsProviderIndex
    {
        public static readonly StateMgmtSettingsProviderIndexDefault INSTANCE =
            new StateMgmtSettingsProviderIndexDefault();

        private StateMgmtSettingsProviderIndexDefault()
        {
        }

        public StateMgmtSetting Unindexed(
            FabricCharge fabricCharge,
            QueryPlanAttributionKey attributionKey,
            EventType eventType,
            StatementRawInfo raw)
        {
            return StateMgmtSettingDefault.INSTANCE;
        }

        public StateMgmtSetting IndexHash(
            FabricCharge fabricCharge,
            QueryPlanAttributionKey attributionKey,
            string indexName,
            EventType eventType,
            StateMgmtIndexDescHash indexDesc,
            StatementRawInfo raw)
        {
            return StateMgmtSettingDefault.INSTANCE;
        }

        public StateMgmtSetting IndexInSingle(
            FabricCharge fabricCharge,
            QueryPlanAttributionKey attributionKey,
            EventType eventType,
            StateMgmtIndexDescInSingle indexDesc,
            StatementRawInfo raw)
        {
            return StateMgmtSettingDefault.INSTANCE;
        }

        public StateMgmtSetting IndexInMulti(
            FabricCharge fabricCharge,
            QueryPlanAttributionKey attributionKey,
            EventType eventType,
            StateMgmtIndexDescInMulti indexDesc,
            StatementRawInfo raw)
        {
            return StateMgmtSettingDefault.INSTANCE;
        }

        public StateMgmtSetting Sorted(
            FabricCharge fabricCharge,
            QueryPlanAttributionKey attributionKey,
            string indexName,
            EventType eventType,
            StateMgmtIndexDescSorted indexDesc,
            StatementRawInfo raw)
        {
            return StateMgmtSettingDefault.INSTANCE;
        }

        public StateMgmtSetting Composite(
            FabricCharge fabricCharge,
            QueryPlanAttributionKey attributionKey,
            string indexName,
            EventType eventType,
            StateMgmtIndexDescComposite indexDesc,
            StatementRawInfo raw)
        {
            return StateMgmtSettingDefault.INSTANCE;
        }

        public StateMgmtSetting Advanced(
            FabricCharge fabricCharge,
            QueryPlanAttributionKey attributionKey,
            string indexName,
            EventType eventType,
            EventAdvancedIndexProvisionCompileTime advancedIndexProvisionDesc,
            StatementRawInfo raw)
        {
            return StateMgmtSettingDefault.INSTANCE;
        }
    }
} // end of namespace