///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.airegistry;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.@join.lookup;
using com.espertech.esper.common.@internal.epl.@join.queryplan;
using com.espertech.esper.common.@internal.epl.namedwindow.core;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.epl.table.update;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.common.@internal.context.aifactory.createindex
{
    public class StatementAgentInstanceFactoryCreateIndex : StatementAgentInstanceFactory
    {
        private QueryPlanIndexItem explicitIndexDesc;
        private string indexModuleName;
        private IndexMultiKey indexMultiKey;
        private string indexName;
        private NamedWindow namedWindow;
        private Table table;

        private Viewable viewable;

        public EventType EventType {
            set {
                StatementEventType = value;
                viewable = new ViewableDefaultImpl(value);
            }
        }

        public string IndexName {
            set => indexName = value;
        }

        public string IndexModuleName {
            set => indexModuleName = value;
        }

        public QueryPlanIndexItem ExplicitIndexDesc {
            set => explicitIndexDesc = value;
        }

        public IndexMultiKey IndexMultiKey {
            set => indexMultiKey = value;
        }

        public NamedWindow NamedWindow {
            set => namedWindow = value;
        }

        public Table Table {
            set => table = value;
        }

        public EventType StatementEventType { get; private set; }

        public void StatementCreate(StatementContext statementContext)
        {
            if (table != null && indexMultiKey.IsUnique) {
                foreach (var callback in table.UpdateStrategyCallbacks) {
                    if (callback.IsMerge) {
                        TableUpdateStrategyFactory.ValidateNewUniqueIndex(
                            callback.TableUpdatedProperties,
                            indexMultiKey.HashIndexedProps);
                    }
                }
            }

            try {
                if (namedWindow != null) {
                    namedWindow.ValidateAddIndex(
                        statementContext.DeploymentId,
                        statementContext.StatementName,
                        indexName,
                        indexModuleName,
                        explicitIndexDesc,
                        indexMultiKey);
                }
                else {
                    table.ValidateAddIndex(
                        statementContext.DeploymentId,
                        statementContext.StatementName,
                        indexName,
                        indexModuleName,
                        explicitIndexDesc,
                        indexMultiKey);
                }
            }
            catch (ExprValidationException ex) {
                throw new EPException(ex.Message, ex);
            }
        }

        public void StatementDestroy(StatementContext statementContext)
        {
            if (namedWindow != null) {
                namedWindow.RemoveIndexReferencesStmtMayRemoveIndex(
                    indexMultiKey,
                    statementContext.DeploymentId,
                    statementContext.StatementName);
            }
            else {
                table.RemoveIndexReferencesStmtMayRemoveIndex(
                    indexMultiKey,
                    statementContext.DeploymentId,
                    statementContext.StatementName);
            }
        }

        public void StatementDestroyPreconditions(StatementContext statementContext)
        {
        }

        public StatementAgentInstanceFactoryResult NewContext(
            AgentInstanceContext agentInstanceContext,
            bool isRecoveringResilient)
        {
            AgentInstanceStopCallback stopCallback;

            if (namedWindow != null) {
                // handle named window index
                var processorInstance = namedWindow.GetNamedWindowInstance(agentInstanceContext);

                if (processorInstance.RootViewInstance.IsVirtualDataWindow) {
                    var virtualDWView = processorInstance.RootViewInstance.VirtualDataWindow;
                    virtualDWView.HandleStartIndex(indexName, explicitIndexDesc);
                    stopCallback = new ProxyAgentInstanceStopCallback {
                        ProcStop = services => { virtualDWView.HandleStopIndex(indexName, explicitIndexDesc); }
                    };
                }
                else {
                    try {
                        processorInstance.RootViewInstance.AddExplicitIndex(
                            indexName,
                            indexModuleName,
                            explicitIndexDesc,
                            isRecoveringResilient);
                    }
                    catch (ExprValidationException e) {
                        throw new EPException("Failed to create index: " + e.Message, e);
                    }

                    stopCallback = new ProxyAgentInstanceStopCallback {
                        ProcStop = services => {
                            var instance = namedWindow.GetNamedWindowInstance(services.AgentInstanceContext);
                            if (instance != null) {
                                instance.RemoveExplicitIndex(indexName);
                            }
                        }
                    };
                }
            }
            else {
                // handle table access
                try {
                    var instance = table.GetTableInstance(agentInstanceContext.AgentInstanceId);
                    instance.AddExplicitIndex(indexName, indexModuleName, explicitIndexDesc, isRecoveringResilient);
                }
                catch (ExprValidationException ex) {
                    throw new EPException("Failed to create index: " + ex.Message, ex);
                }

                stopCallback = new ProxyAgentInstanceStopCallback {
                    ProcStop = services => {
                        var instance = table.GetTableInstance(services.AgentInstanceContext.AgentInstanceId);
                        if (instance != null) {
                            instance.RemoveExplicitIndex(indexName);
                        }
                    }
                };
            }

            return new StatementAgentInstanceFactoryCreateIndexResult(viewable, stopCallback, agentInstanceContext);
        }

        public AIRegistryRequirements RegistryRequirements => AIRegistryRequirements.NoRequirements();

        public StatementAgentInstanceLock ObtainAgentInstanceLock(
            StatementContext statementContext,
            int agentInstanceId)
        {
            return AgentInstanceUtil.NewLock(statementContext);
        }
    }
} // end of namespace