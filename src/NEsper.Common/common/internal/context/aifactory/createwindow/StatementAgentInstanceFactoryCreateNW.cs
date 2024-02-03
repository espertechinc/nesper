///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.common.@internal.context.aifactory.createwindow
{
    public class StatementAgentInstanceFactoryCreateNW
        : StatementAgentInstanceFactory,
            StatementReadyCallback
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private ViewableActivatorFilter _activator;
        private EventType _asEventType;
        private ExprEvaluator _insertFromFilter;
        private NamedWindow _insertFromNamedWindow;
        private string _namedWindowName;
        private ResultSetProcessorFactoryProvider _resultSetProcessorFactoryProvider;
        private ViewFactory[] _viewFactories;

        public ViewableActivatorFilter Activator {
            set => _activator = value;
        }

        public string NamedWindowName {
            set => _namedWindowName = value;
        }

        public ViewFactory[] ViewFactories {
            set => _viewFactories = value;
        }

        public NamedWindow InsertFromNamedWindow {
            set => _insertFromNamedWindow = value;
        }

        public ExprEvaluator InsertFromFilter {
            set => _insertFromFilter = value;
        }

        public EventType AsEventType {
            set => _asEventType = value;
        }

        public ResultSetProcessorFactoryProvider ResultSetProcessorFactoryProvider {
            set => _resultSetProcessorFactoryProvider = value;
        }

        public bool[] PriorFlagPerStream => null;

        public string AsEventTypeName => _asEventType?.Name;

        public EventType StatementEventType => _activator.EventType;

        public void StatementCreate(StatementContext value)
        {
            // The filter lookupables for the as-type apply to this type, when used with contexts, as contexts generated filters for types
            if (value.ContextRuntimeDescriptor != null && _asEventType != null) {
                var namedWindow = value.NamedWindowManagementService.GetNamedWindow(
                    value.DeploymentId,
                    _namedWindowName);
                value.FilterSharedLookupableRepository.ApplyLookupableFromType(
                    _asEventType,
                    namedWindow.RootView.EventType,
                    value.StatementId);
            }
        }

        public void StatementDestroy(StatementContext statementContext)
        {
            (_viewFactories[0] as VirtualDWViewFactory)?.Destroy();

            statementContext.NamedWindowManagementService.DestroyNamedWindow(
                statementContext.DeploymentId,
                _namedWindowName);
        }

        public void StatementDestroyPreconditions(StatementContext statementContext)
        {
        }

        public StatementAgentInstanceFactoryResult NewContext(
            AgentInstanceContext agentInstanceContext,
            bool isRecoveringResilient)
        {
            IList<AgentInstanceMgmtCallback> stopCallbacks = new List<AgentInstanceMgmtCallback>();

            //String windowName = statementSpec.getCreateWindowDesc().getWindowName();
            Viewable finalView;
            Viewable eventStreamParentViewable;
            Viewable topView;
            NamedWindowInstance namedWindowInstance;
            ViewableActivationResult viewableActivationResult;

            try {
                // Register interest
                viewableActivationResult = _activator.Activate(agentInstanceContext, false, isRecoveringResilient);
                stopCallbacks.Add(viewableActivationResult.StopCallback);
                eventStreamParentViewable = viewableActivationResult.Viewable;

                // Obtain processor for this named window
                var namedWindow = agentInstanceContext.NamedWindowManagementService.GetNamedWindow(
                    agentInstanceContext.DeploymentId,
                    _namedWindowName);
                if (namedWindow == null) {
                    throw new EPRuntimeException("Failed to obtain named window '" + _namedWindowName + "'");
                }

                // Allocate processor instance
                namedWindowInstance = new NamedWindowInstance(namedWindow, agentInstanceContext);
                View rootView = namedWindowInstance.RootViewInstance;

                // Materialize views
                var viewFactoryChainContext =
                    new AgentInstanceViewFactoryChainContext(agentInstanceContext, true, null, null, null);
                var viewables = ViewFactoryUtil.Materialize(
                    _viewFactories,
                    eventStreamParentViewable,
                    viewFactoryChainContext,
                    stopCallbacks);

                eventStreamParentViewable.Child = rootView;
                rootView.Parent = eventStreamParentViewable;
                topView = viewables.Top;
                rootView.Child = (View)topView;
                finalView = viewables.Last;

                // If this is a virtual data window implementation, bind it to the context for easy lookup
                AgentInstanceMgmtCallback envStopCallback = null;
                if (finalView is VirtualDWView virtualDwView) {
                    var objectName = "/virtualdw/" + _namedWindowName;
                    try {
                        agentInstanceContext.RuntimeEnvContext.Bind(objectName, virtualDwView.VirtualDataWindow);
                    }
                    catch (NamingException e) {
                        throw new ViewProcessingException("Invalid name for adding to context:" + e.Message, e);
                    }

                    envStopCallback = new CreateNWVirtualDWMgmtCallback(virtualDwView, objectName);
                }

                // destroy the instance
                AgentInstanceMgmtCallback allInOneStopMethod =
                    new CreateNWAllInOneMgmtCallback(namedWindow, envStopCallback);
                stopCallbacks.Add(allInOneStopMethod);

                // Attach tail view
                var tailView = namedWindowInstance.TailViewInstance;
                finalView.Child = tailView;
                tailView.Parent = finalView;
                finalView = tailView;

                // Attach output view
                var pair = StatementAgentInstanceFactoryUtil.StartResultSetAndAggregation(
                    _resultSetProcessorFactoryProvider,
                    agentInstanceContext,
                    false,
                    null);
                var @out = new OutputProcessViewSimpleWProcessor(agentInstanceContext, pair.First);
                finalView.Child = @out;
                @out.Parent = finalView;
                finalView = @out;

                // Handle insert case
                if (_insertFromNamedWindow != null && !isRecoveringResilient) {
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
            return new StatementAgentInstanceFactoryCreateNwResult(
                finalView,
                stopCallback,
                agentInstanceContext,
                eventStreamParentViewable,
                topView,
                namedWindowInstance,
                viewableActivationResult);
        }

        public IReaderWriterLock ObtainAgentInstanceLock(
            StatementContext statementContext,
            int agentInstanceId)
        {
            return AgentInstanceUtil.NewLock(statementContext, agentInstanceId);
        }

        public AIRegistryRequirements RegistryRequirements => AIRegistryRequirements.NoRequirements();

        public void Ready(
            StatementContext statementContext,
            ModuleIncidentals moduleIncidentals,
            bool recovery)
        {
            var namedWindow = statementContext.NamedWindowManagementService.GetNamedWindow(
                statementContext.DeploymentId,
                _namedWindowName);
            namedWindow.StatementContext = statementContext;
        }

        private void HandleInsertFrom(
            AgentInstanceContext agentInstanceContext,
            NamedWindowInstance processorInstance)
        {
            var sourceWindowInstances = _insertFromNamedWindow.GetNamedWindowInstance(agentInstanceContext);
            IList<EventBean> events = new List<EventBean>();
            if (_insertFromFilter != null) {
                var eventsPerStream = new EventBean[1];
                foreach (var candidate in sourceWindowInstances.TailViewInstance) {
                    eventsPerStream[0] = candidate;
                    var result = _insertFromFilter.Evaluate(eventsPerStream, true, agentInstanceContext);
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

        public class CreateNWVirtualDWMgmtCallback : AgentInstanceMgmtCallback
        {
            private readonly VirtualDWView _virtualDwView;
            private readonly string _objectName;

            public CreateNWVirtualDWMgmtCallback(
                VirtualDWView virtualDWView,
                string objectName)
            {
                _virtualDwView = virtualDWView;
                _objectName = objectName;
            }

            public void Stop(AgentInstanceStopServices stopServices)
            {
                try {
                    _virtualDwView.Destroy();
                    stopServices.AgentInstanceContext.RuntimeEnvContext.Unbind(_objectName);
                }
                catch (NamingException) {
                }
            }

            public void Transfer(AgentInstanceTransferServices services)
            {
            }
        }

        public class CreateNWAllInOneMgmtCallback : AgentInstanceMgmtCallback
        {
            private readonly NamedWindow _namedWindow;
            private readonly AgentInstanceMgmtCallback _optionalEnvStopCallback;

            public CreateNWAllInOneMgmtCallback(
                NamedWindow namedWindow,
                AgentInstanceMgmtCallback optionalEnvStopCallback)
            {
                _namedWindow = namedWindow;
                _optionalEnvStopCallback = optionalEnvStopCallback;
            }

            public void Stop(AgentInstanceStopServices services)
            {
                var instance = _namedWindow.GetNamedWindowInstance(services.AgentInstanceContext);
                if (instance == null) {
                    Log.Warn("Named window processor by name '" + _namedWindow.Name + "' has not been found");
                }
                else {
                    instance.Destroy();
                }

                _optionalEnvStopCallback?.Stop(services);
            }

            public void Transfer(AgentInstanceTransferServices services)
            {
                // no action required
            }
        }
    }
} // end of namespace