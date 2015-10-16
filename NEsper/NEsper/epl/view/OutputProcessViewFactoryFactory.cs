///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.util;

namespace com.espertech.esper.epl.view
{
	/// <summary>
	/// Factory for factories for output processing views.
	/// </summary>
	public class OutputProcessViewFactoryFactory
	{
	    public static OutputProcessViewFactory Make(StatementSpecCompiled statementSpec, InternalEventRouter internalEventRouter, StatementContext statementContext, EventType resultEventType, OutputProcessViewCallback optionalOutputProcessViewCallback, TableService tableService, ResultSetProcessorType resultSetProcessorType)
	    {
	        // determine direct-callback
	        if (optionalOutputProcessViewCallback != null) {
	            return new OutputProcessViewFactoryCallback(optionalOutputProcessViewCallback);
	        }

	        // determine routing
	        var isRouted = false;
	        var routeToFront = false;
	        if (statementSpec.InsertIntoDesc != null)
	        {
	            isRouted = true;
	            routeToFront = statementContext.NamedWindowService.IsNamedWindow(statementSpec.InsertIntoDesc.EventTypeName);
	        }

	        OutputStrategyPostProcessFactory outputStrategyPostProcessFactory = null;
	        if ((statementSpec.InsertIntoDesc != null) || (statementSpec.SelectStreamSelectorEnum == SelectClauseStreamSelectorEnum.RSTREAM_ONLY))
	        {
	            SelectClauseStreamSelectorEnum? insertIntoStreamSelector = null;
	            string tableName = null;

	            if (statementSpec.InsertIntoDesc != null) {
	                insertIntoStreamSelector = statementSpec.InsertIntoDesc.StreamSelector;
	                var tableMetadata = tableService.GetTableMetadata(statementSpec.InsertIntoDesc.EventTypeName);
	                if (tableMetadata != null) {
	                    tableName = tableMetadata.TableName;
	                    EPLValidationUtil.ValidateContextName(true, tableName, tableMetadata.ContextName, statementSpec.OptionalContextName, true);
	                }
	            }

	            outputStrategyPostProcessFactory = new OutputStrategyPostProcessFactory(isRouted, insertIntoStreamSelector, statementSpec.SelectStreamSelectorEnum, internalEventRouter, statementContext.EpStatementHandle, routeToFront, tableService, tableName);
	        }

	        // Do we need to enforce an output policy?
	        var streamCount = statementSpec.StreamSpecs.Length;
	        var outputLimitSpec = statementSpec.OutputLimitSpec;
	        var isDistinct = statementSpec.SelectClauseSpec.IsDistinct;
	        var isGrouped = statementSpec.GroupByExpressions != null && statementSpec.GroupByExpressions.GroupByNodes.Length > 0;

	        if (outputLimitSpec != null) {
	            var evaluatorContextStmt = new ExprEvaluatorContextStatement(statementContext, false);
	            var validationContext =
	                new ExprValidationContext(
	                    new StreamTypeServiceImpl(statementContext.EngineURI, false),
	                    statementContext.MethodResolutionService, null, 
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
                        false, false, false, false, null, false);
	            if (outputLimitSpec.AfterTimePeriodExpr != null) {
	                var timePeriodExpr = (ExprTimePeriod) ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.OUTPUTLIMIT, outputLimitSpec.AfterTimePeriodExpr, validationContext);
	                outputLimitSpec.AfterTimePeriodExpr = timePeriodExpr;
	            }
	            if (outputLimitSpec.TimePeriodExpr != null) {
	                var timePeriodExpr = (ExprTimePeriod) ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.OUTPUTLIMIT, outputLimitSpec.TimePeriodExpr, validationContext);
	                outputLimitSpec.TimePeriodExpr = timePeriodExpr;
	                if (timePeriodExpr.IsConstantResult && timePeriodExpr.EvaluateAsSeconds(null, true, new ExprEvaluatorContextStatement(statementContext, false)) <= 0) {
	                    throw new ExprValidationException("Invalid time period expression returns a zero or negative time interval");
	                }
	            }
	        }

	        OutputProcessViewFactory outputProcessViewFactory;
	        if (outputLimitSpec == null)
	        {
	            if (!isDistinct)
	            {
	                outputProcessViewFactory = new OutputProcessViewDirectFactory(statementContext, outputStrategyPostProcessFactory);
	            }
	            else
	            {
	                outputProcessViewFactory = new OutputProcessViewDirectDistinctOrAfterFactory(statementContext, outputStrategyPostProcessFactory, isDistinct, null, null, resultEventType);
	            }
	        }
	        else if (outputLimitSpec.RateType == OutputLimitRateType.AFTER)
	        {
	            outputProcessViewFactory = new OutputProcessViewDirectDistinctOrAfterFactory(statementContext, outputStrategyPostProcessFactory, isDistinct, outputLimitSpec.AfterTimePeriodExpr, outputLimitSpec.AfterNumberOfEvents, resultEventType);
	        }
	        else
	        {
	            try {
	                var isWithHavingClause = statementSpec.HavingExprRootNode != null;
	                var isStartConditionOnCreation = HasOnlyTables(statementSpec.StreamSpecs);
	                var outputConditionFactory = OutputConditionFactoryFactory.CreateCondition(outputLimitSpec, statementContext, isGrouped, isWithHavingClause, isStartConditionOnCreation);
                    var hasOrderBy = statementSpec.OrderByList != null && statementSpec.OrderByList.Length > 0;

	                OutputProcessViewConditionFactory.ConditionType conditionType;

                    var hasAfter = outputLimitSpec.AfterNumberOfEvents != null || outputLimitSpec.AfterTimePeriodExpr != null;
                    var isUnaggregatedUngrouped = resultSetProcessorType == ResultSetProcessorType.HANDTHROUGH || resultSetProcessorType == ResultSetProcessorType.UNAGGREGATED_UNGROUPED;

                    // hint checking with order-by
                    var hasOptHint = HintEnum.ENABLE_OUTPUTLIMIT_OPT.GetHint(statementSpec.Annotations) != null;
                    if (hasOptHint && hasOrderBy) {
                        throw new ExprValidationException("The " + HintEnum.ENABLE_OUTPUTLIMIT_OPT + " hint is not supported with order-by");
                    }

                    if (outputLimitSpec.DisplayLimit == OutputLimitLimitType.SNAPSHOT)
	                {
	                    conditionType = OutputProcessViewConditionFactory.ConditionType.SNAPSHOT;
	                }
	                // For FIRST without groups we are using a special logic that integrates the first-flag, in order to still conveniently use all sorts of output conditions.
	                // FIRST with group-by is handled by setting the output condition to null (OutputConditionNull) and letting the ResultSetProcessor handle first-per-group.
	                // Without having-clause there is no required order of processing, thus also use regular policy.
	                else if (outputLimitSpec.DisplayLimit == OutputLimitLimitType.FIRST && statementSpec.GroupByExpressions == null)
                    {
	                    conditionType = OutputProcessViewConditionFactory.ConditionType.POLICY_FIRST;
	                }
                    else if (isUnaggregatedUngrouped && outputLimitSpec.DisplayLimit == OutputLimitLimitType.LAST)
                    {
                        conditionType = OutputProcessViewConditionFactory.ConditionType.POLICY_LASTALL_UNORDERED;
                    }
                    else if (hasOptHint && outputLimitSpec.DisplayLimit == OutputLimitLimitType.ALL && !hasOrderBy)
                    {
                        conditionType = OutputProcessViewConditionFactory.ConditionType.POLICY_LASTALL_UNORDERED;
                    }
                    else if (hasOptHint && outputLimitSpec.DisplayLimit == OutputLimitLimitType.LAST && !hasOrderBy)
                    {
                        conditionType = OutputProcessViewConditionFactory.ConditionType.POLICY_LASTALL_UNORDERED;
                    }
	                else
	                {
	                    conditionType = OutputProcessViewConditionFactory.ConditionType.POLICY_NONFIRST;
	                }

                    var selectClauseStreamSelectorEnum = statementSpec.SelectStreamSelectorEnum;
	                var terminable = outputLimitSpec.RateType == OutputLimitRateType.TERM || outputLimitSpec.IsAndAfterTerminate;
                    outputProcessViewFactory = new OutputProcessViewConditionFactory(statementContext, outputStrategyPostProcessFactory, isDistinct, outputLimitSpec.AfterTimePeriodExpr, outputLimitSpec.AfterNumberOfEvents, resultEventType, outputConditionFactory, streamCount, conditionType, outputLimitSpec.DisplayLimit, terminable, hasAfter, isUnaggregatedUngrouped, selectClauseStreamSelectorEnum);
	            }
	            catch (Exception ex) {
	                throw new ExprValidationException("Error in the output rate limiting clause: " + ex.Message, ex);
	            }
	        }

	        return outputProcessViewFactory;
	    }

	    private static bool HasOnlyTables(StreamSpecCompiled[] streamSpecs) {
	        if (streamSpecs.Length == 0) {
	            return false;
	        }
	        foreach (var streamSpec in streamSpecs) {
	            if (!(streamSpec is TableQueryStreamSpec)) {
	                return false;
	            }
	        }
	        return true;
	    }
	}
} // end of namespace
