///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.activator;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.airegistry;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.namedwindow.core;
using com.espertech.esper.common.@internal.epl.output.core;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.virtualdw;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.directory;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.context.aifactory.createwindow
{
    public class StatementAgentInstanceFactoryCreateNW
        : StatementAgentInstanceFactory,
            StatementReadyCallback
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private ViewableActivatorFilter activator;
        private EventType asEventType;
        private ExprEvaluator insertFromFilter;
        private NamedWindow insertFromNamedWindow;
        private string namedWindowName;
        private ResultSetProcessorFactoryProvider resultSetProcessorFactoryProvider;
        private ViewFactory[] viewFactories;

        public ViewableActivatorFilter Activator {
            set => activator = value;
        }

        public string NamedWindowName {
            set => namedWindowName = value;
        }

        public ViewFactory[] ViewFactories {
            set => viewFactories = value;
        }

        public NamedWindow InsertFromNamedWindow {
            set => insertFromNamedWindow = value;
        }

        public ExprEvaluator InsertFromFilter {
            set => insertFromFilter = value;
        }

        public EventType AsEventType {
            set => asEventType = value;
        }

        public ResultSetProcessorFactoryProvider ResultSetProcessorFactoryProvider {
            set => resultSetProcessorFactoryProvider = value;
        }

        public bool[] PriorFlagPerStream => null;

        public string AsEventTypeName => asEventType == null ? null : asEventType.Name;

        public EventType StatementEventType => activator.EventType;

        public void StatementCreate(StatementContext statementContext)
        {
            // The filter lookupables for the as-type apply to this type, when used with contexts, as contexts generated filters for types
            if (statementContext.ContextRuntimeDescriptor != null && asEventType != null) {
                var namedWindow = statementContext.NamedWindowManagementService.GetNamedWindow(
                    statementContext.DeploymentId,
                    namedWindowName);
                statementContext.FilterSharedLookupableRepository.ApplyLookupableFromType(
                    asEventType,
                    namedWindow.RootView.EventType,
                    statementContext.StatementId);
            }
        }

        public void StatementDestroy(StatementContext statementContext)
        {
            if (viewFactories[0] is VirtualDWViewFactory) {
                ((VirtualDWViewFactory) viewFactories[0]).Destroy();
            }

            statementContext.NamedWindowManagementService.DestroyNamedWindow(
                statementContext.DeploymentId,
                namedWindowName);
        }

        public void StatementDestroyPreconditions(StatementContext statementContext)
        {
        }

        public StatementAgentInstanceFactoryResult NewContext(
            AgentInstanceContext agentInstanceContext,
            bool isRecoveringResilient)
        {
            IList<AgentInstanceStopCallback> stopCallbacks = new List<AgentInstanceStopCallback>();

            //String windowName = statementSpec.getCreateWindowDesc().getWindowName();
            Viewable finalView;
            Viewable eventStreamParentViewable;
            Viewable topView;
            NamedWindowInstance namedWindowInstance;
            ViewableActivationResult viewableActivationResult;

            try {
                // Register interest
                viewableActivationResult = activator.Activate(agentInstanceContext, false, isRecoveringResilient);
                stopCallbacks.Add(viewableActivationResult.StopCallback);
                eventStreamParentViewable = viewableActivationResult.Viewable;

                // Obtain processor for this named window
                var namedWindow = agentInstanceContext.NamedWindowManagementService.GetNamedWindow(
                    agentInstanceContext.DeploymentId,
                    namedWindowName);
                if (namedWindow == null) {
                    throw new EPRuntimeException("Failed to obtain named window '" + namedWindowName + "'");
                }

                // Allocate processor instance
                namedWindowInstance = new NamedWindowInstance(namedWindow, agentInstanceContext);
                View rootView = namedWindowInstance.RootViewInstance;

                // Materialize views
                var viewFactoryChainContext =
                    new AgentInstanceViewFactoryChainContext(agentInstanceContext, true, null, null);
                var viewables = ViewFactoryUtil.Materialize(
                    viewFactories,
                    eventStreamParentViewable,
                    viewFactoryChainContext,
                    stopCallbacks);

                eventStreamParentViewable.Child = rootView;
                rootView.Parent = eventStreamParentViewable;
                topView = viewables.Top;
                rootView.Child = (View) topView;
                finalView = viewables.Last;

                // If this is a virtual data window implementation, bind it to the context for easy lookup
                AgentInstanceStopCallback envStopCallback = null;
                if (finalView is VirtualDWView) {
                    var objectName = "/virtualdw/" + namedWindowName;
                    var virtualDWView = (VirtualDWView) finalView;
                    try {
                        agentInstanceContext.RuntimeEnvContext.Bind(objectName, virtualDWView.VirtualDataWindow);
                    }
                    catch (NamingException e) {
                        throw new ViewProcessingException("Invalid name for adding to context:" + e.Message, e);
                    }

                    envStopCallback = new ProxyAgentInstanceStopCallback {
                        ProcStop = stopServices => {
                            try {
                                virtualDWView.Destroy();
                                stopServices.AgentInstanceContext.RuntimeEnvContext.Unbind(objectName);
                            }
                            catch (NamingException) {
                            }
                        }
                    };
                }

                var environmentStopCallback = envStopCallback;

                // destroy the instance
                AgentInstanceStopCallback allInOneStopMethod = new ProxyAgentInstanceStopCallback {
                    ProcStop = services => {
                        var instance = namedWindow.GetNamedWindowInstance(agentInstanceContext);
                        if (instance == null) {
                            Log.Warn("Named window processor by name '" + namedWindowName + "' has not been found");
                        }
                        else {
                            instance.Destroy();
                        }

                        if (environmentStopCallback != null) {
                            environmentStopCallback.Stop(services);
                        }
                    }
                };
                stopCallbacks.Add(allInOneStopMethod);

                // Attach tail view
                var tailView = namedWindowInstance.TailViewInstance;
                finalView.Child = tailView;
                tailView.Parent = finalView;
                finalView = tailView;

                // Attach output view
                var pair = StatementAgentInstanceFactoryUtil.StartResultSetAndAggregation(
                    resultSetProcessorFactoryProvider,
                    agentInstanceContext,
                    false,
                    null);
                var @out = new OutputProcessViewSimpleWProcessor(agentInstanceContext, pair.First);
                finalView.Child = @out;
                @out.Parent = finalView;
                finalView = @out;

                // Handle insert case
                if (insertFromNamedWindow != null && !isRecoveringResilient) {
                    HandleInsertFrom(agentInstanceContext, namedWindowInstance);
                }
            }
            catch (EPException) {
                throw;
            }
            catch (Exception ex) {
                var stopCallbackX = AgentInstanceUtil.FinalizeSafeStopCallbacks(stopCallbacks);
                AgentInstanceUtil.StopSafe(stopCallbackX, agentInstanceContext);
                throw new EPException(ex.Message, ex);
            }

            var stopCallback = AgentInstanceUtil.FinalizeSafeStopCallbacks(stopCallbacks);
            return new StatementAgentInstanceFactoryCreateNWResult(
                finalView,
                stopCallback,
                agentInstanceContext,
                eventStreamParentViewable,
                topView,
                namedWindowInstance,
                viewableActivationResult);
        }

        public StatementAgentInstanceLock ObtainAgentInstanceLock(
            StatementContext statementContext,
            int agentInstanceId)
        {
            return AgentInstanceUtil.NewLock(statementContext);
        }

        public AIRegistryRequirements RegistryRequirements => AIRegistryRequirements.NoRequirements();

        public void Ready(
            StatementContext statementContext,
            ModuleIncidentals moduleIncidentals,
            bool recovery)
        {
            var namedWindow = statementContext.NamedWindowManagementService.GetNamedWindow(
                statementContext.DeploymentId,
                namedWindowName);
            namedWindow.StatementContext = statementContext;
        }

        private void HandleInsertFrom(
            AgentInstanceContext agentInstanceContext,
            NamedWindowInstance processorInstance)
        {
            var sourceWindowInstances = insertFromNamedWindow.GetNamedWindowInstance(agentInstanceContext);
            IList<EventBean> events = new List<EventBean>();
            if (insertFromFilter != null) {
                var eventsPerStream = new EventBean[1];
                foreach (var candidate in sourceWindowInstances.TailViewInstance) {
                    eventsPerStream[0] = candidate;
                    var result = insertFromFilter.Evaluate(eventsPerStream, true, agentInstanceContext);
                    if (result == null || false.Equals(result)) {
                        continue;
                    }

                    events.Add(candidate);
                }
            }
            else {
                foreach (var eventBean in sourceWindowInstances.TailViewInstance) {
                    events.Add(eventBean);
                }
            }

            if (events.Count > 0) {
                var rootViewType = processorInstance.RootViewInstance.EventType;
                var convertedEvents = EventTypeUtility.TypeCast(
                    events,
                    rootViewType,
                    agentInstanceContext.EventBeanTypedEventFactory,
                    agentInstanceContext.EventTypeAvroHandler);
                processorInstance.RootViewInstance.Update(convertedEvents, null);
            }
        }
    }
} // end of namespace