///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.factory;
using com.espertech.esper.core.context.mgr;
using com.espertech.esper.core.context.subselect;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.prev;
using com.espertech.esper.epl.expression.prior;
using com.espertech.esper.epl.expression.subquery;
using com.espertech.esper.epl.expression.table;
using com.espertech.esper.epl.spec;
using com.espertech.esper.rowregex;
using com.espertech.esper.view;

namespace com.espertech.esper.core.start
{
    /// <summary>Starts and provides the stop method for EPL statements.</summary>
    public class EPStatementStartMethodSelect : EPStatementStartMethodBase
    {
        public EPStatementStartMethodSelect(StatementSpecCompiled statementSpec)
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
            var statementSpec = base.StatementSpec;

            // validate use of table: may not both read and write
            ValidateTableAccessUse(statementSpec.IntoTableSpec, statementSpec.TableNodes);
    
            var contextName = statementSpec.OptionalContextName;
            var defaultAgentInstanceContext = GetDefaultAgentInstanceContext(statementContext);
            var selectDesc = EPStatementStartMethodSelectUtil.Prepare(statementSpec, services, statementContext, isRecoveringResilient, defaultAgentInstanceContext, IsQueryPlanLogging(services), null, null, null);
            statementContext.StatementAgentInstanceFactory = selectDesc.StatementAgentInstanceFactorySelect;
    
            // allow extension to walk
            statementContext.StatementExtensionServicesContext.PreStartWalk(selectDesc);
    
            // Determine context
            EPStatementStopMethod stopStatementMethod;
            Viewable finalViewable;
            AggregationService aggregationService;
            IDictionary<ExprSubselectNode, SubSelectStrategyHolder> subselectStrategyInstances;
            IDictionary<ExprPriorNode, ExprPriorEvalStrategy> priorStrategyInstances;
            IDictionary<ExprPreviousNode, ExprPreviousEvalStrategy> previousStrategyInstances;
            IDictionary<ExprTableAccessNode, ExprTableAccessEvalStrategy> tableAccessStrategyInstances;
            var preloadList = Collections.GetEmptyList<StatementAgentInstancePreload>();
            RegexExprPreviousEvalStrategy matchRecognizePrevEvalStrategy;
    
            // With context - delegate instantiation to context
            if (statementSpec.OptionalContextName != null) {
    
                // use statement-wide agent-instance-specific aggregation service
                aggregationService = statementContext.StatementAgentInstanceRegistry.AgentInstanceAggregationService;
    
                // use statement-wide agent-instance-specific subselects
                var aiRegistryExpr = statementContext.StatementAgentInstanceRegistry.AgentInstanceExprService;
    
                subselectStrategyInstances = new Dictionary<ExprSubselectNode, SubSelectStrategyHolder>();
                foreach (var entry in selectDesc.SubSelectStrategyCollection.Subqueries) {
                    var specificService = aiRegistryExpr.AllocateSubselect(entry.Key);
                    entry.Key.Strategy = specificService;
    
                    var subselectPriorStrategies = new Dictionary<ExprPriorNode, ExprPriorEvalStrategy>();
                    foreach (var subselectPrior in entry.Value.PriorNodesList) {
                        var specificSubselectPriorService = aiRegistryExpr.AllocatePrior(subselectPrior);
                        subselectPriorStrategies.Put(subselectPrior, specificSubselectPriorService);
                    }
    
                    var subselectPreviousStrategies = new Dictionary<ExprPreviousNode, ExprPreviousEvalStrategy>();
                    foreach (var subselectPrevious in entry.Value.PrevNodesList) {
                        var specificSubselectPreviousService = aiRegistryExpr.AllocatePrevious(subselectPrevious);
                        subselectPreviousStrategies.Put(subselectPrevious, specificSubselectPreviousService);
                    }
    
                    var subselectAggregation = aiRegistryExpr.AllocateSubselectAggregation(entry.Key);
                    var strategyHolder = new SubSelectStrategyHolder(specificService, subselectAggregation, subselectPriorStrategies, subselectPreviousStrategies, null, null, null);
                    subselectStrategyInstances.Put(entry.Key, strategyHolder);
                }
    
                // use statement-wide agent-instance-specific "prior"
                priorStrategyInstances = new Dictionary<ExprPriorNode, ExprPriorEvalStrategy>();
                foreach (var priorNode in selectDesc.ViewResourceDelegateUnverified.PriorRequests) {
                    var specificService = aiRegistryExpr.AllocatePrior(priorNode);
                    priorStrategyInstances.Put(priorNode, specificService);
                }
    
                // use statement-wide agent-instance-specific "previous"
                previousStrategyInstances = new Dictionary<ExprPreviousNode, ExprPreviousEvalStrategy>();
                foreach (var previousNode in selectDesc.ViewResourceDelegateUnverified.PreviousRequests) {
                    var specificService = aiRegistryExpr.AllocatePrevious(previousNode);
                    previousStrategyInstances.Put(previousNode, specificService);
                }
    
                // use statement-wide agent-instance-specific match-recognize "previous"
                matchRecognizePrevEvalStrategy = aiRegistryExpr.AllocateMatchRecognizePrevious();
    
                // use statement-wide agent-instance-specific tables
                tableAccessStrategyInstances = new Dictionary<ExprTableAccessNode, ExprTableAccessEvalStrategy>();
                if (statementSpec.TableNodes != null) {
                    foreach (var tableNode in statementSpec.TableNodes) {
                        var specificService = aiRegistryExpr.AllocateTableAccess(tableNode);
                        tableAccessStrategyInstances.Put(tableNode, specificService);
                    }
                }
    
                var mergeView = new ContextMergeView(selectDesc.ResultSetProcessorPrototypeDesc.ResultSetProcessorFactory.ResultEventType);
                finalViewable = mergeView;
    
                var statement = new ContextManagedStatementSelectDesc(statementSpec, statementContext, mergeView, selectDesc.StatementAgentInstanceFactorySelect,
                        selectDesc.ResultSetProcessorPrototypeDesc.AggregationServiceFactoryDesc.Expressions,
                        selectDesc.SubSelectStrategyCollection);
                services.ContextManagementService.AddStatement(contextName, statement, isRecoveringResilient);
                var selectStop = selectDesc.StopMethod;
                stopStatementMethod = new ProxyEPStatementStopMethod() {
                    ProcStop = () => {
                        services.ContextManagementService.StoppedStatement(contextName, statementContext.StatementName, statementContext.StatementId, statementContext.Expression, statementContext.ExceptionHandlingService);
                        selectStop.Stop();
                    }
                };
    
                selectDesc.DestroyCallbacks.AddCallback(new EPStatementDestroyCallbackContext(services.ContextManagementService, contextName, statementContext.StatementName, statementContext.StatementId));
            } else {
                // Without context - start here
                var resultOfStart = (StatementAgentInstanceFactorySelectResult) selectDesc.StatementAgentInstanceFactorySelect.NewContext(defaultAgentInstanceContext, isRecoveringResilient);
                finalViewable = resultOfStart.FinalView;
    
                var startResultStop = services.EpStatementFactory.MakeStopMethod(resultOfStart);
                var selectStop = selectDesc.StopMethod;
                stopStatementMethod = new ProxyEPStatementStopMethod() {
                    ProcStop = () => {
                        StatementAgentInstanceUtil.StopSafe(startResultStop, statementContext);
                        selectStop.Stop();
                    }
                };
    
                aggregationService = resultOfStart.OptionalAggegationService;
                subselectStrategyInstances = resultOfStart.SubselectStrategies;
                priorStrategyInstances = resultOfStart.PriorNodeStrategies;
                previousStrategyInstances = resultOfStart.PreviousNodeStrategies;
                tableAccessStrategyInstances = resultOfStart.TableAccessEvalStrategies;
                preloadList = resultOfStart.PreloadList;
    
                matchRecognizePrevEvalStrategy = null;
                if (resultOfStart.TopViews.Length > 0) {
                    var matchRecognize = EventRowRegexHelper.RecursiveFindRegexService(resultOfStart.TopViews[0]);
                    if (matchRecognize != null) {
                        matchRecognizePrevEvalStrategy = matchRecognize.PreviousEvaluationStrategy;
                    }
                }
    
                if (statementContext.StatementExtensionServicesContext != null && statementContext.StatementExtensionServicesContext.StmtResources != null) {
                    var holder = statementContext.StatementExtensionServicesContext.ExtractStatementResourceHolder(resultOfStart);
                    statementContext.StatementExtensionServicesContext.StmtResources.Unpartitioned = holder;
                    statementContext.StatementExtensionServicesContext.PostProcessStart(resultOfStart, isRecoveringResilient);
                }
            }
    
            var matchRecognizeNodes = selectDesc.StatementAgentInstanceFactorySelect.ViewResourceDelegate.PerStream[0].MatchRecognizePreviousRequests;
    
            // assign strategies to expression nodes
            EPStatementStartMethodHelperAssignExpr.AssignExpressionStrategies(selectDesc, aggregationService, subselectStrategyInstances, priorStrategyInstances, previousStrategyInstances, matchRecognizeNodes, matchRecognizePrevEvalStrategy, tableAccessStrategyInstances);
    
            // execute preload if any
            foreach (var preload in preloadList) {
                preload.ExecutePreload(defaultAgentInstanceContext);
            }
    
            // handle association to table
            if (statementSpec.IntoTableSpec != null) {
                services.StatementVariableRefService.AddReferences(statementContext.StatementName, statementSpec.IntoTableSpec.Name);
            }
    
            return new EPStatementStartResult(finalViewable, stopStatementMethod, selectDesc.DestroyCallbacks);
        }
    
        private void ValidateTableAccessUse(IntoTableSpec bindingSpec, ExprTableAccessNode[] tableNodes)
        {
            var statementSpec = base.StatementSpec;
            if (statementSpec.IntoTableSpec != null && statementSpec.TableNodes != null && statementSpec.TableNodes.Length > 0) {
                foreach (var node in statementSpec.TableNodes) {
                    if (node.TableName.Equals(statementSpec.IntoTableSpec.Name)) {
                        throw new ExprValidationException("Invalid use of table '" + statementSpec.IntoTableSpec.Name + "', aggregate-into requires write-only, the expression '" +
                                ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(statementSpec.TableNodes[0]) + "' is not allowed");
                    }
                }
            }
    
        }
    }
} // end of namespace
