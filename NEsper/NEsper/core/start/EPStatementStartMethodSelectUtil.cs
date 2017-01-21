///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.client.hook;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.activator;
using com.espertech.esper.core.context.factory;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.annotation;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.db;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.@join.@base;
using com.espertech.esper.epl.named;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.util;
using com.espertech.esper.epl.view;
using com.espertech.esper.filter;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.core.start
{
	/// <summary>
	/// Starts and provides the stop method for EPL statements.
	/// </summary>
	public class EPStatementStartMethodSelectUtil
	{
	    public static EPStatementStartMethodSelectDesc Prepare(
	        StatementSpecCompiled statementSpec,
	        EPServicesContext services,
	        StatementContext statementContext,
	        bool recoveringResilient,
	        AgentInstanceContext defaultAgentInstanceContext,
	        bool queryPlanLogging,
	        ViewableActivatorFactory optionalViewableActivatorFactory,
	        OutputProcessViewCallback optionalOutputProcessViewCallback,
	        SelectExprProcessorDeliveryCallback selectExprProcessorDeliveryCallback)
        {
	        // define stop and destroy
	        var stopCallbacks = new List<StopCallback>();
	        var destroyCallbacks = new EPStatementDestroyCallbackList();

	        // determine context
	        var contextName = statementSpec.OptionalContextName;
	        var contextPropertyRegistry = (contextName != null) ? services.ContextManagementService.GetContextDescriptor(contextName).ContextPropertyRegistry : null;

	        // Determine stream names for each stream - some streams may not have a name given
	        var streamNames = EPStatementStartMethodHelperUtil.DetermineStreamNames(statementSpec.StreamSpecs);
	        var numStreams = streamNames.Length;
	        if (numStreams == 0) {
	            throw new ExprValidationException("The from-clause is required but has not been specified");
	        }
	        var isJoin = statementSpec.StreamSpecs.Length > 1;
	        var hasContext = statementSpec.OptionalContextName != null;

	        // First we create streams for subselects, if there are any
	        var subSelectStreamDesc = EPStatementStartMethodHelperSubselect.CreateSubSelectActivation(services, statementSpec, statementContext, destroyCallbacks);

	        // Create streams and views
	        var eventStreamParentViewableActivators = new ViewableActivator[numStreams];
	        var unmaterializedViewChain = new ViewFactoryChain[numStreams];
	        var eventTypeNames = new string[numStreams];
	        var isNamedWindow = new bool[numStreams];
	        var historicalEventViewables = new HistoricalEventViewable[numStreams];

	        // verify for joins that required views are present
	        var joinAnalysisResult = VerifyJoinViews(statementSpec, statementContext.NamedWindowMgmtService, defaultAgentInstanceContext);
	        var evaluatorContextStmt = new ExprEvaluatorContextStatement(statementContext, false);

	        for (var i = 0; i < statementSpec.StreamSpecs.Length; i++)
	        {
	            var streamSpec = statementSpec.StreamSpecs[i];

	            var isCanIterateUnbound = streamSpec.ViewSpecs.Length == 0 &&
	                    (services.ConfigSnapshot.EngineDefaults.ViewResourcesConfig.IsIterableUnbound ||
	                            AnnotationUtil.FindAttribute(statementSpec.Annotations, typeof(IterableUnboundAttribute)) != null);

	            // Create view factories and parent view based on a filter specification
	            if (streamSpec is FilterStreamSpecCompiled)
	            {
	                var filterStreamSpec = (FilterStreamSpecCompiled) streamSpec;
	                eventTypeNames[i] = filterStreamSpec.FilterSpec.FilterForEventTypeName;

	                // Since only for non-joins we get the existing stream's lock and try to reuse it's views
	                var filterSubselectSameStream = EPStatementStartMethodHelperUtil.DetermineSubquerySameStream(statementSpec, filterStreamSpec);

	                // create activator
	                ViewableActivator activatorDeactivator;
	                if (optionalViewableActivatorFactory != null) {
	                    activatorDeactivator = optionalViewableActivatorFactory.CreateActivatorSimple(filterStreamSpec);
	                    if (activatorDeactivator == null) {
	                        throw new IllegalStateException("Viewable activate is null for " + filterStreamSpec.FilterSpec.FilterForEventType.Name);
	                    }
	                }
	                else {
	                    if (!hasContext) {
	                        activatorDeactivator = services.ViewableActivatorFactory.CreateStreamReuseView(services, statementContext, statementSpec, filterStreamSpec, isJoin, evaluatorContextStmt, filterSubselectSameStream, i, isCanIterateUnbound);
	                    }
	                    else {
	                        InstrumentationAgent instrumentationAgentFilter = null;
	                        if (InstrumentationHelper.ENABLED) {
	                            var eventTypeName = filterStreamSpec.FilterSpec.FilterForEventType.Name;
	                            var streamNumber = i;
	                            instrumentationAgentFilter = new ProxyInstrumentationAgent() {
	                                ProcIndicateQ = () =>  {
	                                    InstrumentationHelper.Get().QFilterActivationStream(eventTypeName, streamNumber);
	                                },
	                                ProcIndicateA = () =>  {
	                                    InstrumentationHelper.Get().AFilterActivationStream();
	                                },
	                            };
	                        }

	                        activatorDeactivator = services.ViewableActivatorFactory.CreateFilterProxy(services, filterStreamSpec.FilterSpec, statementSpec.Annotations, false, instrumentationAgentFilter, isCanIterateUnbound, i);
	                    }
	                }
	                eventStreamParentViewableActivators[i] = activatorDeactivator;

	                var resultEventType = filterStreamSpec.FilterSpec.ResultEventType;
	                unmaterializedViewChain[i] = services.ViewService.CreateFactories(i, resultEventType, streamSpec.ViewSpecs, streamSpec.Options, statementContext, false, -1);
	            }
	            // Create view factories and parent view based on a pattern expression
	            else if (streamSpec is PatternStreamSpecCompiled)
	            {
	                var patternStreamSpec = (PatternStreamSpecCompiled) streamSpec;
	                var usedByChildViews = streamSpec.ViewSpecs.Length > 0 || (statementSpec.InsertIntoDesc != null);
	                var patternTypeName = statementContext.StatementId + "_pattern_" + i;
	                var eventType = services.EventAdapterService.CreateSemiAnonymousMapType(patternTypeName, patternStreamSpec.TaggedEventTypes, patternStreamSpec.ArrayEventTypes, usedByChildViews);
	                unmaterializedViewChain[i] = services.ViewService.CreateFactories(i, eventType, streamSpec.ViewSpecs, streamSpec.Options, statementContext, false, -1);

	                var rootFactoryNode = services.PatternNodeFactory.MakeRootNode(patternStreamSpec.EvalFactoryNode);
	                var patternContext = statementContext.PatternContextFactory.CreateContext(statementContext, i, rootFactoryNode, patternStreamSpec.MatchedEventMapMeta, true);

	                // create activator
	                var patternActivator = services.ViewableActivatorFactory.CreatePattern(patternContext, rootFactoryNode, eventType, EPStatementStartMethodHelperUtil.IsConsumingFilters(patternStreamSpec.EvalFactoryNode), patternStreamSpec.IsSuppressSameEventMatches, patternStreamSpec.IsDiscardPartialsOnMatch, isCanIterateUnbound);
	                eventStreamParentViewableActivators[i] = patternActivator;
	            }
	            // Create view factories and parent view based on a database SQL statement
	            else if (streamSpec is DBStatementStreamSpec)
	            {
	                ValidateNoViews(streamSpec, "Historical data");
	                var sqlStreamSpec = (DBStatementStreamSpec) streamSpec;
	                var typeConversionHook = (SQLColumnTypeConversion) TypeHelper.GetAnnotationHook(statementSpec.Annotations, HookType.SQLCOL, typeof(SQLColumnTypeConversion), statementContext.EngineImportService);
	                var outputRowConversionHook = (SQLOutputRowConversion) TypeHelper.GetAnnotationHook(statementSpec.Annotations, HookType.SQLROW, typeof(SQLOutputRowConversion), statementContext.EngineImportService);
	                var epStatementAgentInstanceHandle = defaultAgentInstanceContext.EpStatementAgentInstanceHandle;
	                var historicalEventViewable = DatabasePollingViewableFactory.CreateDBStatementView(
                        statementContext.StatementId, i, sqlStreamSpec, 
                        services.DatabaseRefService, 
                        services.EventAdapterService,
                        epStatementAgentInstanceHandle, 
                        statementContext.Annotations,
                        typeConversionHook, 
                        outputRowConversionHook,
	                    statementContext.ConfigSnapshot.EngineDefaults.LoggingConfig.IsEnableADO, 
                        services.DataCacheFactory, 
                        statementContext);
	                historicalEventViewables[i] = historicalEventViewable;
	                unmaterializedViewChain[i] = ViewFactoryChain.FromTypeNoViews(historicalEventViewable.EventType);
	                eventStreamParentViewableActivators[i] = services.ViewableActivatorFactory.MakeHistorical(historicalEventViewable);
	                stopCallbacks.Add(historicalEventViewable);
	            }
	            else if (streamSpec is MethodStreamSpec)
	            {
	                ValidateNoViews(streamSpec, "Method data");
	                var methodStreamSpec = (MethodStreamSpec) streamSpec;
	                var epStatementAgentInstanceHandle = defaultAgentInstanceContext.EpStatementAgentInstanceHandle;
	                var historicalEventViewable = MethodPollingViewableFactory.CreatePollMethodView(
	                    i, methodStreamSpec, services.EventAdapterService, epStatementAgentInstanceHandle, services.EngineImportService,
	                    statementContext.SchedulingService, statementContext.ScheduleBucket, evaluatorContextStmt,
	                    statementContext.VariableService, statementContext.ContextName, services.DataCacheFactory,
	                    statementContext);
	                historicalEventViewables[i] = historicalEventViewable;
	                unmaterializedViewChain[i] = ViewFactoryChain.FromTypeNoViews(historicalEventViewable.EventType);
	                eventStreamParentViewableActivators[i] = services.ViewableActivatorFactory.MakeHistorical(historicalEventViewable);
	                stopCallbacks.Add(historicalEventViewable);
	            }
	            else if (streamSpec is TableQueryStreamSpec)
	            {
	                ValidateNoViews(streamSpec, "Table data");
	                var tableStreamSpec = (TableQueryStreamSpec) streamSpec;
	                if (isJoin && tableStreamSpec.FilterExpressions.Count > 0) {
	                    throw new ExprValidationException("Joins with tables do not allow table filter expressions, please add table filters to the where-clause instead");
	                }
	                var metadata = services.TableService.GetTableMetadata(tableStreamSpec.TableName);
	                ExprEvaluator[] tableFilterEvals = null;
	                if (tableStreamSpec.FilterExpressions.Count > 0) {
	                    tableFilterEvals = ExprNodeUtility.GetEvaluators(tableStreamSpec.FilterExpressions);
	                }
	                EPLValidationUtil.ValidateContextName(true, metadata.TableName, metadata.ContextName, statementSpec.OptionalContextName, false);
	                eventStreamParentViewableActivators[i] = services.ViewableActivatorFactory.CreateTable(metadata, tableFilterEvals);
	                unmaterializedViewChain[i] = ViewFactoryChain.FromTypeNoViews(metadata.InternalEventType);
	                eventTypeNames[i] = tableStreamSpec.TableName;
	                joinAnalysisResult.SetTablesForStream(i, metadata);
	                if (tableStreamSpec.Options.IsUnidirectional) {
	                    throw new ExprValidationException("Tables cannot be marked as unidirectional");
	                }
	                if (tableStreamSpec.Options.IsRetainIntersection || tableStreamSpec.Options.IsRetainUnion) {
	                    throw new ExprValidationException("Tables cannot be marked with retain");
	                }
	                if (isJoin) {
	                    destroyCallbacks.AddCallback(new EPStatementDestroyCallbackTableIdxRef(services.TableService, metadata, statementContext.StatementName));
	                }
	                services.StatementVariableRefService.AddReferences(statementContext.StatementName, metadata.TableName);
	            }
	            else if (streamSpec is NamedWindowConsumerStreamSpec)
	            {
	                var namedSpec = (NamedWindowConsumerStreamSpec) streamSpec;
	                var processor = services.NamedWindowMgmtService.GetProcessor(namedSpec.WindowName);
	                var namedWindowType = processor.TailView.EventType;
	                if (namedSpec.OptPropertyEvaluator != null) {
	                    namedWindowType = namedSpec.OptPropertyEvaluator.FragmentEventType;
	                }

	                eventStreamParentViewableActivators[i] = services.ViewableActivatorFactory.CreateNamedWindow(processor, namedSpec, statementContext);
	                services.NamedWindowConsumerMgmtService.AddConsumer(statementContext, namedSpec);
	                unmaterializedViewChain[i] = services.ViewService.CreateFactories(i, namedWindowType, namedSpec.ViewSpecs, namedSpec.Options, statementContext, false, -1);
	                joinAnalysisResult.SetNamedWindow(i);
	                eventTypeNames[i] = namedSpec.WindowName;
	                isNamedWindow[i] = true;

	                // Consumers to named windows cannot declare a data window view onto the named window to avoid duplicate remove streams
	                EPStatementStartMethodHelperValidate.ValidateNoDataWindowOnNamedWindow(unmaterializedViewChain[i].FactoryChain);
	            }
	            else
	            {
	                throw new ExprValidationException("Unknown stream specification type: " + streamSpec);
	            }
	        }

	        // handle match-recognize pattern
	        if (statementSpec.MatchRecognizeSpec != null)
	        {
	            if (isJoin) {
	                throw new ExprValidationException("Joins are not allowed when using match-recognize");
	            }
	            if (joinAnalysisResult.TablesPerStream[0] != null) {
	                throw new ExprValidationException("Tables cannot be used with match-recognize");
	            }
	            var isUnbound = (unmaterializedViewChain[0].FactoryChain.IsEmpty()) && (!(statementSpec.StreamSpecs[0] is NamedWindowConsumerStreamSpec));
	            var factoryX = services.RegexHandlerFactory.MakeViewFactory(unmaterializedViewChain[0], statementSpec.MatchRecognizeSpec, defaultAgentInstanceContext, isUnbound, statementSpec.Annotations, services.ConfigSnapshot.EngineDefaults.MatchRecognizeConfig);
	            unmaterializedViewChain[0].FactoryChain.Add(factoryX);

	            EPStatementStartMethodHelperAssignExpr.AssignAggregations(factoryX.AggregationService, factoryX.AggregationExpressions);
	        }

	        // Obtain event types from view factory chains
	        var streamEventTypes = new EventType[statementSpec.StreamSpecs.Length];
	        for (var i = 0; i < unmaterializedViewChain.Length; i++)
	        {
	            streamEventTypes[i] = unmaterializedViewChain[i].EventType;
	        }

	        // Add uniqueness information useful for joins
	        joinAnalysisResult.AddUniquenessInfo(unmaterializedViewChain, statementSpec.Annotations);

	        // Validate sub-select views
	        var subSelectStrategyCollection = EPStatementStartMethodHelperSubselect.PlanSubSelect(services, statementContext, queryPlanLogging, subSelectStreamDesc, streamNames, streamEventTypes, eventTypeNames, statementSpec.DeclaredExpressions, contextPropertyRegistry);

	        // Construct type information per stream
	        StreamTypeService typeService = new StreamTypeServiceImpl(streamEventTypes, streamNames, EPStatementStartMethodHelperUtil.GetHasIStreamOnly(isNamedWindow, unmaterializedViewChain), services.EngineURI, false);
	        var viewResourceDelegateUnverified = new ViewResourceDelegateUnverified();

	        // Validate views that require validation, specifically streams that don't have
	        // sub-views such as DB SQL joins
	        var historicalViewableDesc = new HistoricalViewableDesc(numStreams);
	        for (var stream = 0; stream < historicalEventViewables.Length; stream++)
	        {
	            var historicalEventViewable = historicalEventViewables[stream];
	            if (historicalEventViewable == null) {
	                continue;
	            }
	            historicalEventViewable.Validate(
                    services.EngineImportService,
	                typeService,
	                statementContext.TimeProvider,
	                statementContext.VariableService, statementContext.TableService,
                    statementContext.ScriptingService, evaluatorContextStmt,
	                services.ConfigSnapshot, services.SchedulingService, services.EngineURI,
	                statementSpec.SqlParameters,
	                statementContext.EventAdapterService, statementContext);
	            historicalViewableDesc.SetHistorical(stream, historicalEventViewable.RequiredStreams);
	            if (historicalEventViewable.RequiredStreams.Contains(stream))
	            {
	                throw new ExprValidationException("Parameters for historical stream " + stream + " indicate that the stream is subordinate to itself as stream parameters originate in the same stream");
	            }
	        }

	        // unidirectional is not supported with into-table
	        if (joinAnalysisResult.IsUnidirectional && statementSpec.IntoTableSpec != null) {
	            throw new ExprValidationException("Into-table does not allow unidirectional joins");
	        }

	        // Construct a processor for results posted by views and joins, which takes care of aggregation if required.
	        // May return null if we don't need to post-process results posted by views or joins.
	        var resultSetProcessorPrototypeDesc = ResultSetProcessorFactoryFactory.GetProcessorPrototype(
	                statementSpec, statementContext, typeService, viewResourceDelegateUnverified, joinAnalysisResult.UnidirectionalInd, true, contextPropertyRegistry, selectExprProcessorDeliveryCallback, services.ConfigSnapshot, services.ResultSetProcessorHelperFactory, false, false);

	        // Validate where-clause filter tree, outer join clause and output limit expression
	        EPStatementStartMethodHelperValidate.ValidateNodes(statementSpec, statementContext, typeService, viewResourceDelegateUnverified);

	        // Handle 'prior' function nodes in terms of view requirements
	        var viewResourceDelegateVerified = EPStatementStartMethodHelperViewResources.VerifyPreviousAndPriorRequirements(unmaterializedViewChain, viewResourceDelegateUnverified);

	        // handle join
	        JoinSetComposerPrototype joinSetComposerPrototype = null;
	        if (numStreams > 1) {
	            var selectsRemoveStream =
                    statementSpec.SelectStreamSelectorEnum.IsSelectsRStream() ||
	                statementSpec.OutputLimitSpec != null;
	            var hasAggregations = !resultSetProcessorPrototypeDesc.AggregationServiceFactoryDesc.Expressions.IsEmpty();
	            joinSetComposerPrototype = JoinSetComposerPrototypeFactory.MakeComposerPrototype(
	                    statementContext.StatementName, statementContext.StatementId,
	                    statementSpec.OuterJoinDescList, statementSpec.FilterRootNode, typeService.EventTypes, streamNames,
	                    joinAnalysisResult, queryPlanLogging, statementContext, historicalViewableDesc, defaultAgentInstanceContext,
	                    selectsRemoveStream, hasAggregations, services.TableService, false, services.EventTableIndexService.AllowInitIndex(recoveringResilient));
	        }

	        // obtain factory for output limiting
	        var outputViewFactory = OutputProcessViewFactoryFactory.Make(statementSpec, services.InternalEventRouter, statementContext, resultSetProcessorPrototypeDesc.ResultSetProcessorFactory.ResultEventType, optionalOutputProcessViewCallback, services.TableService, resultSetProcessorPrototypeDesc.ResultSetProcessorFactory.ResultSetProcessorType, services.ResultSetProcessorHelperFactory, services.StatementVariableRefService);

	        // Factory for statement-context instances
	        var factory = new StatementAgentInstanceFactorySelect(
	                numStreams, eventStreamParentViewableActivators,
	                statementContext, statementSpec, services,
	                typeService, unmaterializedViewChain, resultSetProcessorPrototypeDesc, joinAnalysisResult, recoveringResilient,
	                joinSetComposerPrototype, subSelectStrategyCollection, viewResourceDelegateVerified, outputViewFactory);

	        EPStatementStopMethod stopMethod = new EPStatementStopMethodImpl(statementContext, stopCallbacks);
	        return new EPStatementStartMethodSelectDesc(factory, subSelectStrategyCollection, viewResourceDelegateUnverified, resultSetProcessorPrototypeDesc, stopMethod, destroyCallbacks);
	    }

	    private static void ValidateNoViews(StreamSpecCompiled streamSpec, string conceptName)
	    {
	        if (streamSpec.ViewSpecs.Length > 0) {
	            throw new ExprValidationException(conceptName + " joins do not allow views onto the data, view '"
	                    + streamSpec.ViewSpecs[0].ObjectNamespace + ':' + streamSpec.ViewSpecs[0].ObjectName + "' is not valid in this context");
	        }
	    }

	    private static StreamJoinAnalysisResult VerifyJoinViews(StatementSpecCompiled statementSpec, NamedWindowMgmtService namedWindowMgmtService, AgentInstanceContext defaultAgentInstanceContext)
	    {
	        var streamSpecs = statementSpec.StreamSpecs;
	        var analysisResult = new StreamJoinAnalysisResult(streamSpecs.Length);
	        if (streamSpecs.Length < 2)
	        {
	            return analysisResult;
	        }

	        // Determine if any stream has a unidirectional keyword

	        // inspect unidirectional indicator and named window flags
	        var unidirectionalStreamNumber = -1;
	        for (var i = 0; i < statementSpec.StreamSpecs.Length; i++)
	        {
	            var streamSpec = statementSpec.StreamSpecs[i];
	            if (streamSpec.Options.IsUnidirectional)
	            {
	                analysisResult.SetUnidirectionalInd(i);
	                if (unidirectionalStreamNumber != -1)
	                {
	                    throw new ExprValidationException("The unidirectional keyword can only apply to one stream in a join");
	                }
	                unidirectionalStreamNumber = i;
	            }
	            if (streamSpec.ViewSpecs.Length > 0)
	            {
	                analysisResult.SetHasChildViews(i);
	            }
	            if (streamSpec is NamedWindowConsumerStreamSpec)
	            {
	                var nwSpec = (NamedWindowConsumerStreamSpec) streamSpec;
	                if (nwSpec.OptPropertyEvaluator != null && !streamSpec.Options.IsUnidirectional) {
	                    throw new ExprValidationException("Failed to validate named window use in join, contained-event is only allowed for named windows when marked as unidirectional");
	                }
	                analysisResult.SetNamedWindow(i);
	                var processor = namedWindowMgmtService.GetProcessor(nwSpec.WindowName);
	                string[][] uniqueIndexes = processor.UniqueIndexes;
	                analysisResult.UniqueKeys[i] = uniqueIndexes;
	                if (processor.IsVirtualDataWindow) {
	                    analysisResult.ViewExternal[i] = agentInstanceContext => processor.GetProcessorInstance(agentInstanceContext).RootViewInstance.VirtualDataWindow;
	                }
	            }
	        }
	        if ((unidirectionalStreamNumber != -1) && (analysisResult.HasChildViews[unidirectionalStreamNumber]))
	        {
	            throw new ExprValidationException("The unidirectional keyword requires that no views are declared onto the stream");
	        }
	        analysisResult.UnidirectionalStreamNumber = unidirectionalStreamNumber;

	        // count streams that provide data, excluding streams that poll data (DB and method)
	        var countProviderNonpolling = 0;
	        for (var i = 0; i < statementSpec.StreamSpecs.Length; i++)
	        {
	            var streamSpec = statementSpec.StreamSpecs[i];
	            if ((streamSpec is MethodStreamSpec) ||
	                (streamSpec is DBStatementStreamSpec) ||
	                (streamSpec is TableQueryStreamSpec))
	            {
	                continue;
	            }
	            countProviderNonpolling++;
	        }

	        // if there is only one stream providing data, the analysis is done
	        if (countProviderNonpolling == 1)
	        {
	            return analysisResult;
	        }
	        // there are multiple driving streams, verify the presence of a view for insert/remove stream

	        // validation of join views works differently for unidirectional as there can be self-joins that don't require a view
	        // see if this is a self-join in which all streams are filters and filter specification is the same.
	        FilterSpecCompiled unidirectionalFilterSpec = null;
	        FilterSpecCompiled lastFilterSpec = null;
	        var pureSelfJoin = true;
	        foreach (var streamSpec in statementSpec.StreamSpecs)
	        {
	            if (!(streamSpec is FilterStreamSpecCompiled))
	            {
	                pureSelfJoin = false;
	                continue;
	            }

	            var filterSpec = ((FilterStreamSpecCompiled) streamSpec).FilterSpec;
	            if ((lastFilterSpec != null) && (!lastFilterSpec.EqualsTypeAndFilter(filterSpec)))
	            {
	                pureSelfJoin = false;
	            }
	            if (streamSpec.ViewSpecs.Length > 0)
	            {
	                pureSelfJoin = false;
	            }
	            lastFilterSpec = filterSpec;

	            if (streamSpec.Options.IsUnidirectional)
	            {
	                unidirectionalFilterSpec = filterSpec;
	            }
	        }

	        // self-join without views and not unidirectional
	        if ((pureSelfJoin) && (unidirectionalFilterSpec == null))
	        {
	            analysisResult.IsPureSelfJoin = true;
	            return analysisResult;
	        }

	        // weed out filter and pattern streams that don't have a view in a join
	        for (var i = 0; i < statementSpec.StreamSpecs.Length; i++)
	        {
	            var streamSpec = statementSpec.StreamSpecs[i];
	            if (streamSpec.ViewSpecs.Length > 0)
	            {
	                continue;
	            }

	            var name = streamSpec.OptionalStreamName;
	            if ((name == null) && (streamSpec is FilterStreamSpecCompiled))
	            {
	                name = ((FilterStreamSpecCompiled) streamSpec).FilterSpec.FilterForEventTypeName;
	            }
	            if ((name == null) && (streamSpec is PatternStreamSpecCompiled))
	            {
	                name = "pattern event stream";
	            }

	            if (streamSpec.Options.IsUnidirectional)
	            {
	                continue;
	            }
	            // allow a self-join without a child view, in that the filter spec is the same as the unidirection's stream filter
	            if ((unidirectionalFilterSpec != null) &&
	                (streamSpec is FilterStreamSpecCompiled) &&
	                (((FilterStreamSpecCompiled) streamSpec).FilterSpec.EqualsTypeAndFilter(unidirectionalFilterSpec)))
	            {
	                analysisResult.SetUnidirectionalNonDriving(i);
	                continue;
	            }
	            if ((streamSpec is FilterStreamSpecCompiled) ||
	                (streamSpec is PatternStreamSpecCompiled))
	            {
	                throw new ExprValidationException("Joins require that at least one view is specified for each stream, no view was specified for " + name);
	            }
	        }

	        return analysisResult;
	    }
	}
} // end of namespace
