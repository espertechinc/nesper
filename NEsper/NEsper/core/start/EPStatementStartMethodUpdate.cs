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
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.factory;
using com.espertech.esper.core.context.mgr;
using com.espertech.esper.core.context.subselect;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.subquery;
using com.espertech.esper.epl.spec;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.core.start
{
	/// <summary>
	/// Starts and provides the stop method for EPL statements.
	/// </summary>
	public class EPStatementStartMethodUpdate : EPStatementStartMethodBase
	{
	    public EPStatementStartMethodUpdate(StatementSpecCompiled statementSpec)
            : base(statementSpec)
        {
	    }

	    public override EPStatementStartResult StartInternal(EPServicesContext services, StatementContext statementContext, bool isNewStatement, bool isRecoveringStatement, bool isRecoveringResilient)
        {
	        // define stop and destroy
	        IList<StopCallback> stopCallbacks = new List<StopCallback>();
	        var destroyCallbacks = new EPStatementDestroyCallbackList();

	        // determine context
	        var contextName = _statementSpec.OptionalContextName;
	        if (contextName != null) {
	            throw new ExprValidationException("Update IStream is not supported in conjunction with a context");
	        }

	        // First we create streams for subselects, if there are any
	        var subSelectStreamDesc = EPStatementStartMethodHelperSubselect.CreateSubSelectActivation(services, _statementSpec, statementContext, destroyCallbacks);

	        var streamSpec = _statementSpec.StreamSpecs[0];
	        var updateSpec = _statementSpec.UpdateSpec;
	        string triggereventTypeName;

	        if (streamSpec is FilterStreamSpecCompiled)
	        {
	            var filterStreamSpec = (FilterStreamSpecCompiled) streamSpec;
	            triggereventTypeName = filterStreamSpec.FilterSpec.FilterForEventTypeName;
	        }
	        else if (streamSpec is NamedWindowConsumerStreamSpec)
	        {
	            var namedSpec = (NamedWindowConsumerStreamSpec) streamSpec;
	            triggereventTypeName = namedSpec.WindowName;
	        }
	        else if (streamSpec is TableQueryStreamSpec) {
	            throw new ExprValidationException("Tables cannot be used in an update-istream statement");
	        }
	        else
	        {
	            throw new ExprValidationException("Unknown stream specification streamEventType: " + streamSpec);
	        }

	        // determine a stream name
	        var streamName = triggereventTypeName;
	        if (updateSpec.OptionalStreamName != null)
	        {
	            streamName = updateSpec.OptionalStreamName;
	        }

	        var streamEventType = services.EventAdapterService.GetEventTypeByName(triggereventTypeName);
	        StreamTypeService typeService = new StreamTypeServiceImpl(new EventType[] {streamEventType}, new string[] {streamName}, new bool[] {true}, services.EngineURI, false);

	        // determine subscriber result types
	        var evaluatorContextStmt = new ExprEvaluatorContextStatement(statementContext, false);
	        statementContext.StatementResultService.SetSelectClause(new Type[] {streamEventType.UnderlyingType}, new string[] {"*"}, false, null, evaluatorContextStmt);

	        // Materialize sub-select views
	        var subSelectStrategyCollection = EPStatementStartMethodHelperSubselect.PlanSubSelect(
                services, statementContext, IsQueryPlanLogging(services), subSelectStreamDesc, 
                new string[]{streamName}, new EventType[]{streamEventType}, 
                new string[]{triggereventTypeName}, _statementSpec.DeclaredExpressions,
                null);

	        var validationContext = new ExprValidationContext(
	            typeService, statementContext.MethodResolutionService, null, statementContext.SchedulingService,
	            statementContext.VariableService, statementContext.TableService, evaluatorContextStmt,
	            statementContext.EventAdapterService, statementContext.StatementName, statementContext.StatementId,
	            statementContext.Annotations, statementContext.ContextDescriptor, statementContext.ScriptingService, false,
	            false, false, false, null, false);
	        foreach (var assignment in updateSpec.Assignments)
	        {
	            var validated = ExprNodeUtility.GetValidatedAssignment(assignment, validationContext);
	            assignment.Expression = validated;
	            EPStatementStartMethodHelperValidate.ValidateNoAggregations(validated, "Aggregation functions may not be used within an update-clause");
	        }
	        if (updateSpec.OptionalWhereClause != null)
	        {
	            var validated = ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.WHERE, updateSpec.OptionalWhereClause, validationContext);
	            updateSpec.OptionalWhereClause = validated;
	            EPStatementStartMethodHelperValidate.ValidateNoAggregations(validated, "Aggregation functions may not be used within an update-clause");
	        }

	        // preprocessing view
	        var onExprView = new InternalRoutePreprocessView(streamEventType, statementContext.StatementResultService);

	        // validation
	        var routerDesc = services.InternalEventRouter.GetValidatePreprocessing(onExprView.EventType, updateSpec, statementContext.Annotations);

	        // create context factory
	        var contextFactory = new StatementAgentInstanceFactoryUpdate(statementContext, services, streamEventType, updateSpec, onExprView, routerDesc, subSelectStrategyCollection);
	        statementContext.StatementAgentInstanceFactory = contextFactory;

	        // perform start of hook-up to start
	        Viewable finalViewable;
	        EPStatementStopMethod stopStatementMethod;
	        IDictionary<ExprSubselectNode, SubSelectStrategyHolder> subselectStrategyInstances;

	        // With context - delegate instantiation to context
	        EPStatementStopMethod stopMethod = new EPStatementStopMethodImpl(statementContext, stopCallbacks);
	        if (_statementSpec.OptionalContextName != null) {

	            // use statement-wide agent-instance-specific subselects
	            var aiRegistryExpr = statementContext.StatementAgentInstanceRegistry.AgentInstanceExprService;
	            subselectStrategyInstances = new Dictionary<ExprSubselectNode, SubSelectStrategyHolder>();
	            foreach (var node in subSelectStrategyCollection.Subqueries.Keys) {
	                var specificService = aiRegistryExpr.AllocateSubselect(node);
	                node.Strategy = specificService;
	                subselectStrategyInstances.Put(node, new SubSelectStrategyHolder(null, null, null, null, null, null, null));
	            }

	            var mergeView = new ContextMergeView(onExprView.EventType);
	            finalViewable = mergeView;

	            var statement = new ContextManagedStatementOnTriggerDesc(_statementSpec, statementContext, mergeView, contextFactory);
	            services.ContextManagementService.AddStatement(_statementSpec.OptionalContextName, statement, isRecoveringResilient);
	            stopStatementMethod = new ProxyEPStatementStopMethod(() =>
	            {
	                services.ContextManagementService.StoppedStatement(contextName, statementContext.StatementName, statementContext.StatementId, statementContext.Expression, statementContext.ExceptionHandlingService);
	                stopMethod.Stop();
	            });

	            destroyCallbacks.AddCallback(new EPStatementDestroyCallbackContext(services.ContextManagementService, _statementSpec.OptionalContextName, statementContext.StatementName, statementContext.StatementId));
	        }
	        // Without context - start here
	        else
            {
	            var agentInstanceContext = GetDefaultAgentInstanceContext(statementContext);
	            var resultOfStart = (StatementAgentInstanceFactoryUpdateResult) contextFactory.NewContext(agentInstanceContext, isRecoveringResilient);
	            finalViewable = resultOfStart.FinalView;
	            var stopCallback = services.EpStatementFactory.MakeStopMethod(resultOfStart);
	            stopStatementMethod = new ProxyEPStatementStopMethod(() =>
                {
	                stopCallback.Stop();
	                stopMethod.Stop();
	            });
	            subselectStrategyInstances = resultOfStart.SubselectStrategies;

	            if (statementContext.StatementExtensionServicesContext != null && statementContext.StatementExtensionServicesContext.StmtResources != null)
                {
	                var holder = statementContext.StatementExtensionServicesContext.ExtractStatementResourceHolder(resultOfStart);
	                statementContext.StatementExtensionServicesContext.StmtResources.Unpartitioned = holder;
	                statementContext.StatementExtensionServicesContext.PostProcessStart(resultOfStart, isRecoveringResilient);
	            }
	        }

	        // assign subquery nodes
	        EPStatementStartMethodHelperAssignExpr.AssignSubqueryStrategies(subSelectStrategyCollection, subselectStrategyInstances);

	        return new EPStatementStartResult(finalViewable, stopStatementMethod, destroyCallbacks);
	    }
	}
} // end of namespace
