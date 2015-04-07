///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.core.context.activator;
using com.espertech.esper.core.context.factory;
using com.espertech.esper.core.context.mgr;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.named;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.view;
using com.espertech.esper.epl.virtualdw;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.core.start
{
    /// <summary>
    /// Starts and provides the stop method for EPL statements.
    /// </summary>
    public class EPStatementStartMethodCreateWindow : EPStatementStartMethodBase
    {
        public EPStatementStartMethodCreateWindow(StatementSpecCompiled statementSpec)
            : base(statementSpec)
        {
        }
    
        public override EPStatementStartResult StartInternal(EPServicesContext services, StatementContext statementContext, bool isNewStatement, bool isRecoveringStatement, bool isRecoveringResilient)
        {
    
            // define stop
            IList<StopCallback> stopCallbacks = new List<StopCallback>();
    
            // determine context
            var contextName = StatementSpec.OptionalContextName;
            var singleInstanceContext = contextName == null ? false : services.ContextManagementService.GetContextDescriptor(contextName).IsSingleInstanceContext;
    
            // register agent instance resources for use in HA
            var epStatementAgentInstanceHandle = GetDefaultAgentInstanceHandle(statementContext);
            if (services.SchedulableAgentInstanceDirectory != null) {
                services.SchedulableAgentInstanceDirectory.Add(epStatementAgentInstanceHandle);
            }
    
            // Create view factories and parent view based on a filter specification
            // Since only for non-joins we get the existing stream's lock and try to reuse it's views
            var filterStreamSpec = (FilterStreamSpecCompiled) StatementSpec.StreamSpecs[0];
            InstrumentationAgent instrumentationAgentCreateWindowInsert = null;
            if (InstrumentationHelper.ENABLED) {
                var eventTypeName = filterStreamSpec.FilterSpec.FilterForEventType.Name;
                instrumentationAgentCreateWindowInsert = new ProxyInstrumentationAgent() {
                    ProcIndicateQ = () =>  {
                        InstrumentationHelper.Get().QFilterActivationNamedWindowInsert(eventTypeName);
                    },
                    ProcIndicateA = () =>  {
                        InstrumentationHelper.Get().AFilterActivationNamedWindowInsert();
                    },
                };
            }
            var activator = new ViewableActivatorFilterProxy(services, filterStreamSpec.FilterSpec, statementContext.Annotations, false, instrumentationAgentCreateWindowInsert, false);
    
            // create data window view factories
            var unmaterializedViewChain = services.ViewService.CreateFactories(0, filterStreamSpec.FilterSpec.ResultEventType, filterStreamSpec.ViewSpecs, filterStreamSpec.Options, statementContext);
    
            // verify data window
            VerifyDataWindowViewFactoryChain(unmaterializedViewChain.FactoryChain);
    
            // get processor for variant-streams and versioned typed
            var windowName = StatementSpec.CreateWindowDesc.WindowName;
            var optionalRevisionProcessor = statementContext.ValueAddEventService.GetValueAddProcessor(windowName);
    
            // add named window processor (one per named window for all agent instances)
            var isPrioritized = services.EngineSettingsService.EngineSettings.ExecutionConfig.IsPrioritized;
            var isEnableSubqueryIndexShare = HintEnum.ENABLE_WINDOW_SUBQUERY_INDEXSHARE.GetHint(StatementSpec.Annotations) != null;
            if (!isEnableSubqueryIndexShare && unmaterializedViewChain.FactoryChain[0] is VirtualDWViewFactory) {
                isEnableSubqueryIndexShare = true;  // index share is always enabled for virtual data window (otherwise it wouldn't make sense)
            }
            var isBatchingDataWindow = DetermineBatchingDataWindow(unmaterializedViewChain.FactoryChain);
            var virtualDataWindowFactory = DetermineVirtualDataWindow(unmaterializedViewChain.FactoryChain);
            var optionalUniqueKeyProps = ViewServiceHelper.GetUniqueCandidateProperties(unmaterializedViewChain.FactoryChain, StatementSpec.Annotations);
            var processor = services.NamedWindowService.AddProcessor(windowName, contextName, singleInstanceContext, filterStreamSpec.FilterSpec.ResultEventType, statementContext.StatementResultService, optionalRevisionProcessor, statementContext.Expression, statementContext.StatementName, isPrioritized, isEnableSubqueryIndexShare, isBatchingDataWindow, virtualDataWindowFactory != null, statementContext.EpStatementHandle.MetricsHandle, optionalUniqueKeyProps,
                    StatementSpec.CreateWindowDesc.AsEventTypeName);
    
            Viewable finalViewable;
            EPStatementStopMethod stopStatementMethod;
            EPStatementDestroyMethod destroyStatementMethod;
    
            try {
                // add stop callback
                stopCallbacks.Add(() =>
                {
                    services.NamedWindowService.RemoveProcessor(windowName);
                    if (virtualDataWindowFactory != null) {
                        virtualDataWindowFactory.DestroyNamedWindow();
                    }
                });
    
                // Add a wildcard to the select clause as subscribers received the window contents
                StatementSpec.SelectClauseSpec.SetSelectExprList(new SelectClauseElementWildcard());
                StatementSpec.SelectStreamDirEnum = SelectClauseStreamSelectorEnum.RSTREAM_ISTREAM_BOTH;
    
                // obtain result set processor factory
                StreamTypeService typeService = new StreamTypeServiceImpl(new EventType[] {processor.NamedWindowType}, new String[] {windowName}, new bool[] {true}, services.EngineURI, false);
                var resultSetProcessorPrototype = ResultSetProcessorFactoryFactory.GetProcessorPrototype(
                        StatementSpec, statementContext, typeService, null, new bool[0], true, null, null, services.ConfigSnapshot);
    
                // obtain factory for output limiting
                var outputViewFactory = OutputProcessViewFactoryFactory.Make(StatementSpec, services.InternalEventRouter, statementContext, resultSetProcessorPrototype.ResultSetProcessorFactory.ResultEventType, null, services.TableService);
    
                // create context factory
                var contextFactory = new StatementAgentInstanceFactoryCreateWindow(statementContext, StatementSpec, services, activator, unmaterializedViewChain, resultSetProcessorPrototype, outputViewFactory, isRecoveringStatement);
    
                // With context - delegate instantiation to context
                EPStatementStopMethod stopMethod = new EPStatementStopMethodImpl(statementContext, stopCallbacks).Stop;
                if (StatementSpec.OptionalContextName != null)
                {
    
                    var mergeView = new ContextMergeView(processor.NamedWindowType);
                    finalViewable = mergeView;

                    var statement = new ContextManagedStatementCreateWindowDesc(StatementSpec, statementContext, mergeView, contextFactory);
                    services.ContextManagementService.AddStatement(contextName, statement, isRecoveringResilient);
                    stopStatementMethod = () =>
                    {
                        services.ContextManagementService.StoppedStatement(contextName, statementContext.StatementName, statementContext.StatementId);
                        stopMethod.Invoke();
                    };
    
                    destroyStatementMethod = () =>
                    {
                        services.ContextManagementService.DestroyedStatement(contextName, statementContext.StatementName, statementContext.StatementId);
                    };
                }
                // Without context - start here
                else {
                    var agentInstanceContext = GetDefaultAgentInstanceContext(statementContext);
                    StatementAgentInstanceFactoryCreateWindowResult resultOfStart;
                    try {
                        resultOfStart = (StatementAgentInstanceFactoryCreateWindowResult) contextFactory.NewContext(agentInstanceContext, false);
                    }
                    catch (Exception ex) {
                        services.NamedWindowService.RemoveProcessor(windowName);
                        throw;
                    }
                    finalViewable = resultOfStart.FinalView;
                    stopStatementMethod = () =>
                    {
                        resultOfStart.StopCallback.Invoke();
                        stopMethod.Invoke();
                    };
                    destroyStatementMethod = null;
    
                    if (statementContext.ExtensionServicesContext != null && statementContext.ExtensionServicesContext.StmtResources != null) {
                        statementContext.ExtensionServicesContext.StmtResources.StartContextPartition(resultOfStart, 0);
                    }
                }
            }
            catch (ExprValidationException) {
                services.NamedWindowService.RemoveProcessor(windowName);
                throw;
            }
            catch (Exception) {
                services.NamedWindowService.RemoveProcessor(windowName);
                throw;
            }
    
            return new EPStatementStartResult(finalViewable, stopStatementMethod, destroyStatementMethod);
        }
    
        private static VirtualDWViewFactory DetermineVirtualDataWindow(IList<ViewFactory> viewFactoryChain)
        {
            foreach (var viewFactory in viewFactoryChain) {
                if (viewFactory is VirtualDWViewFactory) {
                    return (VirtualDWViewFactory) viewFactory;
                }
            }
            return null;
        }

        private static bool DetermineBatchingDataWindow(IList<ViewFactory> viewFactoryChain)
        {
            foreach (var viewFactory in viewFactoryChain)
            {
                if (viewFactory is DataWindowBatchingViewFactory)
                {
                    return true;
                }
            }
            return false;
        }

        private void VerifyDataWindowViewFactoryChain(IList<ViewFactory> viewFactories)
        {
            foreach (var viewFactory in viewFactories)
            {
                if (viewFactory is DataWindowViewFactory)
                {
                    return;
                }
            }
            throw new ExprValidationException(NamedWindowServiceConstants.ERROR_MSG_DATAWINDOWS);
        }
    }
}
