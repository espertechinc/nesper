///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.airegistry;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.serde;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;

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
        private Table _table;
        private string _tableName;

        public TableMetadataInternalEventToPublic EventToPublic {
            set => _eventToPublic = value;
        }

        public string TableName {
            set => _tableName = value;
        }

        public EventType PublicEventType {
            set => StatementEventType = value;
        }

        public AggregationRowFactory AggregationRowFactory {
            set => _aggregationRowFactory = value;
        }

        public DataInputOutputSerdeWCollation<AggregationRow> AggregationSerde {
            set => _aggregationSerde = value;
        }

        public EventPropertyValueGetter PrimaryKeyGetter {
            set => _primaryKeyGetter = value;
        }

        public EventType StatementEventType { get; private set; }

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

            AgentInstanceStopCallback stop = new ProxyAgentInstanceStopCallback {
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

        public StatementAgentInstanceLock ObtainAgentInstanceLock(
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
            _table.TableSerdes =
                statementContext.TableManagementService.GetTableSerdes(_table, _aggregationSerde, statementContext);
            _table.PrimaryKeyGetter = _primaryKeyGetter;
            _table.TableReady();
        }
    }
} // end of namespace