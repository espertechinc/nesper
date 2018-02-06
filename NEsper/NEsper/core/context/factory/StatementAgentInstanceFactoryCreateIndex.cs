///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.@join.plan;
using com.espertech.esper.epl.named;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.virtualdw;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.core.context.factory
{
    /// <summary>
    /// Defines the <see cref="StatementAgentInstanceFactoryCreateIndex" />
    /// </summary>
    public class StatementAgentInstanceFactoryCreateIndex : StatementAgentInstanceFactory
    {
        /// <summary>
        /// Defines the services
        /// </summary>
        private readonly EPServicesContext _services;

        /// <summary>
        /// Defines the spec
        /// </summary>
        private readonly CreateIndexDesc _spec;

        /// <summary>
        /// Defines the finalView
        /// </summary>
        private readonly Viewable _finalView;

        /// <summary>
        /// Defines the namedWindowProcessor
        /// </summary>
        private readonly NamedWindowProcessor _namedWindowProcessor;

        /// <summary>
        /// Defines the tableName
        /// </summary>
        private readonly string _tableName;

        /// <summary>
        /// Defines the contextName
        /// </summary>
        private readonly string _contextName;

        /// <summary>
        /// Defines the explicitIndexDesc
        /// </summary>
        private readonly QueryPlanIndexItem _explicitIndexDesc;

        /// <summary>
        /// Initializes a new instance of the <see cref="StatementAgentInstanceFactoryCreateIndex"/> class.
        /// </summary>
        /// <param name="services">The <see cref="EPServicesContext"/></param>
        /// <param name="spec">The <see cref="CreateIndexDesc"/></param>
        /// <param name="finalView">The <see cref="Viewable"/></param>
        /// <param name="namedWindowProcessor">The <see cref="NamedWindowProcessor"/></param>
        /// <param name="tableName">The <see cref="string"/></param>
        /// <param name="contextName">The <see cref="string"/></param>
        /// <param name="explicitIndexDesc">The <see cref="QueryPlanIndexItem"/></param>
        public StatementAgentInstanceFactoryCreateIndex(
            EPServicesContext services,
            CreateIndexDesc spec,
            Viewable finalView,
            NamedWindowProcessor namedWindowProcessor,
            string tableName,
            string contextName,
            QueryPlanIndexItem explicitIndexDesc)
        {
            _services = services;
            _spec = spec;
            _finalView = finalView;
            _namedWindowProcessor = namedWindowProcessor;
            _tableName = tableName;
            _contextName = contextName;
            _explicitIndexDesc = explicitIndexDesc;
        }

        /// <summary>
        /// The NewContext
        /// </summary>
        /// <param name="agentInstanceContext">The <see cref="AgentInstanceContext"/></param>
        /// <param name="isRecoveringResilient">The <see cref="bool"/></param>
        /// <returns>The <see cref="StatementAgentInstanceFactoryResult"/></returns>
        public StatementAgentInstanceFactoryResult NewContext(AgentInstanceContext agentInstanceContext, bool isRecoveringResilient)
        {
            StopCallback stopCallback;

            int agentInstanceId = agentInstanceContext.AgentInstanceId;

            if (_namedWindowProcessor != null)
            {
                // handle named window index
                NamedWindowProcessorInstance processorInstance = _namedWindowProcessor.GetProcessorInstance(agentInstanceContext);

                if (_namedWindowProcessor.IsVirtualDataWindow)
                {
                    VirtualDWView virtualDWView = processorInstance.RootViewInstance.VirtualDataWindow;
                    virtualDWView.HandleStartIndex(_spec);
                    stopCallback = new ProxyStopCallback(() => virtualDWView.HandleStopIndex(_spec));
                }
                else
                {
                    try
                    {
                        processorInstance.RootViewInstance.AddExplicitIndex(_spec.IndexName, _explicitIndexDesc, isRecoveringResilient);
                    }
                    catch (ExprValidationException e)
                    {
                        throw new EPException("Failed to create index: " + e.Message, e);
                    }
                    stopCallback = new ProxyStopCallback(() =>
                    {
                        // we remove the index when context partitioned.
                        // when not context partition the index gets removed when the last reference to the named window gets destroyed.
                        if (_contextName != null)
                        {
                            var instance = _namedWindowProcessor.GetProcessorInstance(agentInstanceId);
                            if (instance != null)
                            {
                                instance.RemoveExplicitIndex(_spec.IndexName);
                            }
                        }

                    });
                }
            }
            else
            {
                // handle table access
                try
                {
                    TableStateInstance instance = _services.TableService.GetState(_tableName, agentInstanceContext.AgentInstanceId);
                    instance.AddExplicitIndex(_spec.IndexName, _explicitIndexDesc, isRecoveringResilient, _contextName != null);
                }
                catch (ExprValidationException ex)
                {
                    throw new EPException("Failed to create index: " + ex.Message, ex);
                }
                stopCallback = new ProxyStopCallback(() =>
                {
                    // we remove the index when context partitioned.
                    // when not context partition the index gets removed when the last reference to the table gets destroyed.
                    if (_contextName != null)
                    {
                        TableStateInstance instance = _services.TableService.GetState(_tableName, agentInstanceId);
                        if (instance != null)
                        {
                            instance.RemoveExplicitIndex(_spec.IndexName);
                        }
                    }
                });
            }

            return new StatementAgentInstanceFactoryCreateIndexResult(_finalView, stopCallback, agentInstanceContext);
        }

        /// <summary>
        /// The AssignExpressions
        /// </summary>
        /// <param name="result">The <see cref="StatementAgentInstanceFactoryResult"/></param>
        public void AssignExpressions(StatementAgentInstanceFactoryResult result)
        {
        }

        /// <summary>
        /// The UnassignExpressions
        /// </summary>
        public void UnassignExpressions()
        {
        }
    }
}
