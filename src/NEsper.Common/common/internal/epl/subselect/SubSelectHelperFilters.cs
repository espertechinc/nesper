///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.subquery;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.common.@internal.statement.helper;
using com.espertech.esper.common.@internal.view.access;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.subselect
{
    public class SubSelectHelperFilters
    {
        public static IList<StmtClassForgeableFactory> HandleSubselectSelectClauses(
            ExprSubselectNode subselect,
            EventType outerEventType,
            string outerEventTypeName,
            string outerStreamName,
            IDictionary<string, Pair<EventType, string>> taggedEventTypes,
            IDictionary<string, Pair<EventType, string>> arrayEventTypes,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            if (subselect.SubselectNumber == -1) {
                throw new IllegalStateException("Subselect is unassigned");
            }

            var statementSpec = subselect.StatementSpecCompiled;
            var filterStreamSpec = statementSpec.StreamSpecs[0];

            IList<ViewFactoryForge> viewForges;
            string subselecteventTypeName;
            IList<StmtClassForgeableFactory> additionalForgeables;

            // construct view factory chain
            EventType eventType;
            try {
                var args = new ViewFactoryForgeArgs(
                    -1,
                    subselect.SubselectNumber,
                    StreamSpecOptions.DEFAULT,
                    null,
                    statementRawInfo,
                    services);
                var streamSpec = statementSpec.StreamSpecs[0];

                if (streamSpec is FilterStreamSpecCompiled) {
                    var filterStreamSpecCompiled = (FilterStreamSpecCompiled)statementSpec.StreamSpecs[0];
                    subselecteventTypeName = filterStreamSpecCompiled.FilterSpecCompiled.FilterForEventTypeName;

                    // A child view is required to limit the stream
                    if (filterStreamSpec.ViewSpecs.Length == 0) {
                        throw new ExprValidationException(
                            "Subqueries require one or more views to limit the stream, consider declaring a length or time window");
                    }

                    var viewForgeDesc = ViewFactoryForgeUtil.CreateForges(
                        filterStreamSpecCompiled.ViewSpecs,
                        args,
                        filterStreamSpecCompiled.FilterSpecCompiled.ResultEventType);
                    viewForges = viewForgeDesc.Forges;
                    additionalForgeables = viewForgeDesc.MultikeyForges;
                    // Register filter, create view factories
                    eventType = viewForges.IsEmpty()
                        ? filterStreamSpecCompiled.FilterSpecCompiled.ResultEventType
                        : viewForges[^1].EventType;
                    subselect.RawEventType = eventType;
                }
                else if (streamSpec is NamedWindowConsumerStreamSpec) {
                    var namedSpec = (NamedWindowConsumerStreamSpec)statementSpec.StreamSpecs[0];
                    var namedWindow = namedSpec.NamedWindow;
                    var viewForgeDesc = ViewFactoryForgeUtil.CreateForges(
                        namedSpec.ViewSpecs,
                        args,
                        namedWindow.EventType);
                    viewForges = viewForgeDesc.Forges;
                    additionalForgeables = viewForgeDesc.MultikeyForges;
                    var namedWindowName = namedWindow.EventType.Name;
                    subselecteventTypeName = namedWindowName;
                    EPLValidationUtil.ValidateContextName(
                        false,
                        namedWindowName,
                        namedWindow.ContextName,
                        statementRawInfo.ContextName,
                        true);
                    subselect.RawEventType = namedWindow.EventType;
                    eventType = namedWindow.EventType;
                }
                else if (streamSpec is TableQueryStreamSpec) {
                    var namedSpec = (TableQueryStreamSpec)statementSpec.StreamSpecs[0];
                    var table = namedSpec.Table;
                    var viewForgeDesc = ViewFactoryForgeUtil.CreateForges(
                        namedSpec.ViewSpecs,
                        args,
                        table.InternalEventType);
                    viewForges = viewForgeDesc.Forges;
                    additionalForgeables = viewForgeDesc.MultikeyForges;
                    var namedWindowName = table.TableName;
                    subselecteventTypeName = namedWindowName;
                    EPLValidationUtil.ValidateContextName(
                        false,
                        namedWindowName,
                        table.OptionalContextName,
                        statementRawInfo.ContextName,
                        true);
                    subselect.RawEventType = table.InternalEventType;
                    eventType = table.InternalEventType;
                }
                else {
                    throw new IllegalStateException("Unexpected stream spec " + streamSpec);
                }
            }
            catch (ViewProcessingException ex) {
                throw new ExprValidationException("Failed to validate subexpression: " + ex.Message, ex);
            }

            // determine a stream name unless one was supplied
            var subexpressionStreamName = SubselectUtil.GetStreamName(
                filterStreamSpec.OptionalStreamName,
                subselect.SubselectNumber);

            // Named windows don't allow data views
            if (filterStreamSpec is NamedWindowConsumerStreamSpec | filterStreamSpec is TableQueryStreamSpec) {
                EPStatementStartMethodHelperValidate.ValidateNoDataWindowOnNamedWindow(viewForges);
            }

            // Streams event types are the original stream types with the stream zero the subselect stream
            var namesAndTypes = new LinkedHashMap<string, Pair<EventType, string>>();
            namesAndTypes.Put(subexpressionStreamName, new Pair<EventType, string>(eventType, subselecteventTypeName));
            namesAndTypes.Put(outerStreamName, new Pair<EventType, string>(outerEventType, outerEventTypeName));
            if (taggedEventTypes != null) {
                foreach (var entry in taggedEventTypes) {
                    namesAndTypes.Put(entry.Key, new Pair<EventType, string>(entry.Value.First, entry.Value.Second));
                }
            }

            if (arrayEventTypes != null) {
                foreach (var entry in arrayEventTypes) {
                    namesAndTypes.Put(entry.Key, new Pair<EventType, string>(entry.Value.First, entry.Value.Second));
                }
            }

            StreamTypeService subselectTypeService = new StreamTypeServiceImpl(namesAndTypes, true, true);
            var viewResourceDelegateSubselect = new ViewResourceDelegateExpr();
            subselect.FilterSubqueryStreamTypes = subselectTypeService;

            // Validate select expression
            var selectClauseSpec = subselect.StatementSpecCompiled.SelectClauseCompiled;
            if (selectClauseSpec.SelectExprList.Length > 0) {
                if (selectClauseSpec.SelectExprList.Length > 1) {
                    throw new ExprValidationException("Subquery multi-column select is not allowed in this context.");
                }

                var element = selectClauseSpec.SelectExprList[0];
                if (element is SelectClauseExprCompiledSpec compiled) {
                    // validate
                    var selectExpression = compiled.SelectExpression;
                    var validationContext = new ExprValidationContextBuilder(
                            subselectTypeService,
                            statementRawInfo,
                            services)
                        .WithViewResourceDelegate(viewResourceDelegateSubselect)
                        .WithAllowBindingConsumption(true)
                        .WithMemberName(new ExprValidationMemberNameQualifiedSubquery(subselect.SubselectNumber))
                        .Build();
                    selectExpression = ExprNodeUtilityValidate.GetValidatedSubtree(
                        ExprNodeOrigin.SUBQUERYSELECT,
                        selectExpression,
                        validationContext);
                    subselect.SelectClause = new ExprNode[] { selectExpression };
                    subselect.SelectAsNames = new string[] { compiled.AssignedName };

                    // handle aggregation
                    var aggExprNodes = new List<ExprAggregateNode>();
                    ExprAggregateNodeUtil.GetAggregatesBottomUp(selectExpression, aggExprNodes);
                    if (aggExprNodes.Count > 0) {
                        // Other stream properties, if there is aggregation, cannot be under aggregation.
                        foreach (var aggNode in aggExprNodes) {
                            var propertiesNodesAggregated = ExprNodeUtilityQuery.GetExpressionProperties(aggNode, true);
                            foreach (var pair in propertiesNodesAggregated) {
                                if (pair.First != 0) {
                                    throw new ExprValidationException(
                                        "Subselect aggregation function cannot aggregate across correlated properties");
                                }
                            }
                        }

                        // This stream (stream 0) properties must either all be under aggregation, or all not be.
                        var propertiesNotAggregated =
                            ExprNodeUtilityQuery.GetExpressionProperties(selectExpression, false);
                        foreach (var pair in propertiesNotAggregated) {
                            if (pair.First == 0) {
                                throw new ExprValidationException(
                                    "Subselect properties must all be within aggregation functions");
                            }
                        }
                    }
                }
            }

            return additionalForgeables;
        }
    }
} // end of namespace