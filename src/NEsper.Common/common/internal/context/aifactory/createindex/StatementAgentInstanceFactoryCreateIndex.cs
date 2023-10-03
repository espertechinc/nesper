///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.airegistry;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.join.lookup;
using com.espertech.esper.common.@internal.epl.join.queryplan;
using com.espertech.esper.common.@internal.epl.namedwindow.core;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.epl.table.update;
using com.espertech.esper.common.@internal.epl.virtualdw;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.threading.locks;


namespace com.espertech.esper.common.@internal.context.aifactory.createindex
{
    public class StatementAgentInstanceFactoryCreateIndex : StatementAgentInstanceFactory
    {
        private EventType eventType;
        private NamedWindow namedWindow;
        private Table table;
        private Viewable viewable;

        public void StatementCreate(StatementContext value)
        {
            if (table != null && IndexMultiKey.IsUnique) {
                foreach (var callback in table.UpdateStrategyCallbacks) {
                    if (callback.IsMerge) {
                        TableUpdateStrategyFactory.ValidateNewUniqueIndex(
                            callback.TableUpdatedProperties,
                            IndexMultiKey.HashIndexedProps);
                    }
                }
            }

            try {
                if (namedWindow != null) {
                    namedWindow.ValidateAddIndex(
                        value.DeploymentId,
                        value.StatementName,
                        IndexName,
                        IndexModuleName,
                        ExplicitIndexDesc,
                        IndexMultiKey);
                }
                else {
                    table.ValidateAddIndex(
                        value.DeploymentId,
                        value.StatementName,
                        IndexName,
                        IndexModuleName,
                        ExplicitIndexDesc,
                        IndexMultiKey);
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
                    IndexMultiKey,
                    statementContext.DeploymentId,
                    statementContext.StatementName);
            }
            else {
                table.RemoveIndexReferencesStmtMayRemoveIndex(
                    IndexMultiKey,
                    statementContext.DeploymentId,
                    statementContext.StatementName);
            }
        }

        public StatementAgentInstanceFactoryResult NewContext(
            AgentInstanceContext agentInstanceContext,
            bool isRecoveringResilient)
        {
            AgentInstanceMgmtCallback stopCallback;
            if (namedWindow != null) {
                // handle named window index
                var processorInstance = namedWindow.GetNamedWindowInstance(agentInstanceContext);
                if (processorInstance.RootViewInstance.IsVirtualDataWindow) {
                    var virtualDWView = processorInstance.RootViewInstance.VirtualDataWindow;
                    virtualDWView.HandleStartIndex(IndexName, ExplicitIndexDesc);
                    stopCallback = new ProxyAgentInstanceMgmtCallback() {
                        ProcStop = (services) => { virtualDWView.HandleStopIndex(IndexName, ExplicitIndexDesc); }
                    };
                }
                else {
                    try {
                        processorInstance.RootViewInstance.AddExplicitIndex(
                            IndexName,
                            IndexModuleName,
                            ExplicitIndexDesc,
                            isRecoveringResilient);
                    }
                    catch (ExprValidationException e) {
                        throw new EPException("Failed to create index: " + e.Message, e);
                    }

                    stopCallback = new ProxyAgentInstanceMgmtCallback() {
                        ProcStop = (services) => {
                            var instance = namedWindow.GetNamedWindowInstance(services.AgentInstanceContext);
                            if (instance != null) {
                                instance.RemoveExplicitIndex(IndexName, IndexModuleName);
                            }
                        }
                    };
                }
            }
            else {
                // handle table access
                try {
                    var instance = table.GetTableInstance(agentInstanceContext.AgentInstanceId);
                    instance.AddExplicitIndex(IndexName, IndexModuleName, ExplicitIndexDesc, isRecoveringResilient);
                }
                catch (ExprValidationException ex) {
                    throw new EPException("Failed to create index: " + ex.Message, ex);
                }

                stopCallback = new ProxyAgentInstanceMgmtCallback() {
                    ProcStop = (services) => {
                        var instance = table.GetTableInstance(services.AgentInstanceContext.AgentInstanceId);
                        if (instance != null) {
                            instance.RemoveExplicitIndex(IndexName, IndexModuleName);
                        }
                    }
                };
            }

            return new StatementAgentInstanceFactoryCreateIndexResult(viewable, stopCallback, agentInstanceContext);
        }

        public IReaderWriterLock ObtainAgentInstanceLock(
            StatementContext statementContext,
            int agentInstanceId)
        {
            return AgentInstanceUtil.NewLock(statementContext, agentInstanceId);
        }

        public EventType EventType {
            set {
                eventType = value;
                viewable = new ViewableDefaultImpl(value);
            }
        }

        public string IndexName { get; set; }

        public string IndexModuleName { get; set; }

        public QueryPlanIndexItem ExplicitIndexDesc { get; set; }

        public IndexMultiKey IndexMultiKey { get; set; }

        public NamedWindow NamedWindow {
            set => namedWindow = value;
        }

        public Table Table {
            set => table = value;
        }

        public EventType StatementEventType => eventType;

        public AIRegistryRequirements RegistryRequirements => AIRegistryRequirements.NoRequirements();
    }
} // end of namespace