///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.core.context.factory;
using com.espertech.esper.core.context.mgr;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
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

        public override EPStatementStartResult StartInternal(
            EPServicesContext services,
            StatementContext statementContext,
            bool isNewStatement,
            bool isRecoveringStatement,
            bool isRecoveringResilient)
        {
            // define stop
            var stopCallbacks = new List<StopCallback>();

            // determine context
            var contextName = _statementSpec.OptionalContextName;

            // Create view factories and parent view based on a filter specification
            // Since only for non-joins we get the existing stream's lock and try to reuse it's views
            var filterStreamSpec = (FilterStreamSpecCompiled)_statementSpec.StreamSpecs[0];
            InstrumentationAgent instrumentationAgentCreateWindowInsert = null;
            if (InstrumentationHelper.ENABLED)
            {
                var eventTypeName = filterStreamSpec.FilterSpec.FilterForEventType.Name;
                instrumentationAgentCreateWindowInsert = new ProxyInstrumentationAgent()
                {
                    ProcIndicateQ = () => InstrumentationHelper.Get().QFilterActivationNamedWindowInsert(eventTypeName),
                    ProcIndicateA = () => InstrumentationHelper.Get().AFilterActivationNamedWindowInsert(),
                };
            }
            var activator = services.ViewableActivatorFactory.CreateFilterProxy(services, filterStreamSpec.FilterSpec, statementContext.Annotations, false, instrumentationAgentCreateWindowInsert, false, 0);

            // create data window view factories
            var unmaterializedViewChain = services.ViewService.CreateFactories(0, filterStreamSpec.FilterSpec.ResultEventType, filterStreamSpec.ViewSpecs, filterStreamSpec.Options, statementContext, false, -1);

            // verify data window
            VerifyDataWindowViewFactoryChain(unmaterializedViewChain.FactoryChain);

            // get processor for variant-streams and versioned typed
            var windowName = _statementSpec.CreateWindowDesc.WindowName;
            var optionalRevisionProcessor = statementContext.ValueAddEventService.GetValueAddProcessor(windowName);

            // add named window processor (one per named window for all agent instances)
            var isPrioritized = services.EngineSettingsService.EngineSettings.Execution.IsPrioritized;
            var isEnableSubqueryIndexShare = HintEnum.ENABLE_WINDOW_SUBQUERY_INDEXSHARE.GetHint(_statementSpec.Annotations) != null;
            if (!isEnableSubqueryIndexShare && unmaterializedViewChain.FactoryChain[0] is VirtualDWViewFactory)
            {
                isEnableSubqueryIndexShare = true;  // index share is always enabled for virtual data window (otherwise it wouldn't make sense)
            }
            var isBatchingDataWindow = DetermineBatchingDataWindow(unmaterializedViewChain.FactoryChain);
            var virtualDataWindowFactory = DetermineVirtualDataWindow(unmaterializedViewChain.FactoryChain);
            var optionalUniqueKeyProps = ViewServiceHelper.GetUniqueCandidateProperties(unmaterializedViewChain.FactoryChain, _statementSpec.Annotations);
            var processor = services.NamedWindowMgmtService.AddProcessor(
                windowName, contextName, filterStreamSpec.FilterSpec.ResultEventType,
                statementContext.StatementResultService, optionalRevisionProcessor, statementContext.Expression,
                statementContext.StatementName, isPrioritized, isEnableSubqueryIndexShare, isBatchingDataWindow,
                virtualDataWindowFactory != null, optionalUniqueKeyProps,
                _statementSpec.CreateWindowDesc.AsEventTypeName,
                statementContext, services.NamedWindowDispatchService,
                services.LockManager);

            Viewable finalViewable;
            EPStatementStopMethod stopStatementMethod;
            EPStatementDestroyMethod destroyStatementMethod;

            try
            {
                // add stop callback
                stopCallbacks.Add(new ProxyStopCallback(() =>
                {
                    services.NamedWindowMgmtService.RemoveProcessor(windowName);
                    if (virtualDataWindowFactory != null)
                    {
                        virtualDataWindowFactory.DestroyNamedWindow();
                    }
                }));

                // Add a wildcard to the select clause as subscribers received the window contents
                _statementSpec.SelectClauseSpec.SetSelectExprList(new SelectClauseElementWildcard());
                _statementSpec.SelectStreamDirEnum = SelectClauseStreamSelectorEnum.RSTREAM_ISTREAM_BOTH;

                // obtain result set processor factory
                StreamTypeService typeService = new StreamTypeServiceImpl(new EventType[] { processor.NamedWindowType }, new string[] { windowName }, new bool[] { true }, services.EngineURI, false);
                var resultSetProcessorPrototype = ResultSetProcessorFactoryFactory.GetProcessorPrototype(
                        _statementSpec, statementContext, typeService, null, new bool[0], true, null, null, services.ConfigSnapshot, services.ResultSetProcessorHelperFactory, false, false);

                // obtain factory for output limiting
                var outputViewFactory = OutputProcessViewFactoryFactory.Make(
                    _statementSpec,
                    services.InternalEventRouter,
                    statementContext,
                    resultSetProcessorPrototype.ResultSetProcessorFactory.ResultEventType, null,
                    services.TableService,
                    resultSetProcessorPrototype.ResultSetProcessorFactory.ResultSetProcessorType,
                    services.ResultSetProcessorHelperFactory,
                    services.StatementVariableRefService);

                // create context factory
                // Factory for statement-context instances
                var contextFactory = new StatementAgentInstanceFactoryCreateWindow(statementContext, _statementSpec, services, activator, unmaterializedViewChain, resultSetProcessorPrototype, outputViewFactory, isRecoveringStatement);
                statementContext.StatementAgentInstanceFactory = contextFactory;

                // With context - delegate instantiation to context
                EPStatementStopMethod stopMethod = new EPStatementStopMethodImpl(statementContext, stopCallbacks);
                if (_statementSpec.OptionalContextName != null)
                {

                    var mergeView = new ContextMergeView(processor.NamedWindowType);
                    finalViewable = mergeView;

                    var statement = new ContextManagedStatementCreateWindowDesc(_statementSpec, statementContext, mergeView, contextFactory);
                    services.ContextManagementService.AddStatement(contextName, statement, isRecoveringResilient);
                    stopStatementMethod = new ProxyEPStatementStopMethod(() =>
                    {
                        services.ContextManagementService.StoppedStatement(contextName, statementContext.StatementName, statementContext.StatementId, statementContext.Expression, statementContext.ExceptionHandlingService);
                        stopMethod.Stop();
                    });

                    destroyStatementMethod = new ProxyEPStatementDestroyMethod(() =>
                        services.ContextManagementService.DestroyedStatement(contextName, statementContext.StatementName, statementContext.StatementId));
                }
                // Without context - start here
                else
                {
                    var agentInstanceContext = GetDefaultAgentInstanceContext(statementContext);
                    StatementAgentInstanceFactoryCreateWindowResult resultOfStart;
                    try
                    {
                        resultOfStart = (StatementAgentInstanceFactoryCreateWindowResult)contextFactory.NewContext(agentInstanceContext, isRecoveringResilient);
                    }
                    catch (Exception)
                    {
                        services.NamedWindowMgmtService.RemoveProcessor(windowName);
                        throw;
                    }
                    finalViewable = resultOfStart.FinalView;
                    var stopCallback = services.EpStatementFactory.MakeStopMethod(resultOfStart);
                    stopStatementMethod = new ProxyEPStatementStopMethod(() =>
                    {
                        stopCallback.Stop();
                        stopMethod.Stop();
                    });
                    destroyStatementMethod = null;

                    if (statementContext.StatementExtensionServicesContext != null && statementContext.StatementExtensionServicesContext.StmtResources != null)
                    {
                        var holder = statementContext.StatementExtensionServicesContext.ExtractStatementResourceHolder(resultOfStart);
                        statementContext.StatementExtensionServicesContext.StmtResources.Unpartitioned = holder;
                        statementContext.StatementExtensionServicesContext.PostProcessStart(resultOfStart, isRecoveringResilient);
                    }
                }
            }
            catch (ExprValidationException)
            {
                services.NamedWindowMgmtService.RemoveProcessor(windowName);
                throw;
            }
            catch (Exception)
            {
                services.NamedWindowMgmtService.RemoveProcessor(windowName);
                throw;
            }

            services.StatementVariableRefService.AddReferences(statementContext.StatementName, windowName);

            return new EPStatementStartResult(finalViewable, stopStatementMethod, destroyStatementMethod);
        }

        private static VirtualDWViewFactory DetermineVirtualDataWindow(IEnumerable<ViewFactory> viewFactoryChain)
        {
            return viewFactoryChain.OfType<VirtualDWViewFactory>().FirstOrDefault();
        }

        private static bool DetermineBatchingDataWindow(IEnumerable<ViewFactory> viewFactoryChain)
        {
            return viewFactoryChain.OfType<DataWindowBatchingViewFactory>().Any();
        }

        private void VerifyDataWindowViewFactoryChain(IEnumerable<ViewFactory> viewFactories)
        {
            if (viewFactories.OfType<DataWindowViewFactory>().Any())
            {
                return;
            }

            throw new ExprValidationException(NamedWindowMgmtServiceConstants.ERROR_MSG_DATAWINDOWS);
        }
    }
} // end of namespace
