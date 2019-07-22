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

        private AggregationRowFactory aggregationRowFactory;
        private DataInputOutputSerdeWCollation<object> aggregationSerde;
        private TableMetadataInternalEventToPublic eventToPublic;
        private EventPropertyValueGetter primaryKeyGetter;

        private Table table;
        private string tableName;

        public TableMetadataInternalEventToPublic EventToPublic {
            set => eventToPublic = value;
        }

        public string TableName {
            set => tableName = value;
        }

        public EventType PublicEventType {
            set => StatementEventType = value;
        }

        public AggregationRowFactory AggregationRowFactory {
            set => aggregationRowFactory = value;
        }

        public DataInputOutputSerdeWCollation<object> AggregationSerde {
            set => aggregationSerde = value;
        }

        public EventPropertyValueGetter PrimaryKeyGetter {
            set => primaryKeyGetter = value;
        }

        public EventType StatementEventType { get; private set; }

        public void StatementCreate(StatementContext statementContext)
        {
        }

        public void StatementDestroy(StatementContext statementContext)
        {
            statementContext.TableManagementService.DestroyTable(statementContext.DeploymentId, tableName);
        }

        public void StatementDestroyPreconditions(StatementContext statementContext)
        {
        }

        public StatementAgentInstanceFactoryResult NewContext(
            AgentInstanceContext agentInstanceContext,
            bool isRecoveringResilient)
        {
            var tableState =
                agentInstanceContext.TableManagementService.AllocateTableInstance(table, agentInstanceContext);
            var finalView = new TableInstanceViewable(table, tableState);

            AgentInstanceStopCallback stop = new ProxyAgentInstanceStopCallback {
                ProcStop = services => {
                    var instance = table.GetTableInstance(agentInstanceContext.AgentInstanceId);
                    if (instance == null) {
                        Log.Warn("Table instance by name '" + tableName + "' has not been found");
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
            table = statementContext.TableManagementService.GetTable(statementContext.DeploymentId, tableName);
            if (table == null) {
                throw new IllegalStateException("Table '" + tableName + "' has not be registered");
            }

            table.StatementContextCreateTable = statementContext;
            table.EventToPublic = eventToPublic;
            table.AggregationRowFactory = aggregationRowFactory;
            table.TableSerdes =
                statementContext.TableManagementService.GetTableSerdes(table, aggregationSerde, statementContext);
            table.PrimaryKeyGetter = primaryKeyGetter;
            table.TableReady();
        }
    }
} // end of namespace