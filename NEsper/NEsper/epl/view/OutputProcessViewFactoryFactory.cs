///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
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
	    public static OutputProcessViewFactory Make(StatementSpecCompiled statementSpec, InternalEventRouter internalEventRouter, StatementContext statementContext, EventType resultEventType, OutputProcessViewCallback optionalOutputProcessViewCallback, TableService tableService, ResultSetProcessorType resultSetProcessorType, ResultSetProcessorHelperFactory resultSetProcessorHelperFactory, StatementVariableRef statementVariableRef)
	    {
	        // determine direct-callback
	        if (optionalOutputProcessViewCallback != null) {
	            return new OutputProcessViewFactoryCallback(optionalOutputProcessViewCallback);
	        }

	        // determine routing
	        bool isRouted = false;
	        bool routeToFront = false;
	        if (statementSpec.InsertIntoDesc != null)
	        {
	            isRouted = true;
	            routeToFront = statementContext.NamedWindowMgmtService.IsNamedWindow(statementSpec.InsertIntoDesc.EventTypeName);
	        }

	        OutputStrategyPostProcessFactory outputStrategyPostProcessFactory = null;
	        if ((statementSpec.InsertIntoDesc != null) || (statementSpec.SelectStreamSelectorEnum == SelectClauseStreamSelectorEnum.RSTREAM_ONLY))
	        {
	            SelectClauseStreamSelectorEnum? insertIntoStreamSelector = null;
	            string tableName = null;

	            if (statementSpec.InsertIntoDesc != null) {
	                insertIntoStreamSelector = statementSpec.InsertIntoDesc.StreamSelector;
	                TableMetadata tableMetadata = tableService.GetTableMetadata(statementSpec.InsertIntoDesc.EventTypeName);
	                if (tableMetadata != null) {
	                    tableName = tableMetadata.TableName;
	                    EPLValidationUtil.ValidateContextName(true, tableName, tableMetadata.ContextName, statementSpec.OptionalContextName, true);
	                    statementVariableRef.AddReferences(statementContext.StatementName, tableMetadata.TableName);
	                }
	            }

	            outputStrategyPostProcessFactory = new OutputStrategyPostProcessFactory(isRouted, insertIntoStreamSelector, statementSpec.SelectStreamSelectorEnum, internalEventRouter, statementContext.EpStatementHandle, routeToFront, tableService, tableName);
	        }

	        // Do we need to enforce an output policy?
	        int streamCount = statementSpec.StreamSpecs.Length;
	        OutputLimitSpec outputLimitSpec = statementSpec.OutputLimitSpec;
	        bool isDistinct = statementSpec.SelectClauseSpec.IsDistinct;
	        bool isGrouped = statementSpec.GroupByExpressions != null && statementSpec.GroupByExpressions.GroupByNodes.Length > 0;

	        OutputProcessViewFactory outputProcessViewFactory;
	        if (outputLimitSpec == null)
	        {
	            if (!isDistinct)
	            {
	                outputProcessViewFactory = new OutputProcessViewDirectFactory(statementContext, outputStrategyPostProcessFactory, resultSetProcessorHelperFactory);
	            }
	            else
	            {
	                outputProcessViewFactory = new OutputProcessViewDirectDistinctOrAfterFactory(statementContext, outputStrategyPostProcessFactory, resultSetProcessorHelperFactory, isDistinct, null, null, resultEventType);
	            }
	        }
	        else if (outputLimitSpec.RateType == OutputLimitRateType.AFTER)
	        {
	            outputProcessViewFactory = new OutputProcessViewDirectDistinctOrAfterFactory(statementContext, outputStrategyPostProcessFactory, resultSetProcessorHelperFactory, isDistinct, outputLimitSpec.AfterTimePeriodExpr, outputLimitSpec.AfterNumberOfEvents, resultEventType);
	        }
	        else
	        {
	            try {
	                bool isWithHavingClause = statementSpec.HavingExprRootNode != null;
	                bool isStartConditionOnCreation = HasOnlyTables(statementSpec.StreamSpecs);
	                OutputConditionFactory outputConditionFactory = OutputConditionFactoryFactory.CreateCondition(outputLimitSpec, statementContext, isGrouped, isWithHavingClause, isStartConditionOnCreation, resultSetProcessorHelperFactory);
	                bool hasOrderBy = statementSpec.OrderByList != null && statementSpec.OrderByList.Length > 0;
	                OutputProcessViewConditionFactory.ConditionType conditionType;
	                bool hasAfter = outputLimitSpec.AfterNumberOfEvents != null || outputLimitSpec.AfterTimePeriodExpr != null;
	                bool isUnaggregatedUngrouped = resultSetProcessorType == ResultSetProcessorType.HANDTHROUGH || resultSetProcessorType == ResultSetProcessorType.UNAGGREGATED_UNGROUPED;

	                // hint checking with order-by
	                bool hasOptHint = HintEnum.ENABLE_OUTPUTLIMIT_OPT.GetHint(statementSpec.Annotations) != null;
	                if (hasOptHint && hasOrderBy) {
	                    throw new ExprValidationException("The " + HintEnum.ENABLE_OUTPUTLIMIT_OPT + " hint is not supported with order-by");
	                }

	                if (outputLimitSpec.DisplayLimit == OutputLimitLimitType.SNAPSHOT) {
	                    conditionType = OutputProcessViewConditionFactory.ConditionType.SNAPSHOT;
	                }
	                // For FIRST without groups we are using a special logic that integrates the first-flag, in order to still conveniently use all sorts of output conditions.
	                // FIRST with group-by is handled by setting the output condition to null (OutputConditionNull) and letting the ResultSetProcessor handle first-per-group.
	                // Without having-clause there is no required order of processing, thus also use regular policy.
	                else if (outputLimitSpec.DisplayLimit == OutputLimitLimitType.FIRST && statementSpec.GroupByExpressions == null) {
	                    conditionType = OutputProcessViewConditionFactory.ConditionType.POLICY_FIRST;
	                }
	                else if (isUnaggregatedUngrouped && outputLimitSpec.DisplayLimit == OutputLimitLimitType.LAST) {
	                    conditionType = OutputProcessViewConditionFactory.ConditionType.POLICY_LASTALL_UNORDERED;
	                }
	                else if (hasOptHint && outputLimitSpec.DisplayLimit == OutputLimitLimitType.ALL && !hasOrderBy) {
	                    conditionType = OutputProcessViewConditionFactory.ConditionType.POLICY_LASTALL_UNORDERED;
	                }
	                else if (hasOptHint && outputLimitSpec.DisplayLimit == OutputLimitLimitType.LAST && !hasOrderBy) {
	                    conditionType = OutputProcessViewConditionFactory.ConditionType.POLICY_LASTALL_UNORDERED;
	                }
	                else {
	                    conditionType = OutputProcessViewConditionFactory.ConditionType.POLICY_NONFIRST;
	                }

	                SelectClauseStreamSelectorEnum selectClauseStreamSelectorEnum = statementSpec.SelectStreamSelectorEnum;
	                bool terminable = outputLimitSpec.RateType == OutputLimitRateType.TERM || outputLimitSpec.IsAndAfterTerminate;
	                outputProcessViewFactory = new OutputProcessViewConditionFactory(statementContext, outputStrategyPostProcessFactory, isDistinct, outputLimitSpec.AfterTimePeriodExpr, outputLimitSpec.AfterNumberOfEvents, resultEventType, outputConditionFactory, streamCount, conditionType, outputLimitSpec.DisplayLimit, terminable, hasAfter, isUnaggregatedUngrouped, selectClauseStreamSelectorEnum, resultSetProcessorHelperFactory);
	            }
	            catch (Exception ex) {
	                throw new ExprValidationException("Error in the output rate limiting clause: " + ex.Message, ex);
	            }
	        }

	        return outputProcessViewFactory;
	    }

	    private static bool HasOnlyTables(StreamSpecCompiled[] streamSpecs)
        {
	        if (streamSpecs.Length == 0)
            {
	            return false;
	        }

	        foreach (StreamSpecCompiled streamSpec in streamSpecs) {
	            if (!(streamSpec is TableQueryStreamSpec)) {
	                return false;
	            }
	        }
	        return true;
	    }
	}
} // end of namespace
