///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.named;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.virtualdw;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.core.context.factory
{
    public class StatementAgentInstanceFactoryCreateIndex : StatementAgentInstanceFactory
    {
        private readonly EPServicesContext services;
        private readonly CreateIndexDesc spec;
        private readonly Viewable finalView;
        private readonly NamedWindowProcessor namedWindowProcessor;
        private readonly string tableName;
        private readonly string contextName;
    
        public StatementAgentInstanceFactoryCreateIndex(EPServicesContext services, CreateIndexDesc spec, Viewable finalView, NamedWindowProcessor namedWindowProcessor, string tableName, string contextName)
        {
            this.services = services;
            this.spec = spec;
            this.finalView = finalView;
            this.namedWindowProcessor = namedWindowProcessor;
            this.tableName = tableName;
            this.contextName = contextName;
        }
    
        public StatementAgentInstanceFactoryResult NewContext(AgentInstanceContext agentInstanceContext, bool isRecoveringResilient)
        {
            StopCallback stopCallback;

            int agentInstanceId = agentInstanceContext.AgentInstanceId;

            if (namedWindowProcessor != null) {
                // handle named window index
                NamedWindowProcessorInstance processorInstance = namedWindowProcessor.GetProcessorInstance(agentInstanceContext);
    
                if (namedWindowProcessor.IsVirtualDataWindow) {
                    VirtualDWView virtualDWView = processorInstance.RootViewInstance.VirtualDataWindow;
                    virtualDWView.HandleStartIndex(spec);
                    stopCallback = new ProxyStopCallback(() => virtualDWView.HandleStopIndex(spec));
                }
                else {
                    try {
                        processorInstance.RootViewInstance.AddExplicitIndex(spec.IsUnique, spec.IndexName, spec.Columns, isRecoveringResilient);
                    }
                    catch (ExprValidationException e) {
                        throw new EPException("Failed to create index: " + e.Message, e);
                    }
                    stopCallback = new ProxyStopCallback(() =>  {
                        // we remove the index when context partitioned.
                        // when not context partition the index gets removed when the last reference to the named window gets destroyed.
                        if (contextName != null)
                        {
                            var instance = namedWindowProcessor.GetProcessorInstance(agentInstanceId);
                            if (instance != null)
                            {
                                instance.RemoveExplicitIndex(spec.IndexName);
                            }
                        }

                    });
                }
            }
            else {
                // handle table access
                try {
                    TableStateInstance instance = services.TableService.GetState(tableName, agentInstanceContext.AgentInstanceId);
                    instance.AddExplicitIndex(spec, isRecoveringResilient, contextName != null);
                }
                catch (ExprValidationException ex) {
                    throw new EPException("Failed to create index: " + ex.Message, ex);
                }
                stopCallback = new ProxyStopCallback(() =>  {
                    // we remove the index when context partitioned.
                    // when not context partition the index gets removed when the last reference to the table gets destroyed.
                    if (contextName != null)
                    {
                        TableStateInstance instance = services.TableService.GetState(tableName, agentInstanceId);
                        if (instance != null)
                        {
                            instance.RemoveExplicitIndex(spec.IndexName);
                        }
                    }
                });
            }
    
            return new StatementAgentInstanceFactoryCreateIndexResult(finalView, stopCallback, agentInstanceContext);
        }

        public void AssignExpressions(StatementAgentInstanceFactoryResult result)
        {
        }

        public void UnassignExpressions()
        {
        }
    }
}
