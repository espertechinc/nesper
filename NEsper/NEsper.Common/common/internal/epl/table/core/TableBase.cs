///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.index.hash;
using com.espertech.esper.common.@internal.epl.@join.lookup;
using com.espertech.esper.common.@internal.epl.@join.queryplan;
using com.espertech.esper.common.@internal.epl.lookupplansubord;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.strategy;
using com.espertech.esper.common.@internal.epl.table.update;

namespace com.espertech.esper.common.@internal.epl.table.core
{
    public abstract class TableBase : Table
    {
        internal readonly TableMetaData metaData;
        internal AggregationRowFactory aggregationRowFactory;
        internal TableMetadataInternalEventToPublic eventToPublic;
        internal EventPropertyValueGetter primaryKeyGetter;
        internal PropertyHashedEventTableFactory primaryKeyIndexFactory;
        internal StatementContext statementContextCreateTable;
        internal TableSerdes tableSerdes;

        protected ISet<TableUpdateStrategyRedoCallback> updateStrategyRedoCallbacks =
            new HashSet<TableUpdateStrategyRedoCallback>();

        public TableBase(TableMetaData metaData)
        {
            this.metaData = metaData;
        }

        public EventPropertyValueGetter PrimaryKeyGetter {
            set => primaryKeyGetter = value;
        }

        public void TableReady()
        {
            if (metaData.IsKeyed) {
                primaryKeyIndexFactory = SetupPrimaryKeyIndexFactory();
            }
        }

        public TableMetadataInternalEventToPublic EventToPublic {
            get => eventToPublic;
            set => eventToPublic = value;
        }

        public TableMetaData MetaData => metaData;

        public TableSerdes TableSerdes {
            get => tableSerdes;
            set => tableSerdes = value;
        }

        public AggregationRowFactory AggregationRowFactory {
            get => aggregationRowFactory;
            set => aggregationRowFactory = value;
        }

        public StatementContext StatementContextCreateTable {
            get => statementContextCreateTable;
            set => statementContextCreateTable = value;
        }

        public PropertyHashedEventTableFactory PrimaryIndexFactory => primaryKeyIndexFactory;

        public string Name => metaData.TableName;

        public EventTableIndexMetadata EventTableIndexMetadata => metaData.IndexMetadata;

        public void ValidateAddIndex(
            string deploymentId,
            string statementName,
            string indexName,
            string indexModuleName,
            QueryPlanIndexItem explicitIndexDesc,
            IndexMultiKey indexMultiKey)
        {
            metaData.IndexMetadata.AddIndexExplicit(
                false,
                indexMultiKey,
                indexName,
                indexModuleName,
                explicitIndexDesc,
                deploymentId);
            foreach (var callback in updateStrategyRedoCallbacks) {
                callback.InitTableUpdateStrategy(this);
            }
        }

        public void RemoveIndexReferencesStmtMayRemoveIndex(
            IndexMultiKey indexMultiKey,
            string deploymentId,
            string statementName)
        {
            var last = metaData.IndexMetadata.RemoveIndexReference(indexMultiKey, deploymentId);
            if (last) {
                metaData.IndexMetadata.RemoveIndex(indexMultiKey);
                RemoveAllInstanceIndexes(indexMultiKey);
            }
        }

        public void RemoveAllInstanceIndexes(IndexMultiKey index)
        {
            var statementResourceService = statementContextCreateTable.StatementCPCacheService.StatementResourceService;
            if (metaData.OptionalContextName == null) {
                var holder = statementResourceService.Unpartitioned;
                if (holder != null && holder.TableInstance != null) {
                    holder.TableInstance.IndexRepository.RemoveIndex(index);
                }
            }
            else {
                foreach (var entry in statementResourceService.ResourcesPartitioned) {
                    if (entry.Value.TableInstance != null) {
                        entry.Value.TableInstance.IndexRepository.RemoveIndex(index);
                    }
                }
            }
        }

        public void AddUpdateStrategyCallback(TableUpdateStrategyRedoCallback callback)
        {
            updateStrategyRedoCallbacks.Add(callback);
        }

        public void RemoveUpdateStrategyCallback(TableUpdateStrategyRedoCallback callback)
        {
            updateStrategyRedoCallbacks.Remove(callback);
        }

        public ICollection<TableUpdateStrategyRedoCallback> UpdateStrategyCallbacks => updateStrategyRedoCallbacks;

        protected abstract PropertyHashedEventTableFactory SetupPrimaryKeyIndexFactory();

        public bool IsGrouped => metaData.KeyTypes != null && metaData.KeyTypes.Length > 0;

        public TableInstance GetTableInstanceNoRemake(int agentInstanceId)
        {
            if (metaData.OptionalContextName == null) {
                return TableInstanceNoContext;
            }

            var statementResourceService = statementContextCreateTable.StatementCPCacheService.StatementResourceService;
            var holder = statementResourceService.GetPartitioned(agentInstanceId);
            return holder == null ? null : holder.TableInstance;
        }

        public TableInstance TableInstanceNoContextNoRemake {
            get {
                var statementResourceService =
                    statementContextCreateTable.StatementCPCacheService.StatementResourceService;
                var holder = statementResourceService.Unpartitioned;
                return holder == null ? null : holder.TableInstance;
            }
        }

        public abstract TableInstance TableInstanceNoContext { get; }

        public abstract TableAndLockProvider GetStateProvider(
            int agentInstanceId,
            bool writesToTables);

        public abstract TableInstance GetTableInstance(int agentInstanceId);
    }
} // end of namespace