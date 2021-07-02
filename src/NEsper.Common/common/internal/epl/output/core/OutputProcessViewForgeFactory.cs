///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.output.condition;
using com.espertech.esper.common.@internal.epl.output.view;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.common.@internal.serde.compiletime.eventtype;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;

namespace com.espertech.esper.common.@internal.epl.output.core
{
    /// <summary>
    /// Factory for factories for output processing views.
    /// </summary>
    public class OutputProcessViewForgeFactory
    {
        public static OutputProcessViewFactoryForgeDesc Make(
            EventType[] typesPerStream,
            EventType resultEventType,
            ResultSetProcessorType resultSetProcessorType,
            StatementSpecCompiled statementSpec,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            var insertIntoDesc = statementSpec.Raw.InsertIntoDesc;
            var selectStreamSelector = statementSpec.Raw.SelectStreamSelectorEnum;
            var outputLimitSpec = statementSpec.Raw.OutputLimitSpec;
            var streamCount = statementSpec.StreamSpecs.Length;
            var isDistinct = statementSpec.Raw.SelectClauseSpec.IsDistinct;
            var isGrouped = statementSpec.GroupByExpressions != null &&
                            statementSpec.GroupByExpressions.GroupByNodes.Length > 0;
            var additionalForgeables = new List<StmtClassForgeableFactory>();

            // determine routing
            var isRouted = false;
            var routeToFront = false;
            if (insertIntoDesc != null) {
                isRouted = true;
                routeToFront = services.NamedWindowCompileTimeResolver.Resolve(insertIntoDesc.EventTypeName) != null;
            }

            OutputStrategyPostProcessForge outputStrategyPostProcessForge = null;
            if ((insertIntoDesc != null) || (selectStreamSelector == SelectClauseStreamSelectorEnum.RSTREAM_ONLY)) {
                SelectClauseStreamSelectorEnum? insertIntoStreamSelector = null;
                TableMetaData table = null;

                if (insertIntoDesc != null) {
                    insertIntoStreamSelector = insertIntoDesc.StreamSelector;
                    table = services.TableCompileTimeResolver.Resolve(statementSpec.Raw.InsertIntoDesc.EventTypeName);
                    if (table != null) {
                        EPLValidationUtil.ValidateContextName(
                            true,
                            table.TableName,
                            table.OptionalContextName,
                            statementSpec.Raw.OptionalContextName,
                            true);
                    }
                }

                var audit = AuditEnum.INSERT.GetAudit(statementSpec.Annotations) != null;
                outputStrategyPostProcessForge = new OutputStrategyPostProcessForge(
                    isRouted,
                    insertIntoStreamSelector,
                    selectStreamSelector,
                    routeToFront,
                    table,
                    audit);
            }

            var multiKeyPlan = MultiKeyPlanner.PlanMultiKeyDistinct(
                isDistinct,
                resultEventType,
                statementRawInfo,
                SerdeCompileTimeResolverNonHA.INSTANCE);
            var distinctMultiKey = multiKeyPlan.ClassRef;
            additionalForgeables.AddRange(multiKeyPlan.MultiKeyForgeables);

            OutputProcessViewFactoryForge outputProcessViewFactoryForge;
            if (outputLimitSpec == null) {
                if (!isDistinct) {
                    if (outputStrategyPostProcessForge == null || !outputStrategyPostProcessForge.HasTable) {
                        // without table we have a shortcut implementation
                        outputProcessViewFactoryForge =
                            new OutputProcessViewDirectSimpleForge(outputStrategyPostProcessForge);
                    }
                    else {
                        outputProcessViewFactoryForge =
                            new OutputProcessViewDirectForge(outputStrategyPostProcessForge);
                    }
                }
                else {
                    outputProcessViewFactoryForge = new OutputProcessViewDirectDistinctOrAfterFactoryForge(
                        outputStrategyPostProcessForge,
                        isDistinct,
                        distinctMultiKey,
                        null,
                        null,
                        resultEventType);
                }
            }
            else if (outputLimitSpec.RateType == OutputLimitRateType.AFTER) {
                outputProcessViewFactoryForge = new OutputProcessViewDirectDistinctOrAfterFactoryForge(
                    outputStrategyPostProcessForge,
                    isDistinct,
                    distinctMultiKey,
                    outputLimitSpec.AfterTimePeriodExpr,
                    outputLimitSpec.AfterNumberOfEvents,
                    resultEventType);
            }
            else {
                try {
                    var isWithHavingClause = statementSpec.Raw.HavingClause != null;
                    var isStartConditionOnCreation = HasOnlyTables(statementSpec.StreamSpecs);
                    var outputConditionFactoryForge =
                        OutputConditionFactoryFactory.CreateCondition(
                            outputLimitSpec,
                            isGrouped,
                            isWithHavingClause,
                            isStartConditionOnCreation,
                            statementRawInfo,
                            services);
                    var hasOrderBy = statementSpec.Raw.OrderByList != null && statementSpec.Raw.OrderByList.Count > 0;
                    var hasAfter = outputLimitSpec.AfterNumberOfEvents != null ||
                                   outputLimitSpec.AfterTimePeriodExpr != null;

                    // hint checking with order-by
                    var hasOptHint = ResultSetProcessorOutputConditionTypeExtensions
                        .GetOutputLimitOpt(statementSpec.Annotations, services.Configuration, hasOrderBy);
                    var conditionType =
                        ResultSetProcessorOutputConditionTypeExtensions
                            .GetConditionType(
                                outputLimitSpec.DisplayLimit,
                                resultSetProcessorType.IsAggregated(),
                                hasOrderBy,
                                hasOptHint,
                                resultSetProcessorType.IsGrouped());

                    // plan serdes
                    foreach (var eventType in typesPerStream) {
                        var serdeForgeables = SerdeEventTypeUtility.Plan(
                            eventType,
                            statementRawInfo,
                            services.SerdeEventTypeRegistry,
                            services.SerdeResolver);
                        additionalForgeables.AddRange(serdeForgeables);
                    }
                    
                    var terminable =
                        outputLimitSpec.RateType == OutputLimitRateType.TERM ||
                        outputLimitSpec.IsAndAfterTerminate;
                    
                    var changeSetStateMgmtSettings = services.StateMgmtSettingsProvider.GetResultSet(statementRawInfo, AppliesTo.RESULTSET_OUTPUTLIMIT);
                    outputProcessViewFactoryForge = new OutputProcessViewConditionForge(
                        outputStrategyPostProcessForge,
                        isDistinct,
                        distinctMultiKey,
                        outputLimitSpec.AfterTimePeriodExpr,
                        outputLimitSpec.AfterNumberOfEvents,
                        outputConditionFactoryForge,
                        streamCount,
                        conditionType,
                        terminable,
                        hasAfter,
                        resultSetProcessorType.IsUnaggregatedUngrouped(),
                        selectStreamSelector,
                        typesPerStream,
                        resultEventType,
                        changeSetStateMgmtSettings);
                }
                catch (Exception ex) {
                    throw new ExprValidationException("Failed to validate the output rate limiting clause: " + ex.Message, ex);
                }
            }
            
            return new OutputProcessViewFactoryForgeDesc(outputProcessViewFactoryForge, additionalForgeables);
        }

        public static OutputProcessViewFactoryForge Make()
        {
            return null;
        }

        private static bool HasOnlyTables(StreamSpecCompiled[] streamSpecs)
        {
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