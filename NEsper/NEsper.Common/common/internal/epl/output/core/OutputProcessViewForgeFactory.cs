///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.output.condition;
using com.espertech.esper.common.@internal.epl.output.view;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.util;

namespace com.espertech.esper.common.@internal.epl.output.core
{
    /// <summary>
    /// Factory for factories for output processing views.
    /// </summary>
    public class OutputProcessViewForgeFactory
    {
        public static OutputProcessViewFactoryForge Make(
            EventType[] typesPerStream,
            EventType resultEventType,
            ResultSetProcessorType resultSetProcessorType,
            StatementSpecCompiled statementSpec,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            InsertIntoDesc insertIntoDesc = statementSpec.Raw.InsertIntoDesc;
            SelectClauseStreamSelectorEnum selectStreamSelector = statementSpec.Raw.SelectStreamSelectorEnum;
            OutputLimitSpec outputLimitSpec = statementSpec.Raw.OutputLimitSpec;
            int streamCount = statementSpec.StreamSpecs.Length;
            bool isDistinct = statementSpec.Raw.SelectClauseSpec.IsDistinct;
            bool isGrouped = statementSpec.GroupByExpressions != null &&
                             statementSpec.GroupByExpressions.GroupByNodes.Length > 0;

            // determine routing
            bool isRouted = false;
            bool routeToFront = false;
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

                bool audit = AuditEnum.INSERT.GetAudit(statementSpec.Annotations) != null;
                outputStrategyPostProcessForge = new OutputStrategyPostProcessForge(
                    isRouted,
                    insertIntoStreamSelector,
                    selectStreamSelector,
                    routeToFront,
                    table,
                    audit);
            }

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
                        null,
                        null,
                        resultEventType);
                }
            }
            else if (outputLimitSpec.RateType == OutputLimitRateType.AFTER) {
                outputProcessViewFactoryForge = new OutputProcessViewDirectDistinctOrAfterFactoryForge(
                    outputStrategyPostProcessForge,
                    isDistinct,
                    outputLimitSpec.AfterTimePeriodExpr,
                    outputLimitSpec.AfterNumberOfEvents,
                    resultEventType);
            }
            else {
                try {
                    bool isWithHavingClause = statementSpec.Raw.HavingClause != null;
                    bool isStartConditionOnCreation = HasOnlyTables(statementSpec.StreamSpecs);
                    OutputConditionFactoryForge outputConditionFactoryForge =
                        OutputConditionFactoryFactory.CreateCondition(
                            outputLimitSpec,
                            isGrouped,
                            isWithHavingClause,
                            isStartConditionOnCreation,
                            statementRawInfo,
                            services);
                    bool hasOrderBy = statementSpec.Raw.OrderByList != null && statementSpec.Raw.OrderByList.Count > 0;
                    bool hasAfter = outputLimitSpec.AfterNumberOfEvents != null ||
                                    outputLimitSpec.AfterTimePeriodExpr != null;

                    // hint checking with order-by
                    bool hasOptHint = ResultSetProcessorOutputConditionTypeExtensions
                        .GetOutputLimitOpt(statementSpec.Annotations, services.Configuration, hasOrderBy);
                    ResultSetProcessorOutputConditionType conditionType =
                        ResultSetProcessorOutputConditionTypeExtensions
                            .GetConditionType(
                                outputLimitSpec.DisplayLimit,
                                resultSetProcessorType.IsAggregated(),
                                hasOrderBy,
                                hasOptHint,
                                resultSetProcessorType.IsGrouped());

                    bool terminable = outputLimitSpec.RateType == OutputLimitRateType.TERM ||
                                      outputLimitSpec.IsAndAfterTerminate;
                    outputProcessViewFactoryForge = new OutputProcessViewConditionForge(
                        outputStrategyPostProcessForge,
                        isDistinct,
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
                        resultEventType);
                }
                catch (Exception ex) {
                    throw new ExprValidationException("Error in the output rate limiting clause: " + ex.Message, ex);
                }
            }

            return outputProcessViewFactoryForge;
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

            foreach (StreamSpecCompiled streamSpec in streamSpecs) {
                if (!(streamSpec is TableQueryStreamSpec)) {
                    return false;
                }
            }

            return true;
        }
    }
} // end of namespace