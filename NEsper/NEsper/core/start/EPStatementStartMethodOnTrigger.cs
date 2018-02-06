///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.soda;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.activator;
using com.espertech.esper.core.context.factory;
using com.espertech.esper.core.context.mgr;
using com.espertech.esper.core.context.subselect;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.prev;
using com.espertech.esper.epl.expression.prior;
using com.espertech.esper.epl.expression.subquery;
using com.espertech.esper.epl.expression.table;
using com.espertech.esper.epl.named;
using com.espertech.esper.epl.property;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.table.onaction;
using com.espertech.esper.epl.variable;
using com.espertech.esper.epl.view;
using com.espertech.esper.events;
using com.espertech.esper.events.map;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.core.start
{
    /// <summary>Starts and provides the stop method for EPL statements.</summary>
    public class EPStatementStartMethodOnTrigger : EPStatementStartMethodBase
    {
        public static readonly string INITIAL_VALUE_STREAM_NAME = "initial";

        public EPStatementStartMethodOnTrigger(StatementSpecCompiled statementSpec)
            : base(statementSpec)
        {
        }

        private static EPStatementStartMethodOnTriggerItem OnSplitValidate(
            StatementSpecCompiled statementSpec,
            StreamTypeService typeServiceTrigger,
            ContextPropertyRegistry contextPropertyRegistry,
            EPServicesContext services,
            StatementContext statementContext,
            PropertyEvaluator optionalPropertyEvaluator)
        {
            var isNamedWindowInsert =
                statementContext.NamedWindowMgmtService.IsNamedWindow(statementSpec.InsertIntoDesc.EventTypeName);
            EPStatementStartMethodHelperValidate.ValidateNodes(
                statementSpec, statementContext, typeServiceTrigger, null);
            var factoryDescs = ResultSetProcessorFactoryFactory.GetProcessorPrototype(
                statementSpec, statementContext, typeServiceTrigger, null, new bool[0], false, contextPropertyRegistry,
                null, services.ConfigSnapshot, services.ResultSetProcessorHelperFactory, false, true);
            return new EPStatementStartMethodOnTriggerItem(
                statementSpec.FilterRootNode, isNamedWindowInsert,
                GetOptionalInsertIntoTableName(statementSpec.InsertIntoDesc, services.TableService), factoryDescs,
                optionalPropertyEvaluator);
        }

        private static string GetOptionalInsertIntoTableName(InsertIntoDesc insertIntoDesc, TableService tableService)
        {
            if (insertIntoDesc == null)
            {
                return null;
            }
            var tableMetadata = tableService.GetTableMetadata(insertIntoDesc.EventTypeName);
            if (tableMetadata != null)
            {
                return tableMetadata.TableName;
            }
            return null;
        }

        public override EPStatementStartResult StartInternal(
            EPServicesContext services,
            StatementContext statementContext,
            bool isNewStatement,
            bool isRecoveringStatement,
            bool isRecoveringResilient)
        {
            // define stop and destroy
            var stopCallbacks = new List<StopCallback>();
            var destroyCallbacks = new EPStatementDestroyCallbackList();

            // determine context
            var contextName = StatementSpec.OptionalContextName;
            ContextPropertyRegistry contextPropertyRegistry = (contextName != null)
                ? services.ContextManagementService.GetContextDescriptor(contextName).ContextPropertyRegistry
                : null;

            // create subselect information
            var subSelectStreamDesc = EPStatementStartMethodHelperSubselect.CreateSubSelectActivation(
                services, StatementSpec, statementContext, destroyCallbacks);

            // obtain activator
            var streamSpec = StatementSpec.StreamSpecs[0];
            ActivatorResult activatorResult;
            StreamSelector? optionalStreamSelector = null;
            if (streamSpec is FilterStreamSpecCompiled)
            {
                var filterStreamSpec = (FilterStreamSpecCompiled) streamSpec;
                activatorResult = ActivatorFilter(statementContext, services, filterStreamSpec);
            }
            else if (streamSpec is PatternStreamSpecCompiled)
            {
                var patternStreamSpec = (PatternStreamSpecCompiled) streamSpec;
                activatorResult = ActivatorPattern(statementContext, services, patternStreamSpec);
            }
            else if (streamSpec is NamedWindowConsumerStreamSpec)
            {
                var namedSpec = (NamedWindowConsumerStreamSpec) streamSpec;
                activatorResult = ActivatorNamedWindow(services, namedSpec, statementContext);
            }
            else if (streamSpec is TableQueryStreamSpec)
            {
                throw new ExprValidationException("Tables cannot be used in an on-action statement triggering stream");
            }
            else
            {
                throw new ExprValidationException("Unknown stream specification type: " + streamSpec);
            }

            // context-factory creation
            //
            // handle on-merge for table
            ContextFactoryResult contextFactoryResult;
            TableMetadata tableMetadata = null;
            if (StatementSpec.OnTriggerDesc is OnTriggerWindowDesc)
            {
                var onTriggerDesc = (OnTriggerWindowDesc) StatementSpec.OnTriggerDesc;
                tableMetadata = services.TableService.GetTableMetadata(onTriggerDesc.WindowName);
                if (tableMetadata != null)
                {
                    contextFactoryResult = HandleContextFactoryOnTriggerTable(
                        statementContext, services, onTriggerDesc, contextName, streamSpec, activatorResult,
                        contextPropertyRegistry, subSelectStreamDesc);
                    services.StatementVariableRefService.AddReferences(
                        statementContext.StatementName, tableMetadata.TableName);
                }
                else if (services.NamedWindowMgmtService.GetProcessor(onTriggerDesc.WindowName) != null)
                {
                    services.StatementVariableRefService.AddReferences(
                        statementContext.StatementName, onTriggerDesc.WindowName);
                    contextFactoryResult = HandleContextFactoryOnTriggerNamedWindow(
                        services, statementContext, onTriggerDesc, contextName, streamSpec, contextPropertyRegistry,
                        subSelectStreamDesc, activatorResult, optionalStreamSelector, stopCallbacks);
                }
                else
                {
                    throw new ExprValidationException(
                        "A named window or variable by name '" + onTriggerDesc.WindowName + "' does not exist");
                }
            }
            else if (StatementSpec.OnTriggerDesc is OnTriggerSetDesc)
            {
                // variable assignments
                var desc = (OnTriggerSetDesc) StatementSpec.OnTriggerDesc;
                contextFactoryResult = HandleContextFactorySetVariable(
                    StatementSpec, statementContext, services, desc, streamSpec, subSelectStreamDesc,
                    contextPropertyRegistry, activatorResult);
            }
            else
            {
                // split-stream use case
                var desc = (OnTriggerSplitStreamDesc) StatementSpec.OnTriggerDesc;
                contextFactoryResult = HandleContextFactorySplitStream(
                    StatementSpec, statementContext, services, desc, streamSpec, contextPropertyRegistry,
                    subSelectStreamDesc, activatorResult);
            }
            statementContext.StatementAgentInstanceFactory = contextFactoryResult.ContextFactory;
            var resultEventType = contextFactoryResult.ResultSetProcessorPrototype == null
                ? null
                : contextFactoryResult.ResultSetProcessorPrototype.ResultSetProcessorFactory.ResultEventType;

            // perform start of hook-up to start
            Viewable finalViewable;
            EPStatementStopMethod stopStatementMethod;
            IDictionary<ExprSubselectNode, SubSelectStrategyHolder> subselectStrategyInstances;
            IDictionary<ExprTableAccessNode, ExprTableAccessEvalStrategy> tableAccessStrategyInstances;
            AggregationService aggregationService;

            // add cleanup for table metadata, if required
            if (tableMetadata != null)
            {
                destroyCallbacks.AddCallback(
                    new EPStatementDestroyCallbackTableIdxRef(
                        services.TableService, tableMetadata, statementContext.StatementName));
                destroyCallbacks.AddCallback(
                    new EPStatementDestroyCallbackTableUpdStr(
                        services.TableService, tableMetadata, statementContext.StatementName));
            }

            // With context - delegate instantiation to context
            var stopMethod = new EPStatementStopMethodImpl(statementContext, stopCallbacks);
            if (StatementSpec.OptionalContextName != null)
            {
                // use statement-wide agent-instance-specific aggregation service
                aggregationService = statementContext.StatementAgentInstanceRegistry.AgentInstanceAggregationService;

                // use statement-wide agent-instance-specific subselects
                var aiRegistryExpr = statementContext.StatementAgentInstanceRegistry.AgentInstanceExprService;
                subselectStrategyInstances = new Dictionary<ExprSubselectNode, SubSelectStrategyHolder>();
                foreach (var entry in contextFactoryResult.SubSelectStrategyCollection.Subqueries)
                {
                    var specificService = aiRegistryExpr.AllocateSubselect(entry.Key);
                    entry.Key.Strategy = specificService;

                    var subselectPriorStrategies = new Dictionary<ExprPriorNode, ExprPriorEvalStrategy>();
                    foreach (ExprPriorNode subselectPrior in entry.Value.PriorNodesList)
                    {
                        var specificSubselectPriorService = aiRegistryExpr.AllocatePrior(subselectPrior);
                        subselectPriorStrategies.Put(subselectPrior, specificSubselectPriorService);
                    }

                    var subselectPreviousStrategies = new Dictionary<ExprPreviousNode, ExprPreviousEvalStrategy>();
                    foreach (ExprPreviousNode subselectPrevious in entry.Value.PrevNodesList)
                    {
                        var specificSubselectPreviousService = aiRegistryExpr.AllocatePrevious(subselectPrevious);
                        subselectPreviousStrategies.Put(subselectPrevious, specificSubselectPreviousService);
                    }

                    var subselectAggregation = aiRegistryExpr.AllocateSubselectAggregation(entry.Key);
                    subselectStrategyInstances.Put(
                        entry.Key,
                        new SubSelectStrategyHolder(
                            specificService, subselectAggregation, subselectPriorStrategies, subselectPreviousStrategies,
                            null, null, null));
                }

                // use statement-wide agent-instance-specific tables
                tableAccessStrategyInstances = new Dictionary<ExprTableAccessNode, ExprTableAccessEvalStrategy>();
                if (StatementSpec.TableNodes != null)
                {
                    foreach (ExprTableAccessNode tableNode in StatementSpec.TableNodes)
                    {
                        var specificService = aiRegistryExpr.AllocateTableAccess(tableNode);
                        tableAccessStrategyInstances.Put(tableNode, specificService);
                    }
                }

                var mergeView = new ContextMergeViewForwarding(resultEventType);
                finalViewable = mergeView;

                var statement = new ContextManagedStatementOnTriggerDesc(
                    StatementSpec, statementContext, mergeView, contextFactoryResult.ContextFactory);
                services.ContextManagementService.AddStatement(contextName, statement, isRecoveringResilient);
                stopStatementMethod = new ProxyEPStatementStopMethod(
                    () =>
                    {
                        services.ContextManagementService.StoppedStatement(
                            contextName, statementContext.StatementName, statementContext.StatementId,
                            statementContext.Expression, statementContext.ExceptionHandlingService);
                        stopMethod.Stop();
                    });

                destroyCallbacks.AddCallback(
                    new EPStatementDestroyCallbackContext(
                        services.ContextManagementService, contextName, statementContext.StatementName,
                        statementContext.StatementId));
            }
            else
            {
                // Without context - start here
                var agentInstanceContext = GetDefaultAgentInstanceContext(statementContext);
                var resultOfStart = contextFactoryResult.ContextFactory.NewContext(agentInstanceContext, isRecoveringResilient);
                finalViewable = resultOfStart.FinalView;
                var stopCallback = services.EpStatementFactory.MakeStopMethod(resultOfStart);
                stopStatementMethod = new ProxyEPStatementStopMethod(
                    () =>
                    {
                        stopCallback.Stop();
                        stopMethod.Stop();
                    });
                aggregationService = resultOfStart.OptionalAggegationService;
                subselectStrategyInstances = resultOfStart.SubselectStrategies;
                tableAccessStrategyInstances = resultOfStart.TableAccessEvalStrategies;

                if (statementContext.StatementExtensionServicesContext != null &&
                    statementContext.StatementExtensionServicesContext.StmtResources != null)
                {
                    var holder =
                        statementContext.StatementExtensionServicesContext.ExtractStatementResourceHolder(resultOfStart);
                    statementContext.StatementExtensionServicesContext.StmtResources.Unpartitioned = holder;
                    statementContext.StatementExtensionServicesContext.PostProcessStart(
                        resultOfStart, isRecoveringResilient);
                }
            }

            // initialize aggregation expression nodes
            if (contextFactoryResult.ResultSetProcessorPrototype != null &&
                contextFactoryResult.ResultSetProcessorPrototype.AggregationServiceFactoryDesc != null)
            {
                EPStatementStartMethodHelperAssignExpr.AssignAggregations(
                    aggregationService,
                    contextFactoryResult.ResultSetProcessorPrototype.AggregationServiceFactoryDesc.Expressions);
            }

            // assign subquery nodes
            EPStatementStartMethodHelperAssignExpr.AssignSubqueryStrategies(
                contextFactoryResult.SubSelectStrategyCollection, subselectStrategyInstances);

            // assign tables
            EPStatementStartMethodHelperAssignExpr.AssignTableAccessStrategies(tableAccessStrategyInstances);

            return new EPStatementStartResult(finalViewable, stopStatementMethod, destroyCallbacks);
        }

        private ActivatorResult ActivatorNamedWindow(
            EPServicesContext services,
            NamedWindowConsumerStreamSpec namedSpec,
            StatementContext statementContext)
        {
            var processor = services.NamedWindowMgmtService.GetProcessor(namedSpec.WindowName);
            if (processor == null)
            {
                throw new ExprValidationException(
                    "A named window by name '" + namedSpec.WindowName + "' does not exist");
            }
            var triggerEventTypeName = namedSpec.WindowName;
            var activator = services.ViewableActivatorFactory.CreateNamedWindow(processor, namedSpec, statementContext);
            var activatorResultEventType = processor.NamedWindowType;
            if (namedSpec.OptPropertyEvaluator != null)
            {
                activatorResultEventType = namedSpec.OptPropertyEvaluator.FragmentEventType;
            }
            services.NamedWindowConsumerMgmtService.AddConsumer(statementContext, namedSpec);
            return new ActivatorResult(activator, triggerEventTypeName, activatorResultEventType);
        }

        private ActivatorResult ActivatorPattern(
            StatementContext statementContext,
            EPServicesContext services,
            PatternStreamSpecCompiled patternStreamSpec)
        {
            var usedByChildViews = patternStreamSpec.ViewSpecs.Length > 0 || (StatementSpec.InsertIntoDesc != null);
            var patternTypeName = statementContext.StatementId + "_patternon";
            var eventType = services.EventAdapterService.CreateSemiAnonymousMapType(
                patternTypeName, patternStreamSpec.TaggedEventTypes, patternStreamSpec.ArrayEventTypes, usedByChildViews);

            var rootNode = services.PatternNodeFactory.MakeRootNode(patternStreamSpec.EvalFactoryNode);
            var patternContext = statementContext.PatternContextFactory.CreateContext(
                statementContext, 0, rootNode, patternStreamSpec.MatchedEventMapMeta, true);
            var activator = services.ViewableActivatorFactory.CreatePattern(
                patternContext, rootNode, eventType,
                EPStatementStartMethodHelperUtil.IsConsumingFilters(patternStreamSpec.EvalFactoryNode), false, false,
                false);
            return new ActivatorResult(activator, null, eventType);
        }

        private ActivatorResult ActivatorFilter(
            StatementContext statementContext,
            EPServicesContext services,
            FilterStreamSpecCompiled filterStreamSpec)
        {
            var triggerEventTypeName = filterStreamSpec.FilterSpec.FilterForEventTypeName;
            InstrumentationAgent instrumentationAgentOnTrigger = null;
            if (InstrumentationHelper.ENABLED)
            {
                var eventTypeName = filterStreamSpec.FilterSpec.FilterForEventType.Name;
                instrumentationAgentOnTrigger = new ProxyInstrumentationAgent
                {
                    ProcIndicateQ = () => InstrumentationHelper.Get().QFilterActivationOnTrigger(eventTypeName),
                    ProcIndicateA = () => InstrumentationHelper.Get().AFilterActivationOnTrigger()
                };
            }
            var activator = services.ViewableActivatorFactory.CreateFilterProxy(
                services, filterStreamSpec.FilterSpec, statementContext.Annotations, false,
                instrumentationAgentOnTrigger, false, 0);
            var activatorResultEventType = filterStreamSpec.FilterSpec.ResultEventType;
            return new ActivatorResult(activator, triggerEventTypeName, activatorResultEventType);
        }

        private ContextFactoryResult HandleContextFactorySetVariable(
            StatementSpecCompiled statementSpec,
            StatementContext statementContext,
            EPServicesContext services,
            OnTriggerSetDesc desc,
            StreamSpecCompiled streamSpec,
            SubSelectActivationCollection subSelectStreamDesc,
            ContextPropertyRegistry contextPropertyRegistry,
            ActivatorResult activatorResult)
        {
            var typeService = new StreamTypeServiceImpl(
                new EventType[]
                {
                    activatorResult.ActivatorResultEventType
                }, new string[]
                {
                    streamSpec.OptionalStreamName
                }, new bool[]
                {
                    true
                }, services.EngineURI, false);
            var validationContext = new ExprValidationContext(
                statementContext.Container,
                typeService, 
                statementContext.EngineImportService, 
                statementContext.StatementExtensionServicesContext,
                null, 
                statementContext.SchedulingService, 
                statementContext.VariableService,
                statementContext.TableService, GetDefaultAgentInstanceContext(statementContext),
                statementContext.EventAdapterService, 
                statementContext.StatementName, 
                statementContext.StatementId,
                statementContext.Annotations, 
                statementContext.ContextDescriptor,
                statementContext.ScriptingService,
                false, false, true, false, null, false);

            // Materialize sub-select views
            var subSelectStrategyCollection = EPStatementStartMethodHelperSubselect.PlanSubSelect(
                services, statementContext, IsQueryPlanLogging(services), subSelectStreamDesc, new string[]
                {
                    streamSpec.OptionalStreamName
                }, new EventType[]
                {
                    activatorResult.ActivatorResultEventType
                }, new string[]
                {
                    activatorResult.TriggerEventTypeName
                }, statementSpec.DeclaredExpressions, contextPropertyRegistry);

            foreach (var assignment in desc.Assignments)
            {
                var validated = ExprNodeUtility.GetValidatedAssignment(assignment, validationContext);
                assignment.Expression = validated;
            }

            OnSetVariableViewFactory onSetVariableViewFactory;
            try
            {
                var exprEvaluatorContext = new ExprEvaluatorContextStatement(statementContext, false);
                onSetVariableViewFactory = new OnSetVariableViewFactory(
                    statementContext.StatementId, desc, statementContext.EventAdapterService,
                    statementContext.VariableService, statementContext.StatementResultService, exprEvaluatorContext);
            }
            catch (VariableValueException ex)
            {
                throw new ExprValidationException("Error in variable assignment: " + ex.Message, ex);
            }

            EventType outputEventType = onSetVariableViewFactory.EventType;

            // handle output format
            var defaultSelectAllSpec = new StatementSpecCompiled();
            defaultSelectAllSpec.SelectClauseSpec.SetSelectExprList(new SelectClauseElementWildcard());
            var streamTypeService = new StreamTypeServiceImpl(
                new EventType[]
                {
                    outputEventType
                }, new string[]
                {
                    "trigger_stream"
                }, new bool[]
                {
                    true
                }, services.EngineURI, false);
            var outputResultSetProcessorPrototype =
                ResultSetProcessorFactoryFactory.GetProcessorPrototype(
                    defaultSelectAllSpec, statementContext, streamTypeService, null, new bool[0], true,
                    contextPropertyRegistry, null, services.ConfigSnapshot, services.ResultSetProcessorHelperFactory,
                    false, true);

            var outputViewFactory = OutputProcessViewFactoryFactory.Make(
                statementSpec, services.InternalEventRouter, statementContext, null, null, services.TableService,
                outputResultSetProcessorPrototype.ResultSetProcessorFactory.ResultSetProcessorType,
                services.ResultSetProcessorHelperFactory, services.StatementVariableRefService);
            var contextFactory = new StatementAgentInstanceFactoryOnTriggerSetVariable(
                statementContext, statementSpec, services, activatorResult.Activator, subSelectStrategyCollection,
                onSetVariableViewFactory, outputResultSetProcessorPrototype, outputViewFactory);
            return new ContextFactoryResult(contextFactory, subSelectStrategyCollection, null);
        }

        private ContextFactoryResult HandleContextFactorySplitStream(
            StatementSpecCompiled statementSpec,
            StatementContext statementContext,
            EPServicesContext services,
            OnTriggerSplitStreamDesc desc,
            StreamSpecCompiled streamSpec,
            ContextPropertyRegistry contextPropertyRegistry,
            SubSelectActivationCollection subSelectStreamDesc,
            ActivatorResult activatorResult)
        {
            if (statementSpec.InsertIntoDesc == null)
            {
                throw new ExprValidationException(
                    "Required insert-into clause is not provided, the clause is required for split-stream syntax");
            }
            if ((statementSpec.GroupByExpressions != null && statementSpec.GroupByExpressions.GroupByNodes.Length > 0) ||
                (statementSpec.HavingExprRootNode != null) || (statementSpec.OrderByList.Length > 0))
            {
                throw new ExprValidationException(
                    "A group-by clause, having-clause or order-by clause is not allowed for the split stream syntax");
            }

            var streamName = streamSpec.OptionalStreamName;
            if (streamName == null)
            {
                streamName = "stream_0";
            }
            var typeServiceTrigger = new StreamTypeServiceImpl(
                new EventType[]
                {
                    activatorResult.ActivatorResultEventType
                }, new string[]
                {
                    streamName
                }, new bool[]
                {
                    true
                }, services.EngineURI, false);

            // materialize sub-select views
            var subSelectStrategyCollection = EPStatementStartMethodHelperSubselect.PlanSubSelect(
                services, statementContext, IsQueryPlanLogging(services), subSelectStreamDesc, new string[]
                {
                    streamSpec.OptionalStreamName
                }, new EventType[]
                {
                    activatorResult.ActivatorResultEventType
                }, new string[]
                {
                    activatorResult.TriggerEventTypeName
                }, statementSpec.DeclaredExpressions, contextPropertyRegistry);

            // compile top-level split
            var items = new EPStatementStartMethodOnTriggerItem[desc.SplitStreams.Count + 1];
            items[0] = OnSplitValidate(
                statementSpec, typeServiceTrigger, contextPropertyRegistry, services, statementContext, null);

            // compile each additional split
            var index = 1;
            var assignedTypeNumberStack = new List<int>();
            foreach (var splits in desc.SplitStreams)
            {
                var splitSpec = new StatementSpecCompiled();
                splitSpec.InsertIntoDesc = splits.InsertInto;
                splitSpec.SelectClauseSpec = StatementLifecycleSvcImpl.CompileSelectAllowSubselect(splits.SelectClause);
                splitSpec.FilterExprRootNode = splits.WhereClause;

                PropertyEvaluator optionalPropertyEvaluator = null;
                StreamTypeService typeServiceProperty;
                if (splits.FromClause != null)
                {
                    optionalPropertyEvaluator =
                        PropertyEvaluatorFactory.MakeEvaluator(
                            statementContext.Container,
                            splits.FromClause.PropertyEvalSpec, 
                            activatorResult.ActivatorResultEventType, 
                            streamName,
                            services.EventAdapterService, 
                            services.EngineImportService, 
                            services.SchedulingService,
                            services.VariableService, 
                            services.ScriptingService, 
                            services.TableService, 
                            typeServiceTrigger.EngineURIQualifier, 
                            statementContext.StatementId, 
                            statementContext.StatementName, 
                            statementContext.Annotations,
                            assignedTypeNumberStack, 
                            services.ConfigSnapshot, 
                            services.NamedWindowMgmtService,
                            statementContext.StatementExtensionServicesContext
                            );

                    typeServiceProperty = new StreamTypeServiceImpl(
                        new EventType[]
                        {
                            optionalPropertyEvaluator.FragmentEventType
                        }, new string[]
                        {
                            splits.FromClause.OptionalStreamName
                        }, new bool[]
                        {
                            true
                        }, services.EngineURI, false);
                }
                else
                {
                    typeServiceProperty = typeServiceTrigger;
                }

                items[index] = OnSplitValidate(
                    splitSpec, typeServiceProperty, contextPropertyRegistry, services, statementContext,
                    optionalPropertyEvaluator);
                index++;
            }

            var contextFactory = new StatementAgentInstanceFactoryOnTriggerSplit(
                statementContext, statementSpec, services, activatorResult.Activator, subSelectStrategyCollection, items,
                activatorResult.ActivatorResultEventType);
            return new ContextFactoryResult(contextFactory, subSelectStrategyCollection, null);
        }

        private ContextFactoryResult HandleContextFactoryOnTriggerNamedWindow(
            EPServicesContext services,
            StatementContext statementContext,
            OnTriggerWindowDesc onTriggerDesc,
            string contextName,
            StreamSpecCompiled streamSpec,
            ContextPropertyRegistry contextPropertyRegistry,
            SubSelectActivationCollection subSelectStreamDesc,
            ActivatorResult activatorResult,
            StreamSelector? optionalStreamSelector,
            IList<StopCallback> stopCallbacks)
        {
            var processor = services.NamedWindowMgmtService.GetProcessor(onTriggerDesc.WindowName);

            // validate context
            ValidateOnExpressionContext(
                contextName, processor.ContextName, "Named window '" + onTriggerDesc.WindowName + "'");

            var namedWindowType = processor.NamedWindowType;
            services.StatementEventTypeRefService.AddReferences(
                statementContext.StatementName, new string[]
                {
                    onTriggerDesc.WindowName
                });

            // validate expressions and plan subselects
            var validationResult = ValidateOnTriggerPlan(
                services, statementContext, onTriggerDesc, namedWindowType, streamSpec, activatorResult,
                contextPropertyRegistry, subSelectStreamDesc, null);

            InternalEventRouter routerService = null;
            var addToFront = false;
            string optionalInsertIntoTableName = null;
            if (StatementSpec.InsertIntoDesc != null || onTriggerDesc is OnTriggerMergeDesc)
            {
                routerService = services.InternalEventRouter;
            }
            if (StatementSpec.InsertIntoDesc != null)
            {
                var tableMetadata = services.TableService.GetTableMetadata(StatementSpec.InsertIntoDesc.EventTypeName);
                if (tableMetadata != null)
                {
                    optionalInsertIntoTableName = tableMetadata.TableName;
                    routerService = null;
                }
                addToFront =
                    statementContext.NamedWindowMgmtService.IsNamedWindow(StatementSpec.InsertIntoDesc.EventTypeName);
            }
            bool isDistinct = StatementSpec.SelectClauseSpec.IsDistinct;
            EventType selectResultEventType =
                validationResult.ResultSetProcessorPrototype.ResultSetProcessorFactory.ResultEventType;
            var createNamedWindowMetricsHandle = processor.CreateNamedWindowMetricsHandle;

            var onExprFactory = NamedWindowOnExprFactoryFactory.Make(
                namedWindowType, onTriggerDesc.WindowName, validationResult.ZeroStreamAliasName,
                onTriggerDesc,
                activatorResult.ActivatorResultEventType, streamSpec.OptionalStreamName, addToFront, routerService,
                selectResultEventType,
                statementContext, createNamedWindowMetricsHandle, isDistinct, optionalStreamSelector,
                optionalInsertIntoTableName);

            // For on-delete/set/update/merge, create an output processor that passes on as a wildcard the underlying event
            ResultSetProcessorFactoryDesc outputResultSetProcessorPrototype = null;
            if ((StatementSpec.OnTriggerDesc.OnTriggerType == OnTriggerType.ON_DELETE) ||
                (StatementSpec.OnTriggerDesc.OnTriggerType == OnTriggerType.ON_UPDATE) ||
                (StatementSpec.OnTriggerDesc.OnTriggerType == OnTriggerType.ON_MERGE))
            {
                var defaultSelectAllSpec = new StatementSpecCompiled();
                defaultSelectAllSpec.SelectClauseSpec.SetSelectExprList(new SelectClauseElementWildcard());
                var streamTypeService = new StreamTypeServiceImpl(
                    new EventType[]
                    {
                        namedWindowType
                    }, new string[]
                    {
                        "trigger_stream"
                    }, new bool[]
                    {
                        true
                    }, services.EngineURI, false);
                outputResultSetProcessorPrototype =
                    ResultSetProcessorFactoryFactory.GetProcessorPrototype(
                        defaultSelectAllSpec, statementContext, streamTypeService, null, new bool[0], true,
                        contextPropertyRegistry, null, services.ConfigSnapshot, services.ResultSetProcessorHelperFactory,
                        false, true);
            }

            EventType resultEventType =
                validationResult.ResultSetProcessorPrototype.ResultSetProcessorFactory.ResultEventType;
            var outputViewFactory = OutputProcessViewFactoryFactory.Make(
                StatementSpec, services.InternalEventRouter, statementContext, resultEventType, null,
                services.TableService,
                validationResult.ResultSetProcessorPrototype.ResultSetProcessorFactory.ResultSetProcessorType,
                services.ResultSetProcessorHelperFactory, services.StatementVariableRefService);

            var contextFactory = new StatementAgentInstanceFactoryOnTriggerNamedWindow(
                statementContext, StatementSpec, services, activatorResult.Activator,
                validationResult.SubSelectStrategyCollection, validationResult.ResultSetProcessorPrototype,
                validationResult.ValidatedJoin, outputResultSetProcessorPrototype, onExprFactory, outputViewFactory,
                activatorResult.ActivatorResultEventType, processor, stopCallbacks);
            return new ContextFactoryResult(
                contextFactory, validationResult.SubSelectStrategyCollection,
                validationResult.ResultSetProcessorPrototype);
        }

        private TriggerValidationPlanResult ValidateOnTriggerPlan(
            EPServicesContext services,
            StatementContext statementContext,
            OnTriggerWindowDesc onTriggerDesc,
            EventType namedWindowType,
            StreamSpecCompiled streamSpec,
            ActivatorResult activatorResult,
            ContextPropertyRegistry contextPropertyRegistry,
            SubSelectActivationCollection subSelectStreamDesc,
            string optionalTableName)
        {
            var zeroStreamAliasName = onTriggerDesc.OptionalAsName;
            if (zeroStreamAliasName == null)
            {
                zeroStreamAliasName = "stream_0";
            }
            var streamName = streamSpec.OptionalStreamName;
            if (streamName == null)
            {
                streamName = "stream_1";
            }
            var namedWindowTypeName = onTriggerDesc.WindowName;

            // Materialize sub-select views
            // 0 - named window stream
            // 1 - arriving stream
            // 2 - initial value before update
            var subSelectStrategyCollection = EPStatementStartMethodHelperSubselect.PlanSubSelect(
                services, statementContext, IsQueryPlanLogging(services), subSelectStreamDesc, new string[]
                {
                    zeroStreamAliasName,
                    streamSpec.OptionalStreamName
                }, new EventType[]
                {
                    namedWindowType,
                    activatorResult.ActivatorResultEventType
                }, new string[]
                {
                    namedWindowTypeName,
                    activatorResult.TriggerEventTypeName
                }, StatementSpec.DeclaredExpressions, contextPropertyRegistry);

            var typeService = new StreamTypeServiceImpl(
                new EventType[]
                {
                    namedWindowType,
                    activatorResult.ActivatorResultEventType
                }, new string[]
                {
                    zeroStreamAliasName,
                    streamName
                }, new bool[]
                {
                    false,
                    true
                }, services.EngineURI, true);

            // allow "initial" as a prefix to properties
            StreamTypeServiceImpl assignmentTypeService;
            if (zeroStreamAliasName.Equals(INITIAL_VALUE_STREAM_NAME) || streamName.Equals(INITIAL_VALUE_STREAM_NAME))
            {
                assignmentTypeService = typeService;
            }
            else
            {
                assignmentTypeService = new StreamTypeServiceImpl(
                    new EventType[]
                    {
                        namedWindowType,
                        activatorResult.ActivatorResultEventType,
                        namedWindowType
                    }, new string[]
                    {
                        zeroStreamAliasName,
                        streamName,
                        INITIAL_VALUE_STREAM_NAME
                    }, new bool[]
                    {
                        false,
                        true,
                        true
                    }, services.EngineURI, false);
                assignmentTypeService.IsStreamZeroUnambigous = true;
            }

            if (onTriggerDesc is OnTriggerWindowUpdateDesc)
            {
                var updateDesc = (OnTriggerWindowUpdateDesc) onTriggerDesc;
                var validationContext = new ExprValidationContext(
                    statementContext.Container,
                    assignmentTypeService, 
                    statementContext.EngineImportService,
                    statementContext.StatementExtensionServicesContext, null, 
                    statementContext.SchedulingService,
                    statementContext.VariableService,
                    statementContext.TableService,
                    GetDefaultAgentInstanceContext(statementContext), 
                    statementContext.EventAdapterService,
                    statementContext.StatementName, 
                    statementContext.StatementId, 
                    statementContext.Annotations,
                    statementContext.ContextDescriptor,
                    statementContext.ScriptingService,
                    false, false, true, false,
                    null, false);
                foreach (var assignment in updateDesc.Assignments)
                {
                    var validated = ExprNodeUtility.GetValidatedAssignment(assignment, validationContext);
                    assignment.Expression = validated;
                    EPStatementStartMethodHelperValidate.ValidateNoAggregations(
                        validated, "Aggregation functions may not be used within an on-update-clause");
                }
            }
            if (onTriggerDesc is OnTriggerMergeDesc)
            {
                var mergeDesc = (OnTriggerMergeDesc) onTriggerDesc;
                ValidateMergeDesc(
                    mergeDesc, statementContext, namedWindowType, zeroStreamAliasName,
                    activatorResult.ActivatorResultEventType, streamName);
            }

            // validate join expression
            var validatedJoin = ValidateJoinNamedWindow(
                services.EngineURI, statementContext, ExprNodeOrigin.WHERE, StatementSpec.FilterRootNode,
                namedWindowType, zeroStreamAliasName, namedWindowTypeName,
                activatorResult.ActivatorResultEventType, streamName, activatorResult.TriggerEventTypeName,
                optionalTableName);

            // validate filter, output rate limiting
            EPStatementStartMethodHelperValidate.ValidateNodes(StatementSpec, statementContext, typeService, null);

            // Construct a processor for results; for use in on-select to process selection results
            // Use a wildcard select if the select-clause is empty, such as for on-delete.
            // For on-select the select clause is not empty.
            if (StatementSpec.SelectClauseSpec.SelectExprList.Length == 0)
            {
                StatementSpec.SelectClauseSpec.SetSelectExprList(new SelectClauseElementWildcard());
            }
            var resultSetProcessorPrototype = ResultSetProcessorFactoryFactory.GetProcessorPrototype(
                StatementSpec, statementContext, typeService, null, new bool[0], true, contextPropertyRegistry, null,
                services.ConfigSnapshot, services.ResultSetProcessorHelperFactory, false, true);

            return new TriggerValidationPlanResult(
                subSelectStrategyCollection, resultSetProcessorPrototype, validatedJoin, zeroStreamAliasName);
        }

        private void ValidateOnExpressionContext(string onExprContextName, string desiredContextName, string title)
        {
            if (onExprContextName == null)
            {
                if (desiredContextName != null)
                {
                    throw new ExprValidationException(
                        string.Format(
                            "Cannot create on-trigger expression: {0} was declared with context '{1}', please declare the same context name",
                            title, CompatExtensions.RenderAny(desiredContextName)));
                }
                return;
            }
            if (!onExprContextName.Equals(desiredContextName))
            {
                throw new ExprValidationException(
                    string.Format(
                        "Cannot create on-trigger expression: {0} was declared with context '{1}', please use the same context instead",
                        title, CompatExtensions.RenderAny(desiredContextName)));
            }
        }

        private ContextFactoryResult HandleContextFactoryOnTriggerTable(
            StatementContext statementContext,
            EPServicesContext services,
            OnTriggerWindowDesc onTriggerDesc,
            string contextName,
            StreamSpecCompiled streamSpec,
            ActivatorResult activatorResult,
            ContextPropertyRegistry contextPropertyRegistry,
            SubSelectActivationCollection subSelectStreamDesc)
        {
            var metadata = services.TableService.GetTableMetadata(onTriggerDesc.WindowName);

            // validate context
            ValidateOnExpressionContext(contextName, metadata.ContextName, "Table '" + onTriggerDesc.WindowName + "'");

            InternalEventRouter routerService = null;
            if (StatementSpec.InsertIntoDesc != null || onTriggerDesc is OnTriggerMergeDesc)
            {
                routerService = services.InternalEventRouter;
            }

            // validate expressions and plan subselects
            var validationResult = ValidateOnTriggerPlan(
                services, statementContext, onTriggerDesc, metadata.InternalEventType, streamSpec, activatorResult,
                contextPropertyRegistry, subSelectStreamDesc, metadata.TableName);

            // table on-action view factory
            var onExprFactory = TableOnViewFactoryFactory.Make(
                metadata, onTriggerDesc, activatorResult.ActivatorResultEventType, streamSpec.OptionalStreamName,
                statementContext, statementContext.EpStatementHandle.MetricsHandle, false, routerService);

            // For on-delete/set/update/merge, create an output processor that passes on as a wildcard the underlying event
            ResultSetProcessorFactoryDesc outputResultSetProcessorPrototype = null;
            if ((StatementSpec.OnTriggerDesc.OnTriggerType == OnTriggerType.ON_DELETE) ||
                (StatementSpec.OnTriggerDesc.OnTriggerType == OnTriggerType.ON_UPDATE) ||
                (StatementSpec.OnTriggerDesc.OnTriggerType == OnTriggerType.ON_MERGE))
            {
                var defaultSelectAllSpec = new StatementSpecCompiled();
                defaultSelectAllSpec.SelectClauseSpec.SetSelectExprList(new SelectClauseElementWildcard());
                // we'll be expecting public-type events as there is no copy op
                var streamTypeService = new StreamTypeServiceImpl(
                    new EventType[]
                    {
                        metadata.PublicEventType
                    }, new string[]
                    {
                        "trigger_stream"
                    }, new bool[]
                    {
                        true
                    }, services.EngineURI, false);
                outputResultSetProcessorPrototype =
                    ResultSetProcessorFactoryFactory.GetProcessorPrototype(
                        defaultSelectAllSpec, statementContext, streamTypeService, null, new bool[0], true,
                        contextPropertyRegistry, null, services.ConfigSnapshot, services.ResultSetProcessorHelperFactory,
                        false, true);
            }

            EventType resultEventType =
                validationResult.ResultSetProcessorPrototype.ResultSetProcessorFactory.ResultEventType;
            var outputViewFactory = OutputProcessViewFactoryFactory.Make(
                StatementSpec, services.InternalEventRouter, statementContext, resultEventType, null,
                services.TableService,
                validationResult.ResultSetProcessorPrototype.ResultSetProcessorFactory.ResultSetProcessorType,
                services.ResultSetProcessorHelperFactory, services.StatementVariableRefService);

            var contextFactory = new StatementAgentInstanceFactoryOnTriggerTable(
                statementContext, StatementSpec, services, activatorResult.Activator,
                validationResult.SubSelectStrategyCollection, validationResult.ResultSetProcessorPrototype,
                validationResult.ValidatedJoin, onExprFactory, activatorResult.ActivatorResultEventType, metadata,
                outputResultSetProcessorPrototype, outputViewFactory);

            return new ContextFactoryResult(
                contextFactory, validationResult.SubSelectStrategyCollection,
                validationResult.ResultSetProcessorPrototype);
        }

        private void ValidateMergeDesc(
            OnTriggerMergeDesc mergeDesc,
            StatementContext statementContext,
            EventType namedWindowType,
            string namedWindowName,
            EventType triggerStreamType,
            string triggerStreamName)
        {
            var exprNodeErrorMessage = "Aggregation functions may not be used within an merge-clause";
            var evaluatorContextStmt = new ExprEvaluatorContextStatement(statementContext, false);

            foreach (var matchedItem in mergeDesc.Items)
            {

                var dummyTypeNoProperties =
                    new MapEventType(
                        EventTypeMetadata.CreateAnonymous("merge_named_window_insert", ApplicationType.MAP),
                        "merge_named_window_insert", 0, null, Collections.EmptyDataMap, null, null, null);
                var twoStreamTypeSvc = new StreamTypeServiceImpl(
                    new EventType[]
                    {
                        namedWindowType,
                        triggerStreamType
                    },
                    new string[]
                    {
                        namedWindowName,
                        triggerStreamName
                    }, new bool[]
                    {
                        true,
                        true
                    }, statementContext.EngineURI, true);
                var insertOnlyTypeSvc = new StreamTypeServiceImpl(
                    new EventType[]
                    {
                        dummyTypeNoProperties,
                        triggerStreamType
                    },
                    new string[]
                    {
                        UuidGenerator.Generate(),
                        triggerStreamName
                    }, new bool[]
                    {
                        true,
                        true
                    }, statementContext.EngineURI, true);

                // we may provide an additional stream "initial" for the prior value, unless already defined
                StreamTypeServiceImpl assignmentStreamTypeSvc;
                if (namedWindowName.Equals(INITIAL_VALUE_STREAM_NAME) ||
                    triggerStreamName.Equals(INITIAL_VALUE_STREAM_NAME))
                {
                    assignmentStreamTypeSvc = twoStreamTypeSvc;
                }
                else
                {
                    assignmentStreamTypeSvc = new StreamTypeServiceImpl(
                        new EventType[]
                        {
                            namedWindowType,
                            triggerStreamType,
                            namedWindowType
                        },
                        new string[]
                        {
                            namedWindowName,
                            triggerStreamName,
                            INITIAL_VALUE_STREAM_NAME
                        }, new bool[]
                        {
                            true,
                            true,
                            true
                        }, statementContext.EngineURI, false);
                    assignmentStreamTypeSvc.IsStreamZeroUnambigous = true;
                }

                if (matchedItem.OptionalMatchCond != null)
                {
                    StreamTypeService matchValidStreams = matchedItem.IsMatchedUnmatched
                        ? twoStreamTypeSvc
                        : insertOnlyTypeSvc;
                    matchedItem.OptionalMatchCond =
                        EPStatementStartMethodHelperValidate.ValidateExprNoAgg(
                            ExprNodeOrigin.MERGEMATCHCOND, matchedItem.OptionalMatchCond, matchValidStreams,
                            statementContext, evaluatorContextStmt, exprNodeErrorMessage, true);
                    if (!matchedItem.IsMatchedUnmatched)
                    {
                        EPStatementStartMethodHelperValidate.ValidateSubqueryExcludeOuterStream(
                            matchedItem.OptionalMatchCond);
                    }
                }

                foreach (var item in matchedItem.Actions)
                {
                    if (item is OnTriggerMergeActionDelete)
                    {
                        var delete = (OnTriggerMergeActionDelete) item;
                        if (delete.OptionalWhereClause != null)
                        {
                            delete.OptionalWhereClause =
                                EPStatementStartMethodHelperValidate.ValidateExprNoAgg(
                                    ExprNodeOrigin.MERGEMATCHWHERE, delete.OptionalWhereClause, twoStreamTypeSvc,
                                    statementContext, evaluatorContextStmt, exprNodeErrorMessage, true);
                        }
                    }
                    else if (item is OnTriggerMergeActionUpdate)
                    {
                        var update = (OnTriggerMergeActionUpdate) item;
                        if (update.OptionalWhereClause != null)
                        {
                            update.OptionalWhereClause =
                                EPStatementStartMethodHelperValidate.ValidateExprNoAgg(
                                    ExprNodeOrigin.MERGEMATCHWHERE, update.OptionalWhereClause, twoStreamTypeSvc,
                                    statementContext, evaluatorContextStmt, exprNodeErrorMessage, true);
                        }
                        foreach (var assignment in update.Assignments)
                        {
                            assignment.Expression =
                                EPStatementStartMethodHelperValidate.ValidateExprNoAgg(
                                    ExprNodeOrigin.UPDATEASSIGN, assignment.Expression, assignmentStreamTypeSvc,
                                    statementContext, evaluatorContextStmt, exprNodeErrorMessage, true);
                        }
                    }
                    else if (item is OnTriggerMergeActionInsert)
                    {
                        var insert = (OnTriggerMergeActionInsert) item;

                        StreamTypeService insertTypeSvc;
                        if (insert.OptionalStreamName == null || insert.OptionalStreamName.Equals(namedWindowName))
                        {
                            insertTypeSvc = insertOnlyTypeSvc;
                        }
                        else
                        {
                            insertTypeSvc = twoStreamTypeSvc;
                        }

                        var compiledSelect = new List<SelectClauseElementCompiled>();
                        if (insert.OptionalWhereClause != null)
                        {
                            insert.OptionalWhereClause =
                                EPStatementStartMethodHelperValidate.ValidateExprNoAgg(
                                    ExprNodeOrigin.MERGEMATCHWHERE, insert.OptionalWhereClause, insertTypeSvc,
                                    statementContext, evaluatorContextStmt, exprNodeErrorMessage, true);
                        }
                        var colIndex = 0;
                        foreach (var raw in insert.SelectClause)
                        {
                            if (raw is SelectClauseStreamRawSpec)
                            {
                                var rawStreamSpec = (SelectClauseStreamRawSpec) raw;
                                int? foundStreamNum = null;
                                for (var s = 0; s < insertTypeSvc.StreamNames.Length; s++)
                                {
                                    if (rawStreamSpec.StreamName.Equals(insertTypeSvc.StreamNames[s]))
                                    {
                                        foundStreamNum = s;
                                        break;
                                    }
                                }
                                if (foundStreamNum == null)
                                {
                                    throw new ExprValidationException(
                                        "Stream by name '" + rawStreamSpec.StreamName + "' was not found");
                                }
                                var streamSelectSpec = new SelectClauseStreamCompiledSpec(
                                    rawStreamSpec.StreamName, rawStreamSpec.OptionalAsName);
                                streamSelectSpec.StreamNumber = foundStreamNum.Value;
                                compiledSelect.Add(streamSelectSpec);
                            }
                            else if (raw is SelectClauseExprRawSpec)
                            {
                                var exprSpec = (SelectClauseExprRawSpec) raw;
                                var validationContext = new ExprValidationContext(
                                    statementContext.Container,
                                    insertTypeSvc, statementContext.EngineImportService,
                                    statementContext.StatementExtensionServicesContext, null,
                                    statementContext.TimeProvider,
                                    statementContext.VariableService,
                                    statementContext.TableService,
                                    evaluatorContextStmt,
                                    statementContext.EventAdapterService,
                                    statementContext.StatementName,
                                    statementContext.StatementId,
                                    statementContext.Annotations,
                                    statementContext.ContextDescriptor,
                                    statementContext.ScriptingService,
                                    false, false, true, false, null, false);
                                var exprCompiled = ExprNodeUtility.GetValidatedSubtree(
                                    ExprNodeOrigin.SELECT, exprSpec.SelectExpression, validationContext);
                                var resultName = exprSpec.OptionalAsName;
                                if (resultName == null)
                                {
                                    if (insert.Columns.Count > colIndex)
                                    {
                                        resultName = insert.Columns[colIndex];
                                    }
                                    else
                                    {
                                        resultName = ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(exprCompiled);
                                    }
                                }
                                compiledSelect.Add(
                                    new SelectClauseExprCompiledSpec(
                                        exprCompiled, resultName, exprSpec.OptionalAsName, exprSpec.IsEvents));
                                EPStatementStartMethodHelperValidate.ValidateNoAggregations(
                                    exprCompiled,
                                    "Expression in a merge-selection may not utilize aggregation functions");
                            }
                            else if (raw is SelectClauseElementWildcard)
                            {
                                compiledSelect.Add(new SelectClauseElementWildcard());
                            }
                            else
                            {
                                throw new IllegalStateException("Unknown select clause item:" + raw);
                            }
                            colIndex++;
                        }
                        insert.SelectClauseCompiled = compiledSelect;
                    }
                    else
                    {
                        throw new ArgumentException("Unrecognized merge item '" + item.GetType().FullName + "'");
                    }
                }
            }
        }

        // For delete actions from named windows
        protected ExprNode ValidateJoinNamedWindow(
            string engineURI,
            StatementContext statementContext,
            ExprNodeOrigin exprNodeOrigin,
            ExprNode deleteJoinExpr,
            EventType namedWindowType,
            string namedWindowStreamName,
            string namedWindowName,
            EventType filteredType,
            string filterStreamName,
            string filteredTypeName,
            string optionalTableName
            )
        {
            if (deleteJoinExpr == null)
            {
                return null;
            }

            var namesAndTypes = new LinkedHashMap<string, Pair<EventType, string>>();
            namesAndTypes.Put(namedWindowStreamName, new Pair<EventType, string>(namedWindowType, namedWindowName));
            namesAndTypes.Put(filterStreamName, new Pair<EventType, string>(filteredType, filteredTypeName));
            var typeService = new StreamTypeServiceImpl(namesAndTypes, engineURI, false, false);

            var evaluatorContextStmt = new ExprEvaluatorContextStatement(statementContext, false);
            var validationContext = new ExprValidationContext(
                statementContext.Container,
                typeService, 
                statementContext.EngineImportService, 
                statementContext.StatementExtensionServicesContext,
                null, 
                statementContext.SchedulingService, 
                statementContext.VariableService,
                statementContext.TableService, 
                evaluatorContextStmt, 
                statementContext.EventAdapterService,
                statementContext.StatementName, 
                statementContext.StatementId, 
                statementContext.Annotations,
                statementContext.ContextDescriptor, 
                statementContext.ScriptingService, 
                false, false, true, false, null, false);
            return ExprNodeUtility.GetValidatedSubtree(exprNodeOrigin, deleteJoinExpr, validationContext);
        }

        internal class ContextFactoryResult
        {
            internal StatementAgentInstanceFactoryOnTriggerBase ContextFactory;
            internal SubSelectStrategyCollection SubSelectStrategyCollection;
            internal ResultSetProcessorFactoryDesc ResultSetProcessorPrototype;

            internal ContextFactoryResult(
                StatementAgentInstanceFactoryOnTriggerBase contextFactory,
                SubSelectStrategyCollection subSelectStrategyCollection,
                ResultSetProcessorFactoryDesc resultSetProcessorPrototype)
            {
                ContextFactory = contextFactory;
                SubSelectStrategyCollection = subSelectStrategyCollection;
                ResultSetProcessorPrototype = resultSetProcessorPrototype;
            }
        }

        internal class ActivatorResult
        {
            internal readonly ViewableActivator Activator;
            internal readonly string TriggerEventTypeName;
            internal readonly EventType ActivatorResultEventType;

            internal ActivatorResult(
                ViewableActivator activator,
                string triggerEventTypeName,
                EventType activatorResultEventType)
            {
                Activator = activator;
                TriggerEventTypeName = triggerEventTypeName;
                ActivatorResultEventType = activatorResultEventType;
            }
        }

        internal class TriggerValidationPlanResult
        {
            internal SubSelectStrategyCollection SubSelectStrategyCollection;
            internal ResultSetProcessorFactoryDesc ResultSetProcessorPrototype;
            internal ExprNode ValidatedJoin;
            internal string ZeroStreamAliasName;

            internal TriggerValidationPlanResult(
                SubSelectStrategyCollection subSelectStrategyCollection,
                ResultSetProcessorFactoryDesc resultSetProcessorPrototype,
                ExprNode validatedJoin,
                string zeroStreamAliasName)
            {
                SubSelectStrategyCollection = subSelectStrategyCollection;
                ResultSetProcessorPrototype = resultSetProcessorPrototype;
                ValidatedJoin = validatedJoin;
                ZeroStreamAliasName = zeroStreamAliasName;
            }
        }
    }
} // end of namespace
