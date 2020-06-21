///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.airegistry;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.serde;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.common.@internal.context.aifactory.createtable
{
    public class StatementAgentInstanceFactoryCreateTable : StatementAgentInstanceFactory,
        StatementReadyCallback
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private AggregationRowFactory _aggregationRowFactory;
        private DataInputOutputSerdeWCollation<AggregationRow> _aggregationSerde;
        private TableMetadataInternalEventToPublic _eventToPublic;
        private EventPropertyValueGetter _primaryKeyGetter;
        private DataInputOutputSerde<object> _primaryKeySerde;
        private MultiKeyFromObjectArray _primaryKeyObjectArrayTransform;
        private MultiKeyFromMultiKey _primaryKeyIntoTableTransform;
        private DataInputOutputSerde<object>[] _propertyForges;
        
        private Table _table;
        private string _tableName;
        private EventType _publicEventType;

        public TableMetadataInternalEventToPublic EventToPublic {
            get => _eventToPublic;
            set => _eventToPublic = value;
        }

        public string TableName {
            get => _tableName;
            set => _tableName = value;
        }

        public EventType PublicEventType {
            get => _publicEventType;
            set => _publicEventType = value;
        }

        public AggregationRowFactory AggregationRowFactory {
            get => _aggregationRowFactory;
            set => _aggregationRowFactory = value;
        }

        public DataInputOutputSerdeWCollation<AggregationRow> AggregationSerde {
            get => _aggregationSerde;
            set => _aggregationSerde = value;
        }

        public EventPropertyValueGetter PrimaryKeyGetter {
            get => _primaryKeyGetter;
            set => _primaryKeyGetter = value;
        }

        public DataInputOutputSerde<object> PrimaryKeySerde {
            get => _primaryKeySerde;
            set => _primaryKeySerde = value;
        }

        public MultiKeyFromObjectArray PrimaryKeyObjectArrayTransform {
            get => _primaryKeyObjectArrayTransform;
            set => _primaryKeyObjectArrayTransform = value;
        }

        public MultiKeyFromMultiKey PrimaryKeyIntoTableTransform {
            get => _primaryKeyIntoTableTransform;
            set => _primaryKeyIntoTableTransform = value;
        }

        public DataInputOutputSerde<object>[] PropertyForges {
            get => _propertyForges;
            set => _propertyForges = value;
        }

        public EventType StatementEventType {
            get => _publicEventType;
        }

        public void StatementCreate(StatementContext statementContext)
        {
        }

        public void StatementDestroy(StatementContext statementContext)
        {
            statementContext.TableManagementService.DestroyTable(statementContext.DeploymentId, _tableName);
        }

        public void StatementDestroyPreconditions(StatementContext statementContext)
        {
        }

        public StatementAgentInstanceFactoryResult NewContext(
            AgentInstanceContext agentInstanceContext,
            bool isRecoveringResilient)
        {
            var tableState =
                agentInstanceContext.TableManagementService.AllocateTableInstance(_table, agentInstanceContext);
            var finalView = new TableInstanceViewable(_table, tableState);

            AgentInstanceMgmtCallback stop = new ProxyAgentInstanceMgmtCallback {
                ProcStop = services => {
                    var instance = _table.GetTableInstance(agentInstanceContext.AgentInstanceId);
                    if (instance == null) {
                        Log.Warn("Table instance by name '" + _tableName + "' has not been found");
                    }
                    else {
                        instance.Destroy();
                    }
                }
            };

            return new StatementAgentInstanceFactoryCreateTableResult(
                finalView,
                stop,
                agentInstanceContext,
                tableState);
        }

        public AIRegistryRequirements RegistryRequirements => AIRegistryRequirements.NoRequirements();

        public IReaderWriterLock ObtainAgentInstanceLock(
            StatementContext statementContext,
            int agentInstanceId)
        {
            return AgentInstanceUtil.NewLock(statementContext);
        }

        public void Ready(
            StatementContext statementContext,
            ModuleIncidentals moduleIncidentals,
            bool recovery)
        {
            _table = statementContext.TableManagementService.GetTable(statementContext.DeploymentId, _tableName);
            if (_table == null) {
                throw new IllegalStateException("Table '" + _tableName + "' has not be registered");
            }

            _table.StatementContextCreateTable = statementContext;
            _table.EventToPublic = _eventToPublic;
            _table.AggregationRowFactory = _aggregationRowFactory;
            _table.TableSerdes = new TableSerdes(_propertyForges, _aggregationSerde);
            _table.PrimaryKeyGetter = _primaryKeyGetter;
            _table.PrimaryKeySerde = _primaryKeySerde;
            _table.PrimaryKeyObjectArrayTransform = _primaryKeyObjectArrayTransform;
            _table.PrimaryKeyIntoTableTransform = _primaryKeyIntoTableTransform;

            _table.TableReady();
        }
    }
} // end of namespace