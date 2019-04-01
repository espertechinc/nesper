///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.collection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.ontrigger.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.subquery;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.resultset.@select.core;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.epl.subselect;
using com.espertech.esper.common.@internal.epl.table.strategy;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.statement.helper;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.aifactory.ontrigger.ontrigger
{
    public class OnTriggerPlanValidator
    {
        public const string INITIAL_VALUE_STREAM_NAME = "initial";

        public static OnTriggerPlanValidationResult ValidateOnTriggerPlan(
            EventType namedWindowOrTableType,
            OnTriggerWindowDesc onTriggerDesc,
            StreamSpecCompiled streamSpec,
            OnTriggerActivatorDesc activatorResult,
            IDictionary<ExprSubselectNode, SubSelectActivationPlan> subselectActivation,
            StatementBaseInfo @base,
            StatementCompileTimeServices services)
        {
            var zeroStreamAliasName = onTriggerDesc.OptionalAsName;
            if (zeroStreamAliasName == null) {
                zeroStreamAliasName = "stream_0";
            }

            var streamName = streamSpec.OptionalStreamName;
            if (streamName == null) {
                streamName = "stream_1";
            }

            var namedWindowTypeName = onTriggerDesc.WindowName;

            // Materialize sub-select views
            // 0 - named window stream
            // 1 - arriving stream
            // 2 - initial value before update
            string[] subselectStreamNames = {zeroStreamAliasName, streamSpec.OptionalStreamName};
            EventType[] subselectEventTypes = {namedWindowOrTableType, activatorResult.ActivatorResultEventType};
            string[] subselectEventTypeNames = {namedWindowTypeName, activatorResult.TriggerEventTypeName};
            var subselectForges = SubSelectHelperForgePlanner.PlanSubSelect(
                @base, subselectActivation, subselectStreamNames, subselectEventTypes, subselectEventTypeNames,
                services);

            var typeService = new StreamTypeServiceImpl(
                new[] {namedWindowOrTableType, activatorResult.ActivatorResultEventType},
                new[] {zeroStreamAliasName, streamName}, new[] {false, true}, true, false);

            // allow "initial" as a prefix to properties
            StreamTypeServiceImpl assignmentTypeService;
            if (zeroStreamAliasName.Equals(INITIAL_VALUE_STREAM_NAME) || streamName.Equals(INITIAL_VALUE_STREAM_NAME)) {
                assignmentTypeService = typeService;
            }
            else {
                assignmentTypeService = new StreamTypeServiceImpl(
                    new[] {namedWindowOrTableType, activatorResult.ActivatorResultEventType, namedWindowOrTableType},
                    new[] {zeroStreamAliasName, streamName, INITIAL_VALUE_STREAM_NAME}, new[] {false, true, true},
                    false, false);
                assignmentTypeService.IsStreamZeroUnambigous = true;
            }

            if (onTriggerDesc is OnTriggerWindowUpdateDesc) {
                var updateDesc = (OnTriggerWindowUpdateDesc) onTriggerDesc;
                var validationContext = new ExprValidationContextBuilder(
                        assignmentTypeService, @base.StatementRawInfo, services)
                    .WithAllowBindingConsumption(true).Build();
                foreach (var assignment in updateDesc.Assignments) {
                    var validated = ExprNodeUtilityValidate.GetValidatedAssignment(assignment, validationContext);
                    assignment.Expression = validated;
                    EPStatementStartMethodHelperValidate.ValidateNoAggregations(
                        validated, "Aggregation functions may not be used within an on-update-clause");
                }
            }

            if (onTriggerDesc is OnTriggerMergeDesc) {
                var mergeDesc = (OnTriggerMergeDesc) onTriggerDesc;
                ValidateMergeDesc(
                    mergeDesc, namedWindowOrTableType, zeroStreamAliasName, activatorResult.ActivatorResultEventType,
                    streamName, @base.StatementRawInfo, services);
            }

            // validate join expression
            var validatedJoin = ValidateJoinNamedWindow(
                ExprNodeOrigin.WHERE, @base.StatementSpec.Raw.WhereClause,
                namedWindowOrTableType, zeroStreamAliasName, namedWindowTypeName,
                activatorResult.ActivatorResultEventType, streamName, activatorResult.TriggerEventTypeName,
                null, @base.StatementRawInfo, services);

            // validate filter, output rate limiting
            EPStatementStartMethodHelperValidate.ValidateNodes(
                @base.StatementSpec.Raw, typeService, null, @base.StatementRawInfo, services);

            // Construct a processor for results; for use in on-select to process selection results
            // Use a wildcard select if the select-clause is empty, such as for on-delete.
            // For on-select the select clause is not empty.
            if (@base.StatementSpec.SelectClauseCompiled.SelectExprList.Length == 0) {
                @base.StatementSpec.SelectClauseCompiled.WithSelectExprList(new SelectClauseElementWildcard());
            }

            var resultSetProcessorPrototype = ResultSetProcessorFactoryFactory.GetProcessorPrototype(
                new ResultSetSpec(@base.StatementSpec),
                typeService, null, new bool[0], true, @base.ContextPropertyRegistry, false, true,
                @base.StatementRawInfo, services);

            // plan table access
            var tableAccessForges = ExprTableEvalHelperPlan.PlanTableAccess(@base.StatementSpec.TableAccessNodes);

            return new OnTriggerPlanValidationResult(
                subselectForges, tableAccessForges, resultSetProcessorPrototype, validatedJoin, zeroStreamAliasName);
        }

        protected internal static ExprNode ValidateJoinNamedWindow(
            ExprNodeOrigin exprNodeOrigin,
            ExprNode deleteJoinExpr,
            EventType namedWindowType,
            string namedWindowStreamName,
            string namedWindowName,
            EventType filteredType,
            string filterStreamName,
            string filteredTypeName,
            string optionalTableName,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices
        )
        {
            if (deleteJoinExpr == null) {
                return null;
            }

            var namesAndTypes = new LinkedHashMap<string, Pair<EventType, string>>();
            namesAndTypes.Put(namedWindowStreamName, new Pair<EventType, string>(namedWindowType, namedWindowName));
            namesAndTypes.Put(filterStreamName, new Pair<EventType, string>(filteredType, filteredTypeName));
            StreamTypeService typeService = new StreamTypeServiceImpl(namesAndTypes, false, false);

            var validationContext = new ExprValidationContextBuilder(typeService, statementRawInfo, compileTimeServices)
                .WithAllowBindingConsumption(true).Build();
            return ExprNodeUtilityValidate.GetValidatedSubtree(exprNodeOrigin, deleteJoinExpr, validationContext);
        }

        private static void ValidateMergeDesc(
            OnTriggerMergeDesc mergeDesc, EventType namedWindowType, string namedWindowName,
            EventType triggerStreamType, string triggerStreamName, StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            var exprNodeErrorMessage = "Aggregation functions may not be used within an merge-clause";

            var dummyTypeNoPropertiesMeta = new EventTypeMetadata(
                "merge_named_window_insert", statementRawInfo.ModuleName, EventTypeTypeClass.STREAM,
                EventTypeApplicationType.MAP, NameAccessModifier.TRANSIENT, EventTypeBusModifier.NONBUS, false,
                EventTypeIdPair.Unassigned());
            EventType dummyTypeNoProperties = BaseNestableEventUtil.MakeMapTypeCompileTime(
                dummyTypeNoPropertiesMeta, Collections.GetEmptyMap<string, object>(), null, null, null, null,
                services.BeanEventTypeFactoryPrivate, services.EventTypeCompileTimeResolver);
            StreamTypeService insertOnlyTypeSvc = new StreamTypeServiceImpl(
                new[] {dummyTypeNoProperties, triggerStreamType},
                new[] {UuidGenerator.Generate(), triggerStreamName}, new[] {true, true}, true, false);
            var twoStreamTypeSvc = new StreamTypeServiceImpl(
                new[] {namedWindowType, triggerStreamType},
                new[] {namedWindowName, triggerStreamName}, new[] {true, true}, true, false);

            foreach (var matchedItem in mergeDesc.Items) {
                // we may provide an additional stream "initial" for the prior value, unless already defined
                StreamTypeServiceImpl assignmentStreamTypeSvc;
                if (namedWindowName.Equals(INITIAL_VALUE_STREAM_NAME) ||
                    triggerStreamName.Equals(INITIAL_VALUE_STREAM_NAME)) {
                    assignmentStreamTypeSvc = twoStreamTypeSvc;
                }
                else {
                    assignmentStreamTypeSvc = new StreamTypeServiceImpl(
                        new[] {namedWindowType, triggerStreamType, namedWindowType},
                        new[] {namedWindowName, triggerStreamName, INITIAL_VALUE_STREAM_NAME}, new[] {true, true, true},
                        false, false);
                    assignmentStreamTypeSvc.IsStreamZeroUnambigous = true;
                }

                if (matchedItem.OptionalMatchCond != null) {
                    var matchValidStreams = matchedItem.IsMatchedUnmatched ? twoStreamTypeSvc : insertOnlyTypeSvc;
                    matchedItem.OptionalMatchCond = EPStatementStartMethodHelperValidate.ValidateExprNoAgg(
                        ExprNodeOrigin.MERGEMATCHCOND, matchedItem.OptionalMatchCond, matchValidStreams,
                        exprNodeErrorMessage, true, statementRawInfo, services);
                    if (!matchedItem.IsMatchedUnmatched) {
                        EPStatementStartMethodHelperValidate.ValidateSubqueryExcludeOuterStream(
                            matchedItem.OptionalMatchCond);
                    }
                }

                foreach (var item in matchedItem.Actions) {
                    if (item is OnTriggerMergeActionDelete) {
                        var delete = (OnTriggerMergeActionDelete) item;
                        if (delete.OptionalWhereClause != null) {
                            delete.OptionalWhereClause = EPStatementStartMethodHelperValidate.ValidateExprNoAgg(
                                ExprNodeOrigin.MERGEMATCHWHERE, delete.OptionalWhereClause, twoStreamTypeSvc,
                                exprNodeErrorMessage, true, statementRawInfo, services);
                        }
                    }
                    else if (item is OnTriggerMergeActionUpdate) {
                        var update = (OnTriggerMergeActionUpdate) item;
                        if (update.OptionalWhereClause != null) {
                            update.OptionalWhereClause = EPStatementStartMethodHelperValidate.ValidateExprNoAgg(
                                ExprNodeOrigin.MERGEMATCHWHERE, update.OptionalWhereClause, twoStreamTypeSvc,
                                exprNodeErrorMessage, true, statementRawInfo, services);
                        }

                        foreach (var assignment in update.Assignments) {
                            assignment.Expression = EPStatementStartMethodHelperValidate.ValidateExprNoAgg(
                                ExprNodeOrigin.UPDATEASSIGN, assignment.Expression, assignmentStreamTypeSvc,
                                exprNodeErrorMessage, true, statementRawInfo, services);
                        }
                    }
                    else if (item is OnTriggerMergeActionInsert) {
                        var insert = (OnTriggerMergeActionInsert) item;

                        var insertTypeSvc = GetInsertStreamService(
                            insert.OptionalStreamName, namedWindowName, insertOnlyTypeSvc, twoStreamTypeSvc);

                        if (insert.OptionalWhereClause != null) {
                            insert.OptionalWhereClause = EPStatementStartMethodHelperValidate.ValidateExprNoAgg(
                                ExprNodeOrigin.MERGEMATCHWHERE, insert.OptionalWhereClause, insertTypeSvc,
                                exprNodeErrorMessage, true, statementRawInfo, services);
                        }

                        var compiledSelect = ValidateInsertSelect(
                            insert.SelectClause, insertTypeSvc, insert.Columns, statementRawInfo, services);
                        insert.SelectClauseCompiled = compiledSelect;
                    }
                    else {
                        throw new ArgumentException("Unrecognized merge item '" + item.GetType().Name + "'");
                    }
                }
            }

            if (mergeDesc.OptionalInsertNoMatch != null) {
                var insertTypeSvc = GetInsertStreamService(
                    mergeDesc.OptionalInsertNoMatch.OptionalStreamName, namedWindowName, insertOnlyTypeSvc,
                    twoStreamTypeSvc);
                var compiledSelect = ValidateInsertSelect(
                    mergeDesc.OptionalInsertNoMatch.SelectClause, insertTypeSvc,
                    mergeDesc.OptionalInsertNoMatch.Columns, statementRawInfo, services);
                mergeDesc.OptionalInsertNoMatch.SelectClauseCompiled = compiledSelect;
            }
        }

        private static StreamTypeService GetInsertStreamService(
            string optionalStreamName, string namedWindowName, StreamTypeService insertOnlyTypeSvc,
            StreamTypeServiceImpl twoStreamTypeSvc)
        {
            if (optionalStreamName == null || optionalStreamName.Equals(
                    namedWindowName, StringComparison.InvariantCultureIgnoreCase)) {
                // if no name was provided in "insert into NAME" or the name is the named window we use the empty type in the first column
                return insertOnlyTypeSvc;
            }

            return twoStreamTypeSvc;
        }

        private static IList<SelectClauseElementCompiled> ValidateInsertSelect(
            IList<SelectClauseElementRaw> selectClause, StreamTypeService insertTypeSvc, IList<string> insertColumns,
            StatementRawInfo statementRawInfo, StatementCompileTimeServices services)
        {
            var colIndex = 0;
            IList<SelectClauseElementCompiled> compiledSelect = new List<SelectClauseElementCompiled>();
            foreach (var raw in selectClause) {
                if (raw is SelectClauseStreamRawSpec) {
                    var rawStreamSpec = (SelectClauseStreamRawSpec) raw;
                    int? foundStreamNum = null;
                    for (var s = 0; s < insertTypeSvc.StreamNames.Length; s++) {
                        if (rawStreamSpec.StreamName.Equals(insertTypeSvc.StreamNames[s])) {
                            foundStreamNum = s;
                            break;
                        }
                    }

                    if (foundStreamNum == null) {
                        throw new ExprValidationException(
                            "Stream by name '" + rawStreamSpec.StreamName + "' was not found");
                    }

                    var streamSelectSpec = new SelectClauseStreamCompiledSpec(
                        rawStreamSpec.StreamName, rawStreamSpec.OptionalAsName);
                    streamSelectSpec.StreamNumber = foundStreamNum.Value;
                    compiledSelect.Add(streamSelectSpec);
                }
                else if (raw is SelectClauseExprRawSpec) {
                    var exprSpec = (SelectClauseExprRawSpec) raw;
                    var validationContext = new ExprValidationContextBuilder(insertTypeSvc, statementRawInfo, services)
                        .WithAllowBindingConsumption(true).Build();
                    var exprCompiled = ExprNodeUtilityValidate.GetValidatedSubtree(
                        ExprNodeOrigin.SELECT, exprSpec.SelectExpression, validationContext);
                    var resultName = exprSpec.OptionalAsName;
                    if (resultName == null) {
                        if (insertColumns.Count > colIndex) {
                            resultName = insertColumns[colIndex];
                        }
                        else {
                            resultName = ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(exprCompiled);
                        }
                    }

                    compiledSelect.Add(
                        new SelectClauseExprCompiledSpec(
                            exprCompiled, resultName, exprSpec.OptionalAsName, exprSpec.IsEvents));
                    EPStatementStartMethodHelperValidate.ValidateNoAggregations(
                        exprCompiled, "Expression in a merge-selection may not utilize aggregation functions");
                }
                else if (raw is SelectClauseElementWildcard) {
                    compiledSelect.Add(new SelectClauseElementWildcard());
                }
                else {
                    throw new IllegalStateException("Unknown select clause item:" + raw);
                }

                colIndex++;
            }

            return compiledSelect;
        }
    }
} // end of namespace