///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.index.hash;
using com.espertech.esper.common.@internal.epl.join.lookup;
using com.espertech.esper.common.@internal.epl.join.queryplan;
using com.espertech.esper.common.@internal.epl.lookupplansubord;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.strategy;
using com.espertech.esper.common.@internal.epl.table.update;

namespace com.espertech.esper.common.@internal.epl.table.core
{
    public interface Table
    {
        EventPropertyValueGetter PrimaryKeyGetter { set; }

        string Name { get; }

        TableMetadataInternalEventToPublic EventToPublic { get; set; }

        TableMetaData MetaData { get; }

        TableSerdes TableSerdes { get; set; }

        DataInputOutputSerde PrimaryKeySerde { get; set; }

        MultiKeyFromObjectArray PrimaryKeyObjectArrayTransform { get; set; }

        MultiKeyFromMultiKey PrimaryKeyIntoTableTransform { get; set; }
        
        AggregationRowFactory AggregationRowFactory { get; set; }

        StatementContext StatementContextCreateTable { get; set; }

        bool IsGrouped { get; }

        EventTableIndexMetadata EventTableIndexMetadata { get; }

        PropertyHashedEventTableFactory PrimaryIndexFactory { get; }

        TableInstance TableInstanceNoContext { get; }

        ICollection<TableUpdateStrategyRedoCallback> UpdateStrategyCallbacks { get; }

        void TableReady();

        TableAndLockProvider GetStateProvider(
            int agentInstanceId,
            bool writesToTables);

        TableInstance GetTableInstance(int agentInstanceId);

        void ValidateAddIndex(
            string deploymentId,
            string statementName,
            string indexName,
            string indexModuleName,
            QueryPlanIndexItem explicitIndexDesc,
            IndexMultiKey indexMultiKey);

        void RemoveIndexReferencesStmtMayRemoveIndex(
            IndexMultiKey indexMultiKey,
            string deploymentId,
            string statementName);

        void RemoveAllInstanceIndexes(IndexMultiKey indexMultiKey);

        void AddUpdateStrategyCallback(TableUpdateStrategyRedoCallback callback);

        void RemoveUpdateStrategyCallback(TableUpdateStrategyRedoCallback callback);
    }
} // end of namespace