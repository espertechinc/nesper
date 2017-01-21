///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.activator;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.core.start;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.named;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.view;
using com.espertech.esper.epl.virtualdw;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.core.context.factory
{
    public class StatementAgentInstanceFactoryCreateWindow : StatementAgentInstanceFactoryBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    
        private readonly StatementContext _statementContext;
        private readonly StatementSpecCompiled _statementSpec;
        private readonly EPServicesContext _services;
        private readonly ViewableActivator _activator;
        private readonly ViewFactoryChain _unmaterializedViewChain;
        private readonly ResultSetProcessorFactoryDesc _resultSetProcessorPrototype;
        private readonly OutputProcessViewFactory _outputProcessViewFactory;
        private readonly bool _isRecoveringStatement;
    
        public StatementAgentInstanceFactoryCreateWindow(StatementContext statementContext, StatementSpecCompiled statementSpec, EPServicesContext services, ViewableActivator activator, ViewFactoryChain unmaterializedViewChain, ResultSetProcessorFactoryDesc resultSetProcessorPrototype, OutputProcessViewFactory outputProcessViewFactory, bool recoveringStatement)
            : base(statementContext.Annotations)
        {
            _statementContext = statementContext;
            _statementSpec = statementSpec;
            _services = services;
            _activator = activator;
            _unmaterializedViewChain = unmaterializedViewChain;
            _resultSetProcessorPrototype = resultSetProcessorPrototype;
            _outputProcessViewFactory = outputProcessViewFactory;
            _isRecoveringStatement = recoveringStatement;
        }

        protected override StatementAgentInstanceFactoryResult NewContextInternal(AgentInstanceContext agentInstanceContext, bool isRecoveringResilient)
        {
            var stopCallbacks = new List<StopCallback>();
    
            String windowName = _statementSpec.CreateWindowDesc.WindowName;
            Viewable finalView;
            Viewable eventStreamParentViewable;
            StatementAgentInstancePostLoad postLoad;
            Viewable topView;
            NamedWindowProcessorInstance processorInstance;
            ViewableActivationResult viewableActivationResult;
    
            try {
                // Register interest
                viewableActivationResult = _activator.Activate(agentInstanceContext, false, isRecoveringResilient);
                stopCallbacks.Add(viewableActivationResult.StopCallback);
                eventStreamParentViewable = viewableActivationResult.Viewable;
    
                // Obtain processor for this named window
                var processor = _services.NamedWindowMgmtService.GetProcessor(windowName);
                if (processor == null) {
                    throw new Exception("Failed to obtain named window processor for named window '" + windowName + "'");
                }
    
                // Allocate processor instance
                processorInstance = processor.AddInstance(agentInstanceContext);
                var rootView = processorInstance.RootViewInstance;
                eventStreamParentViewable.AddView(rootView);
    
                // Materialize views
                var viewFactoryChainContext = new AgentInstanceViewFactoryChainContext(agentInstanceContext, true, null, null);
                var createResult = _services.ViewService.CreateViews(rootView, _unmaterializedViewChain.FactoryChain, viewFactoryChainContext, false);
                topView = createResult.TopViewable;
                finalView = createResult.FinalViewable;

                // add views to stop callback if applicable
                StatementAgentInstanceFactorySelect.AddViewStopCallback(stopCallbacks, createResult.NewViews);
    
                // If this is a virtual data window implementation, bind it to the context for easy lookup
                StopCallback envStopCallback = null;
                if (finalView is VirtualDWView) {
                    var objectName = "/virtualdw/" + windowName;
                    var virtualDWView = (VirtualDWView) finalView;
                    _services.EngineEnvContext.Bind(objectName, virtualDWView.VirtualDataWindow);
                    envStopCallback = new ProxyStopCallback(() =>
                    {
                        virtualDWView.Dispose();
                        _services.EngineEnvContext.Unbind(objectName);
                    });
                }
                StopCallback environmentStopCallback = envStopCallback;
    
                // Only if we are context-allocated: destroy the instance
                var contextName = processor.ContextName;
                var agentInstanceId = agentInstanceContext.AgentInstanceId;
                var allInOneStopMethod = new ProxyStopCallback(() =>
                {
                    var windowNameX = _statementSpec.CreateWindowDesc.WindowName;
                    var processorX = _services.NamedWindowMgmtService.GetProcessor(windowNameX);
                    if (processorX == null)
                    {
                        Log.Warn("Named window processor by name '" + windowNameX + "' has not been found");
                    }
                    else
                    {
                        NamedWindowProcessorInstance instance =
                            processorX.GetProcessorInstanceAllowUnpartitioned(agentInstanceId);
                        if (instance != null)
                        {
                            if (contextName != null)
                            {
                                instance.Dispose();
                            }
                            else
                            {
                                instance.Stop();
                            }
                        }
                    }
                    if (environmentStopCallback != null)
                    {
                        environmentStopCallback.Stop();
                    }
                });

                stopCallbacks.Add(allInOneStopMethod);
    
                // Attach tail view
                NamedWindowTailViewInstance tailView = processorInstance.TailViewInstance;
                finalView.AddView(tailView);
                finalView = tailView;
    
                // obtain result set processor
                ResultSetProcessor resultSetProcessor = EPStatementStartMethodHelperAssignExpr.GetAssignResultSetProcessor(agentInstanceContext, _resultSetProcessorPrototype, false, null, false);
    
                // Attach output view
                View outputView = _outputProcessViewFactory.MakeView(resultSetProcessor, agentInstanceContext);
                finalView.AddView(outputView);
                finalView = outputView;
    
                // obtain post load
                postLoad = processorInstance.PostLoad;
    
                // Handle insert case
                if (_statementSpec.CreateWindowDesc.IsInsert && !_isRecoveringStatement)
                {
                    String insertFromWindow = _statementSpec.CreateWindowDesc.InsertFromWindow;
                    NamedWindowProcessor namedWindowProcessor = _services.NamedWindowMgmtService.GetProcessor(insertFromWindow);
                    NamedWindowProcessorInstance sourceWindowInstances = namedWindowProcessor.GetProcessorInstance(agentInstanceContext);
                    IList<EventBean> events = new List<EventBean>();
                    if (_statementSpec.CreateWindowDesc.InsertFilter != null)
                    {
                        var eventsPerStream = new EventBean[1];
                        var filter = _statementSpec.CreateWindowDesc.InsertFilter.ExprEvaluator;
                        for (IEnumerator<EventBean> it = sourceWindowInstances.TailViewInstance.GetEnumerator(); it.MoveNext();)
                        {
                            var candidate = it.Current;
                            eventsPerStream[0] = candidate;
                            var result = filter.Evaluate(new EvaluateParams(eventsPerStream, true, agentInstanceContext));
                            if ((result == null) || (false.Equals(result)))
                            {
                                continue;
                            }
                            events.Add(candidate);
                        }
                    }
                    else
                    {
                        for (IEnumerator<EventBean> it = sourceWindowInstances.TailViewInstance.GetEnumerator(); it.MoveNext();)
                        {
                            events.Add(it.Current);
                        }
                    }
                    if (events.Count > 0)
                    {
                        EventType rootViewType = rootView.EventType;
                        EventBean[] convertedEvents = _services.EventAdapterService.TypeCast(events, rootViewType);
                        rootView.Update(convertedEvents, null);
                    }
                }
            }
            catch (Exception)
            {
                StopCallback callback = StatementAgentInstanceUtil.GetStopCallback(stopCallbacks, agentInstanceContext);
                StatementAgentInstanceUtil.StopSafe(callback, _statementContext);
                throw;
            }

            var createWindowResult = new StatementAgentInstanceFactoryCreateWindowResult(
                finalView, null, agentInstanceContext, eventStreamParentViewable, postLoad, topView, processorInstance, viewableActivationResult);
            if (_statementContext.StatementExtensionServicesContext != null)
            {
                _statementContext.StatementExtensionServicesContext.ContributeStopCallback(createWindowResult, stopCallbacks);
            }
    
            Log.Debug(".start Statement start completed");
            StopCallback stopCallback = StatementAgentInstanceUtil.GetStopCallback(stopCallbacks, agentInstanceContext);
            createWindowResult.StopCallback = stopCallback;

            return createWindowResult;
        }

        public override void AssignExpressions(StatementAgentInstanceFactoryResult result)
        {
        }

        public override void UnassignExpressions()
        {
        }
    }
}
