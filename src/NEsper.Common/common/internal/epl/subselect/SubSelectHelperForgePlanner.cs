///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.name;
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.select;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.declared.compiletime;
using com.espertech.esper.common.@internal.epl.expression.subquery;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.index.composite;
using com.espertech.esper.common.@internal.epl.index.hash;
using com.espertech.esper.common.@internal.epl.index.inkeyword;
using com.espertech.esper.common.@internal.epl.index.sorted;
using com.espertech.esper.common.@internal.epl.index.unindexed;
using com.espertech.esper.common.@internal.epl.join.hint;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.common.@internal.epl.join.queryplan;
using com.espertech.esper.common.@internal.epl.join.queryplanbuild;
using com.espertech.esper.common.@internal.epl.join.support;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.common.@internal.epl.lookupplan;
using com.espertech.esper.common.@internal.epl.namedwindow.path;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.metrics.audit;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.common.@internal.statement.helper;
using com.espertech.esper.common.@internal.statemgmtsettings;
using com.espertech.esper.common.@internal.view.access;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.prior;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.subselect
{
	public class SubSelectHelperForgePlanner
	{
		private static readonly ILog QueryPlanLog = LogManager.GetLogger(AuditPath.QUERYPLAN_LOG);

		private const string MSG_SUBQUERY_REQUIRES_WINDOW =
			"Subqueries require one or more views to limit the stream, consider declaring a length or time window (applies to correlated or non-fully-aggregated subqueries)";

		public static SubSelectHelperForgePlan PlanSubSelect(
			StatementBaseInfo statement,
			IDictionary<ExprSubselectNode, SubSelectActivationPlan> subselectActivation,
			string[] outerStreamNames,
			EventType[] outerEventTypesSelect,
			string[] outerEventTypeNamees,
			StatementCompileTimeServices compileTimeServices)
		{

			var declaredExpressions = statement.StatementSpec.DeclaredExpressions;
			IDictionary<ExprSubselectNode, SubSelectFactoryForge> subselectForges = new LinkedHashMap<ExprSubselectNode, SubSelectFactoryForge>();
			var additionalForgeables = new List<StmtClassForgeableFactory>(2);

			IDictionary<ExprDeclaredNode, IList<ExprDeclaredNode>> declaredExpressionCallHierarchy = null;
			if (declaredExpressions.Length > 0) {
				declaredExpressionCallHierarchy = ExprNodeUtilityQuery.GetDeclaredExpressionCallHierarchy(declaredExpressions);
			}

			foreach (var entry in subselectActivation) {
				var subselect = entry.Key;
				var subSelectActivation = entry.Value;

				try {
					var forgeDesc = PlanSubSelectInternal(
						subselect,
						subSelectActivation,
						outerStreamNames,
						outerEventTypesSelect,
						outerEventTypeNamees,
						declaredExpressions,
						statement.ContextPropertyRegistry,
						declaredExpressionCallHierarchy,
						statement,
						compileTimeServices);
					subselectForges.Put(entry.Key, forgeDesc.SubSelectFactoryForge);
					additionalForgeables.AddAll(forgeDesc.AdditionalForgeables);
				}
				catch (Exception ex) {
					throw new ExprValidationException("Failed to plan " + ExprNodeUtilityMake.GetSubqueryInfoText(subselect) + ": " + ex.Message, ex);
				}
			}

			return new SubSelectHelperForgePlan(subselectForges, additionalForgeables);
		}

		private static SubSelectFactoryForgeDesc PlanSubSelectInternal(
			ExprSubselectNode subselect,
			SubSelectActivationPlan subselectActivation,
			string[] outerStreamNames,
			EventType[] outerEventTypesSelect,
			string[] outerEventTypeNamees,
			ExprDeclaredNode[] declaredExpressions,
			ContextPropertyRegistry contextPropertyRegistry,
			IDictionary<ExprDeclaredNode, IList<ExprDeclaredNode>> declaredExpressionCallHierarchy,
			StatementBaseInfo statement,
			StatementCompileTimeServices services)
		{
			var queryPlanLogging = services.Configuration.Common.Logging.IsEnableQueryPlan;
			if (queryPlanLogging && QueryPlanLog.IsInfoEnabled) {
				QueryPlanLog.Info("For statement '" + statement.StatementNumber + "' subquery " + subselect.SubselectNumber);
			}

			var annotations = statement.StatementSpec.Annotations;
			var indexHint = IndexHint.GetIndexHint(annotations);
			var subselectSpec = subselect.StatementSpecCompiled;
			var filterStreamSpec = subselectSpec.StreamSpecs[0];
			var subqueryNum = subselect.SubselectNumber;

			string subselecteventTypeName = null;
			if (filterStreamSpec is FilterStreamSpecCompiled) {
				subselecteventTypeName = ((FilterStreamSpecCompiled) filterStreamSpec).FilterSpecCompiled.FilterForEventTypeName;
			}
			else if (filterStreamSpec is NamedWindowConsumerStreamSpec) {
				subselecteventTypeName = ((NamedWindowConsumerStreamSpec) filterStreamSpec).NamedWindow.EventType.Name;
			}
			else if (filterStreamSpec is TableQueryStreamSpec) {
				subselecteventTypeName = ((TableQueryStreamSpec) filterStreamSpec).Table.TableName;
			}

			var viewForges = subselectActivation.ViewForges;
			var eventType = viewForges.IsEmpty() ? subselectActivation.ViewableType : viewForges[viewForges.Count - 1].EventType;

			// determine a stream name unless one was supplied
			var subexpressionStreamName = SubselectUtil.GetStreamName(filterStreamSpec.OptionalStreamName, subselect.SubselectNumber);
			var allStreamNames = new string[outerStreamNames.Length + 1];
			Array.Copy(outerStreamNames, 0, allStreamNames, 1, outerStreamNames.Length);
			allStreamNames[0] = subexpressionStreamName;

			// Named windows don't allow data views
			if (filterStreamSpec is NamedWindowConsumerStreamSpec || filterStreamSpec is TableQueryStreamSpec) {
				EPStatementStartMethodHelperValidate.ValidateNoDataWindowOnNamedWindow(viewForges);
			}

			// Expression declarations are copies of a predefined expression body with their own stream context.
			// Should only be invoked if the subselect belongs to that instance.
			StreamTypeService subselectTypeService = null;
			EventType[] outerEventTypes = null;

			// determine subselect type information from the enclosing declared expression, if possibly enclosed
			if (declaredExpressions != null && declaredExpressions.Length > 0) {
				subselectTypeService = GetDeclaredExprTypeService(
					declaredExpressions,
					declaredExpressionCallHierarchy,
					outerStreamNames,
					outerEventTypesSelect,
					subselect,
					subexpressionStreamName,
					eventType);
				if (subselectTypeService != null) {
					outerEventTypes = new EventType[subselectTypeService.EventTypes.Length - 1];
					Array.Copy(subselectTypeService.EventTypes, 1, outerEventTypes, 0, subselectTypeService.EventTypes.Length - 1);
				}
			}

			// Use the override provided by the subselect if present
			if (subselectTypeService == null) {
				if (subselect.FilterSubqueryStreamTypes != null) {
					subselectTypeService = subselect.FilterSubqueryStreamTypes;
					outerEventTypes = new EventType[subselectTypeService.EventTypes.Length - 1];
					Array.Copy(subselectTypeService.EventTypes, 1, outerEventTypes, 0, subselectTypeService.EventTypes.Length - 1);
				}
				else {
					// Streams event types are the original stream types with the stream zero the subselect stream
					var namesAndTypes = new LinkedHashMap<string, Pair<EventType, string>>();
					namesAndTypes.Put(subexpressionStreamName, new Pair<EventType, string>(eventType, subselecteventTypeName));
					for (var i = 0; i < outerEventTypesSelect.Length; i++) {
						var pair = new Pair<EventType, string>(outerEventTypesSelect[i], outerEventTypeNamees[i]);
						namesAndTypes.Put(outerStreamNames[i], pair);
					}

					subselectTypeService = new StreamTypeServiceImpl(namesAndTypes, true, true);
					outerEventTypes = outerEventTypesSelect;
				}
			}

			// Validate select expression
			var viewResourceDelegateSubselect = new ViewResourceDelegateExpr();
			var selectClauseSpec = subselect.StatementSpecCompiled.SelectClauseCompiled;
			IList<ExprNode> selectExpressions = new List<ExprNode>();
			IList<string> assignedNames = new List<string>();
			var isWildcard = false;
			var isStreamWildcard = false;
			bool hasNonAggregatedProperties;

			var validationContext = new ExprValidationContextBuilder(subselectTypeService, statement.StatementRawInfo, services)
				.WithViewResourceDelegate(viewResourceDelegateSubselect)
				.WithAllowBindingConsumption(true)
				.WithMemberName(new ExprValidationMemberNameQualifiedSubquery(subqueryNum))
				.Build();
			IList<ExprAggregateNode> aggExprNodesSelect = new List<ExprAggregateNode>();

			for (var i = 0; i < selectClauseSpec.SelectExprList.Length; i++) {
				var element = selectClauseSpec.SelectExprList[i];

				if (element is SelectClauseExprCompiledSpec) {
					// validate
					var compiled = (SelectClauseExprCompiledSpec) element;
					var selectExpression = compiled.SelectExpression;
					selectExpression = ExprNodeUtilityValidate.GetValidatedSubtree(ExprNodeOrigin.SELECT, selectExpression, validationContext);

					selectExpressions.Add(selectExpression);
					if (compiled.AssignedName == null) {
						assignedNames.Add(ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(selectExpression));
					}
					else {
						assignedNames.Add(compiled.AssignedName);
					}

					// handle aggregation
					ExprAggregateNodeUtil.GetAggregatesBottomUp(selectExpression, aggExprNodesSelect);

					// This stream (stream 0) properties must either all be under aggregation, or all not be.
					if (aggExprNodesSelect.Count > 0) {
						var propertiesNotAggregated = ExprNodeUtilityQuery.GetExpressionProperties(selectExpression, false);
						foreach (var pair in propertiesNotAggregated) {
							if (pair.First == 0) {
								throw new ExprValidationException("Subselect properties must all be within aggregation functions");
							}
						}
					}
				}
				else if (element is SelectClauseElementWildcard) {
					isWildcard = true;
				}
				else if (element is SelectClauseStreamCompiledSpec) {
					isStreamWildcard = true;
				}
			} // end of for loop

			// validate having-clause and collect aggregations
			IList<ExprAggregateNode> aggExpressionNodesHaving = EmptyList<ExprAggregateNode>.Instance;
			if (subselectSpec.Raw.HavingClause != null) {
				var validatedHavingClause = ExprNodeUtilityValidate.GetValidatedSubtree(
					ExprNodeOrigin.HAVING,
					subselectSpec.Raw.HavingClause,
					validationContext);
				if (!validatedHavingClause.Forge.EvaluationType.IsBoolean()) {
					throw new ExprValidationException("Subselect having-clause expression must return a boolean value");
				}

				aggExpressionNodesHaving = new List<ExprAggregateNode>();
				ExprAggregateNodeUtil.GetAggregatesBottomUp(validatedHavingClause, aggExpressionNodesHaving);
				ValidateAggregationPropsAndLocalGroup(aggExpressionNodesHaving);

				// if the having-clause does not have aggregations, it becomes part of the filter
				if (aggExpressionNodesHaving.IsEmpty()) {
					var filter = subselectSpec.Raw.WhereClause;
					if (filter == null) {
						subselectSpec.Raw.WhereClause = subselectSpec.Raw.HavingClause;
					}
					else {
						subselectSpec.Raw.WhereClause =
							ExprNodeUtilityMake.ConnectExpressionsByLogicalAnd(Arrays.AsList(subselectSpec.Raw.WhereClause, subselectSpec.Raw.HavingClause));
					}

					subselectSpec.Raw.HavingClause = null;
				}
				else {
					subselect.HavingExpr = validatedHavingClause.Forge;
					var nonAggregatedPropsHaving = ExprNodeUtilityAggregation.GetNonAggregatedProps(
						validationContext.StreamTypeService.EventTypes,
						Collections.SingletonList(validatedHavingClause),
						contextPropertyRegistry);
					foreach (var prop in nonAggregatedPropsHaving.Properties) {
						if (prop.StreamNum == 0) {
							throw new ExprValidationException(
								"Subselect having-clause requires that all properties are under aggregation, consider using the 'first' aggregation function instead");
						}
					}
				}
			}

			// Figure out all non-aggregated event properties in the select clause (props not under a sum/avg/max aggregation node)
			var nonAggregatedPropsSelect = ExprNodeUtilityAggregation.GetNonAggregatedProps(
				validationContext.StreamTypeService.EventTypes,
				selectExpressions,
				contextPropertyRegistry);
			hasNonAggregatedProperties = !nonAggregatedPropsSelect.IsEmpty();

			// Validate and set select-clause names and expressions
			if (!selectExpressions.IsEmpty()) {
				if (isWildcard || isStreamWildcard) {
					throw new ExprValidationException(
						"Subquery multi-column select does not allow wildcard or stream wildcard when selecting multiple columns.");
				}

				if (selectExpressions.Count > 1 && !subselect.IsAllowMultiColumnSelect) {
					throw new ExprValidationException("Subquery multi-column select is not allowed in this context.");
				}

				if (subselectSpec.GroupByExpressions == null &&
				    selectExpressions.Count > 1 &&
				    aggExprNodesSelect.Count > 0 &&
				    hasNonAggregatedProperties) {
					throw new ExprValidationException(
						"Subquery with multi-column select requires that either all or none of the selected columns are under aggregation, unless a group-by clause is also specified");
				}

				subselect.SelectClause = selectExpressions.ToArray();
				subselect.SelectAsNames = assignedNames.ToArray();
			}

			// Handle aggregation
			ExprNodePropOrStreamSet propertiesGroupBy = null;
			AggregationServiceForgeDesc aggregationServiceForgeDesc = null;
			ExprNode[] groupByNodes = null;
			MultiKeyPlan groupByMultikeyPlan = null;
			IList<StmtClassForgeableFactory> additionalForgeables = new List<StmtClassForgeableFactory>();
			if (aggExprNodesSelect.Count > 0 || aggExpressionNodesHaving.Count > 0) {
				var groupBy = subselectSpec.GroupByExpressions;
				if (groupBy != null && groupBy.GroupByRollupLevels != null) {
					throw new ExprValidationException("Group-by expressions in a subselect may not have rollups");
				}

				groupByNodes = groupBy == null ? null : groupBy.GroupByNodes;
				var hasGroupBy = groupByNodes != null && groupByNodes.Length > 0;
				if (hasGroupBy) {

					// validate group-by
					for (var i = 0; i < groupByNodes.Length; i++) {
						groupByNodes[i] = ExprNodeUtilityValidate.GetValidatedSubtree(ExprNodeOrigin.GROUPBY, groupByNodes[i], validationContext);
						var minimal = ExprNodeUtilityValidate.IsMinimalExpression(groupByNodes[i]);
						if (minimal != null) {
							throw new ExprValidationException("Group-by expressions in a subselect may not have " + minimal);
						}
					}

					// Get a list of event properties being aggregated in the select clause, if any
					propertiesGroupBy = ExprNodeUtilityAggregation.GetGroupByPropertiesValidateHasOne(groupByNodes);

					// Validated all group-by properties come from stream itself
					var firstNonZeroGroupBy = propertiesGroupBy.FirstWithStreamNumNotZero;
					if (firstNonZeroGroupBy != null) {
						throw new ExprValidationException(
							"Subselect with group-by requires that group-by properties are provided by the subselect stream only (" +
							firstNonZeroGroupBy.Textual +
							" is not)");
					}

					// Validate that this is a grouped full-aggregated case
					var reasonMessage = propertiesGroupBy.NotContainsAll(nonAggregatedPropsSelect);
					var allInGroupBy = reasonMessage == null;
					if (!allInGroupBy) {
						throw new ExprValidationException(
							"Subselect with group-by requires non-aggregated properties in the select-clause to also appear in the group-by clause");
					}

					// Plan multikey
					groupByMultikeyPlan = MultiKeyPlanner.PlanMultiKey(groupByNodes, false, statement.StatementRawInfo, services.SerdeResolver);
					additionalForgeables.AddAll(groupByMultikeyPlan.MultiKeyForgeables);
				}

				// Other stream properties, if there is aggregation, cannot be under aggregation.
				ValidateAggregationPropsAndLocalGroup(aggExprNodesSelect);

				// determine whether select-clause has grouped-by expressions
				IList<ExprAggregateNodeGroupKey> groupKeyExpressions = null;
				var groupByExpressions = ExprNodeUtilityQuery.EMPTY_EXPR_ARRAY;
				if (hasGroupBy) {
					groupByExpressions = subselectSpec.GroupByExpressions.GroupByNodes;
					for (var i = 0; i < selectExpressions.Count; i++) {
						var selectExpression = selectExpressions[i];
						var revalidate = false;
						for (var j = 0; j < groupByExpressions.Length; j++) {
							var foundPairs = ExprNodeUtilityQuery.FindExpression(selectExpression, groupByExpressions[j]);
							foreach (var pair in foundPairs) {
								CodegenFieldName aggName = new CodegenFieldNameSubqueryAgg(subqueryNum);
								var replacement = new ExprAggregateNodeGroupKey(
									groupByExpressions.Length,
									j,
									groupByExpressions[j].Forge.EvaluationType,
									aggName);
								if (pair.First == null) {
									selectExpressions[i] = replacement;
								}
								else {
									ExprNodeUtilityModify.ReplaceChildNode(pair.First, pair.Second, replacement);
									revalidate = true;
								}

								if (groupKeyExpressions == null) {
									groupKeyExpressions = new List<ExprAggregateNodeGroupKey>();
								}

								groupKeyExpressions.Add(replacement);
							}
						}

						// if the select-clause expression changed, revalidate it
						if (revalidate) {
							selectExpression = ExprNodeUtilityValidate.GetValidatedSubtree(ExprNodeOrigin.SELECT, selectExpression, validationContext);
							selectExpressions[i] = selectExpression;
						}
					} // end of for loop
				}

				aggregationServiceForgeDesc = AggregationServiceFactoryFactory.GetService(
					aggExprNodesSelect,
					EmptyDictionary<ExprNode, string>.Instance,
					EmptyList<ExprDeclaredNode>.Instance, 
					groupByExpressions,
					groupByMultikeyPlan == null ? null : groupByMultikeyPlan.ClassRef,
					aggExpressionNodesHaving,
					EmptyList<ExprAggregateNode>.Instance,
					groupKeyExpressions,
					hasGroupBy,
					annotations,
					services.VariableCompileTimeResolver,
					true,
					subselectSpec.Raw.WhereClause,
					subselectSpec.Raw.HavingClause,
					subselectTypeService.EventTypes,
					null,
					subselectSpec.Raw.OptionalContextName,
					null,
					null,
					false,
					services.IsFireAndForget,
					false,
					services.ImportServiceCompileTime,
					statement.StatementRawInfo,
					services.SerdeResolver,
					services.StateMgmtSettingsProvider);
				additionalForgeables.AddAll(aggregationServiceForgeDesc.AdditionalForgeables);

				// assign select-clause
				if (!selectExpressions.IsEmpty()) {
					subselect.SelectClause = selectExpressions.ToArray();
					subselect.SelectAsNames = assignedNames.ToArray();
				}
			}

			// no aggregation functions allowed in filter
			if (subselectSpec.Raw.WhereClause != null) {
				var aggExprNodesFilter = new List<ExprAggregateNode>();
				ExprAggregateNodeUtil.GetAggregatesBottomUp(subselectSpec.Raw.WhereClause, aggExprNodesFilter);
				if (aggExprNodesFilter.Count > 0) {
					throw new ExprValidationException(
						"Aggregation functions are not supported within subquery filters, consider using a having-clause or insert-into instead");
				}
			}

			// validate filter expression, if there is one
			var filterExpr = subselectSpec.Raw.WhereClause;

			// add the table filter for tables
			if (filterStreamSpec is TableQueryStreamSpec) {
				var table = (TableQueryStreamSpec) filterStreamSpec;
				filterExpr = ExprNodeUtilityMake.ConnectExpressionsByLogicalAnd(table.FilterExpressions, filterExpr);
			}

			// determine correlated
			var correlatedSubquery = false;
			if (filterExpr != null) {
				filterExpr = ExprNodeUtilityValidate.GetValidatedSubtree(ExprNodeOrigin.FILTER, filterExpr, validationContext);
				if (!filterExpr.Forge.EvaluationType.IsBoolean()) {
					throw new ExprValidationException("Subselect filter expression must return a boolean value");
				}

				// check the presence of a correlated filter, not allowed with aggregation
				var visitor = new ExprNodeIdentifierVisitor(true);
				filterExpr.Accept(visitor);
				var propertiesNodes = visitor.ExprProperties;
				foreach (var pair in propertiesNodes) {
					if (pair.First != 0) {
						correlatedSubquery = true;
						break;
					}
				}
			}

			var viewResourceDelegateDesc = ViewResourceVerifyHelper.VerifyPreviousAndPriorRequirements(
				new IList<ViewFactoryForge>[] { viewForges },
				viewResourceDelegateSubselect)[0];
			if (ViewResourceDelegateDesc.HasPrior(new ViewResourceDelegateDesc[] {viewResourceDelegateDesc})) {
				if (!viewResourceDelegateDesc.PriorRequests.IsEmpty()) {
					var unbound = viewForges.IsEmpty();
					var stateMgmtSettings = unbound
						? StateMgmtSettingDefault.INSTANCE
						: services.StateMgmtSettingsProvider.GetView(statement.StatementRawInfo, -1, true, false, AppliesTo.WINDOW_PRIOR);
					viewForges.Add(
						new PriorEventViewForge(
							unbound,
							viewForges.IsEmpty() ? eventType : viewForges[viewForges.Count - 1].EventType,
							stateMgmtSettings));
				}
			}

			// Set the aggregated flag
			// This must occur here as some analysis of return type depends on aggregated or not.
			if (aggregationServiceForgeDesc == null) {
				subselect.SubselectAggregationType = ExprSubselectNode.SubqueryAggregationType.NONE;
			}
			else {
				subselect.SubselectAggregationType = hasNonAggregatedProperties
					? ExprSubselectNode.SubqueryAggregationType.FULLY_AGGREGATED_WPROPS
					: ExprSubselectNode.SubqueryAggregationType.FULLY_AGGREGATED_NOPROPS;
			}

			// Set the filter.
			var filterExprEval = (filterExpr == null) ? null : filterExpr.Forge;
			var assignedFilterExpr = aggregationServiceForgeDesc != null ? null : filterExprEval;
			subselect.FilterExpr = assignedFilterExpr;

			// validation for correlated subqueries against named windows contained-event syntax
			if (filterStreamSpec is NamedWindowConsumerStreamSpec && correlatedSubquery) {
				var namedSpec = (NamedWindowConsumerStreamSpec) filterStreamSpec;
				if (namedSpec.OptPropertyEvaluator != null) {
					throw new ExprValidationException(
						"Failed to validate named window use in subquery, contained-event is only allowed for named windows when not correlated");
				}
			}

			// Validate presence of a data window
			ValidateSubqueryDataWindow(subselect, correlatedSubquery, hasNonAggregatedProperties, propertiesGroupBy, nonAggregatedPropsSelect);

			// Determine strategy factories
			//

			// handle named window index share first
			SubSelectFactoryForge forge = null; 
			if (filterStreamSpec is NamedWindowConsumerStreamSpec) {
				var namedSpec = (NamedWindowConsumerStreamSpec) filterStreamSpec;
				if (namedSpec.FilterExpressions.IsEmpty()) {
					var namedWindowX = namedSpec.NamedWindow;
					var disableIndexShare = HintEnum.DISABLE_WINDOW_SUBQUERY_INDEXSHARE.GetHint(annotations) != null;
					if (disableIndexShare && namedWindowX.IsVirtualDataWindow) {
						disableIndexShare = false;
					}

					if ((!disableIndexShare && namedWindowX.IsEnableIndexShare) || (services.IsFireAndForget)) {
						ValidateContextAssociation(statement.ContextName, namedWindowX.ContextName, "named window '" + namedWindowX.EventType.Name + "'");
						if (queryPlanLogging && QueryPlanLog.IsInfoEnabled) {
							QueryPlanLog.Info("prefering shared index");
						}

						var fullTableScanX = HintEnum.SET_NOINDEX.GetHint(annotations) != null;
						var excludePlanHint = ExcludePlanHint.GetHint(allStreamNames, statement.StatementRawInfo, services);
						var joinedPropPlan = QueryPlanIndexBuilder.GetJoinProps(
							filterExpr,
							outerEventTypes.Length,
							subselectTypeService.EventTypes,
							excludePlanHint);
						var strategyForgeX = new SubSelectStrategyFactoryIndexShareForge(
							subqueryNum,
							subselectActivation,
							outerEventTypesSelect,
							namedWindowX,
							null,
							fullTableScanX,
							indexHint,
							joinedPropPlan,
							filterExprEval,
							groupByNodes,
							aggregationServiceForgeDesc,
							statement,
							services);
						additionalForgeables.AddAll(strategyForgeX.AdditionalForgeables);
						forge = new SubSelectFactoryForge(subqueryNum, subselectActivation.Activator, strategyForgeX);
					}
				}
				else if (services.IsFireAndForget) {
					throw new ExprValidationException("Subqueries in fire-and-forget queries do not allow filter expressions");
				}
			} else if (filterStreamSpec is TableQueryStreamSpec tableSpec) {
				// handle table-subselect
				ValidateContextAssociation(
					statement.StatementRawInfo.ContextName,
					tableSpec.Table.OptionalContextName,
					"table '" + tableSpec.Table.TableName + "'");
				var fullTableScanX = HintEnum.SET_NOINDEX.GetHint(annotations) != null;
				var excludePlanHint = ExcludePlanHint.GetHint(allStreamNames, statement.StatementRawInfo, services);
				var joinedPropPlan = QueryPlanIndexBuilder.GetJoinProps(filterExpr, outerEventTypes.Length, subselectTypeService.EventTypes, excludePlanHint);
				var strategyForgeX = new SubSelectStrategyFactoryIndexShareForge(
					subqueryNum,
					subselectActivation,
					outerEventTypesSelect,
					null,
					tableSpec.Table,
					fullTableScanX,
					indexHint,
					joinedPropPlan,
					filterExprEval,
					groupByNodes,
					aggregationServiceForgeDesc,
					statement,
					services);
				additionalForgeables.AddAll(strategyForgeX.AdditionalForgeables);
				forge = new SubSelectFactoryForge(subqueryNum, subselectActivation.Activator, strategyForgeX);
			}

			if (forge == null) {
				// determine unique keys, if any
				var optionalUniqueProps = StreamJoinAnalysisResultCompileTime.GetUniqueCandidateProperties(viewForges, annotations);
				NamedWindowMetaData namedWindow = null;
				ExprNode namedWindowFilterExpr = null;
				QueryGraphForge namedWindowFilterQueryGraph = null;
				if (filterStreamSpec is NamedWindowConsumerStreamSpec) {
					var namedSpec = (NamedWindowConsumerStreamSpec) filterStreamSpec;
					namedWindow = namedSpec.NamedWindow;
					optionalUniqueProps = namedWindow.UniquenessAsSet;
					if (namedSpec.FilterExpressions != null && !namedSpec.FilterExpressions.IsEmpty()) {
						var types = new StreamTypeServiceImpl(namedWindow.EventType, namedWindow.EventType.Name, false);
						namedWindowFilterExpr = ExprNodeUtilityMake.ConnectExpressionsByLogicalAndWhenNeeded(namedSpec.FilterExpressions);
						namedWindowFilterQueryGraph = EPLValidationUtil.ValidateFilterGetQueryGraphSafe(
							namedWindowFilterExpr,
							types,
							statement.StatementRawInfo,
							services);
					}
				}

				// handle local stream + named-window-stream
				var fullTableScan = HintEnum.SET_NOINDEX.GetHint(annotations) != null;
				var indexDesc = DetermineSubqueryIndexFactory(
					filterExpr,
					eventType,
					outerEventTypes,
					subselectTypeService,
					fullTableScan,
					queryPlanLogging,
					optionalUniqueProps,
					statement,
					subselect,
					services);
				additionalForgeables.AddAll(indexDesc.AdditionalForgeables);
				var indexPair = new Pair<EventTableFactoryFactoryForge, SubordTableLookupStrategyFactoryForge>(indexDesc.TableForge, indexDesc.LookupForge);

				SubSelectStrategyFactoryForge strategyForge = new SubSelectStrategyFactoryLocalViewPreloadedForge(
					viewForges,
					viewResourceDelegateDesc,
					indexPair,
					filterExpr,
					correlatedSubquery,
					aggregationServiceForgeDesc, /* viewResourceDelegateVerified */
					subqueryNum,
					groupByNodes,
					namedWindow,
					namedWindowFilterExpr,
					namedWindowFilterQueryGraph,
					groupByMultikeyPlan == null ? null : groupByMultikeyPlan.ClassRef);

				forge = new SubSelectFactoryForge(subqueryNum, subselectActivation.Activator, strategyForge);
			}

			// For subselect in filters, we must validate-subquery again as the first validate was not including the information compiled herein.
			// This is because filters are validated first so their information is available to stream-type-service and this validation.
			// Validate-subquery validates and builds the subselect strategy forge.
			if (subselect.IsFilterStreamSubselect) {
				subselect.ValidateSubquery(subselect.FilterStreamExprValidationContext);
			}

			return new SubSelectFactoryForgeDesc(forge, additionalForgeables);
		}

		private static void ValidateSubqueryDataWindow(
			ExprSubselectNode subselectNode,
			bool correlatedSubquery,
			bool hasNonAggregatedProperties,
			ExprNodePropOrStreamSet propertiesGroupBy,
			ExprNodePropOrStreamSet nonAggregatedPropsSelect)
		{
			// validation applies only to type+filter subqueries that have no data window
			var streamSpec = subselectNode.StatementSpecCompiled.StreamSpecs[0];
			if (!(streamSpec is FilterStreamSpecCompiled) || streamSpec.ViewSpecs.Length > 0) {
				return;
			}

			if (correlatedSubquery) {
				throw new ExprValidationException(MSG_SUBQUERY_REQUIRES_WINDOW);
			}

			// we have non-aggregated properties
			if (hasNonAggregatedProperties) {
				if (propertiesGroupBy == null) {
					throw new ExprValidationException(MSG_SUBQUERY_REQUIRES_WINDOW);
				}

				var reason = nonAggregatedPropsSelect.NotContainsAll(propertiesGroupBy);
				if (reason != null) {
					throw new ExprValidationException(MSG_SUBQUERY_REQUIRES_WINDOW);
				}
			}
		}

		private static void ValidateAggregationPropsAndLocalGroup(IList<ExprAggregateNode> aggregateNodes)
		{
			foreach (var aggNode in aggregateNodes) {
				var propertiesNodesAggregated = ExprNodeUtilityQuery.GetExpressionProperties(aggNode, true);
				foreach (var pair in propertiesNodesAggregated) {
					if (pair.First != 0) {
						throw new ExprValidationException("Subselect aggregation functions cannot aggregate across correlated properties");
					}
				}

				if (aggNode.OptionalLocalGroupBy != null) {
					throw new ExprValidationException("Subselect aggregations functions cannot specify a group-by");
				}
			}
		}

		private static SubqueryIndexForgeDesc DetermineSubqueryIndexFactory(
			ExprNode filterExpr,
			EventType viewableEventType,
			EventType[] outerEventTypes,
			StreamTypeService subselectTypeService,
			bool fullTableScan,
			bool queryPlanLogging,
			ISet<string> optionalUniqueProps,
			StatementBaseInfo statement,
			ExprSubselectNode subselect,
			StatementCompileTimeServices services)
		{
			var desc = DetermineSubqueryIndexInternalFactory(
				filterExpr,
				viewableEventType,
				outerEventTypes,
				subselectTypeService,
				fullTableScan,
				optionalUniqueProps,
				statement,
				subselect,
				services);

			var hook = QueryPlanIndexHookUtil.GetHook(statement.StatementSpec.Annotations, services.ImportServiceCompileTime);
			if (queryPlanLogging && (QueryPlanLog.IsInfoEnabled || hook != null)) {
				QueryPlanLog.Info("local index");
				QueryPlanLog.Info("strategy " + desc.LookupForge.ToQueryPlan());
				QueryPlanLog.Info("table " + desc.TableForge.ToQueryPlan());
				if (hook != null) {
					var strategyName = desc.LookupForge.GetType().Name;
					hook.Subquery(
						new QueryPlanIndexDescSubquery(
							new IndexNameAndDescPair[] {
								new IndexNameAndDescPair(null, desc.TableForge.EventTableClass.Name)
							},
							subselect.SubselectNumber,
							strategyName));
				}
			}

			return desc;
		}

		private static string ValidateContextAssociation(
			string optionalProvidedContextName,
			string entityDeclaredContextName,
			string entityDesc)
		{
			if (entityDeclaredContextName != null) {
				if (optionalProvidedContextName == null || !optionalProvidedContextName.Equals(entityDeclaredContextName)) {
					throw new ExprValidationException(
						"Mismatch in context specification, the context for the " +
						entityDesc +
						" is '" +
						entityDeclaredContextName +
						"' and the query specifies " +
						(optionalProvidedContextName == null ? "no context " : "context '" + optionalProvidedContextName + "'"));
				}
			}

			return null;
		}

		private static SubqueryIndexForgeDesc DetermineSubqueryIndexInternalFactory(
			ExprNode filterExpr,
			EventType viewableEventType,
			EventType[] outerEventTypes,
			StreamTypeService subselectTypeService,
			bool fullTableScan,
			ISet<string> optionalUniqueProps,
			StatementBaseInfo statement,
			ExprSubselectNode subselectNode,
			StatementCompileTimeServices services)
		{
			var subqueryNumber = subselectNode.SubselectNumber;

			// No filter expression means full table scan
			if ((filterExpr == null) || fullTableScan) {
				var stateMgmtSettings = services.StateMgmtSettingsProvider.GetIndex(statement.StatementRawInfo, AppliesTo.INDEX_UNINDEXED);
				var tableForge = new UnindexedEventTableFactoryFactoryForge(0, subqueryNumber, false, stateMgmtSettings);
				var strategy = new SubordFullTableScanLookupStrategyFactoryForge();
				return new SubqueryIndexForgeDesc(tableForge, strategy, EmptyList<StmtClassForgeableFactory>.Instance);
			}

			// Build a list of streams and indexes
			var excludePlanHint = ExcludePlanHint.GetHint(subselectTypeService.StreamNames, statement.StatementRawInfo, services);
			var joinPropDesc = QueryPlanIndexBuilder.GetJoinProps(filterExpr, outerEventTypes.Length, subselectTypeService.EventTypes, excludePlanHint);
			var hashKeys = joinPropDesc.HashProps;
			var rangeKeys = joinPropDesc.RangeProps;
			IList<SubordPropHashKeyForge> hashKeyList = new List<SubordPropHashKeyForge>(hashKeys.Values);
			IList<SubordPropRangeKeyForge> rangeKeyList = new List<SubordPropRangeKeyForge>(rangeKeys.Values);
			var unique = false;
			IList<ExprNode> inKeywordSingleIdxKeys = null;
			ExprNode inKeywordMultiIdxKey = null;

			// If this is a unique-view and there are unique criteria, use these
			if (optionalUniqueProps != null && !optionalUniqueProps.IsEmpty()) {
				var found = true;
				foreach (var uniqueProp in optionalUniqueProps) {
					if (!hashKeys.ContainsKey(uniqueProp)) {
						found = false;
						break;
					}
				}

				if (found) {
					var hashKeysArray = hashKeys.Keys.ToArray();
					foreach (var hashKey in hashKeysArray) {
						if (!optionalUniqueProps.Contains(hashKey)) {
							hashKeys.Remove(hashKey);
						}
					}

					hashKeyList = new List<SubordPropHashKeyForge>(hashKeys.Values);
					unique = true;
					rangeKeyList.Clear();
					rangeKeys.Clear();
				}
			}

			// build table (local table)
			EventTableFactoryFactoryForge eventTableFactory;
			CoercionDesc hashCoercionDesc;
			CoercionDesc rangeCoercionDesc;
			var additionalForgeables = new List<StmtClassForgeableFactory>();
			MultiKeyClassRef hashMultikeyClasses = null;
			if (hashKeys.Count != 0 && rangeKeys.IsEmpty()) {
				var indexedProps = hashKeys.Keys.ToArray();
				hashCoercionDesc = CoercionUtil.GetCoercionTypesHash(viewableEventType, indexedProps, hashKeyList);
				rangeCoercionDesc = new CoercionDesc(false, null);
				var multiKeyPlan = MultiKeyPlanner.PlanMultiKey(hashCoercionDesc.CoercionTypes, false, statement.StatementRawInfo, services.SerdeResolver);
				additionalForgeables.AddAll(multiKeyPlan.MultiKeyForgeables);
				hashMultikeyClasses = multiKeyPlan.ClassRef;
				var stateMgmtSettings = services.StateMgmtSettingsProvider.GetIndex(statement.StatementRawInfo, AppliesTo.INDEX_HASH);
				eventTableFactory = new PropertyHashedFactoryFactoryForge(
					0,
					subqueryNumber,
					false,
					indexedProps,
					viewableEventType,
					unique,
					hashCoercionDesc,
					multiKeyPlan.ClassRef,
					stateMgmtSettings);
			}
			else if (hashKeys.IsEmpty() && rangeKeys.IsEmpty()) {
				rangeCoercionDesc = new CoercionDesc(false, null);
				if (joinPropDesc.InKeywordSingleIndex != null) {
					var prop = joinPropDesc.InKeywordSingleIndex.IndexedProp;
					var propTypes = new Type[] {viewableEventType.GetPropertyType(prop)};
					hashCoercionDesc = new CoercionDesc(false, propTypes);
					var serdeForge = services.SerdeResolver.SerdeForIndexHashNonArray(propTypes[0], statement.StatementRawInfo);
					hashMultikeyClasses = new MultiKeyClassRefWSerde(serdeForge, propTypes);
					var stateMgmtSettings = services.StateMgmtSettingsProvider.GetIndex(statement.StatementRawInfo, AppliesTo.INDEX_IN);
					eventTableFactory = new PropertyHashedFactoryFactoryForge(
						0,
						subqueryNumber,
						false,
						new string[] {prop},
						viewableEventType,
						unique,
						hashCoercionDesc,
						hashMultikeyClasses,
						stateMgmtSettings);
					inKeywordSingleIdxKeys = joinPropDesc.InKeywordSingleIndex.Expressions;
				}
				else if (joinPropDesc.InKeywordMultiIndex != null) {
					var props = joinPropDesc.InKeywordMultiIndex.IndexedProp;
					hashCoercionDesc = new CoercionDesc(false, EventTypeUtility.GetPropertyTypes(viewableEventType, props));
					var serdes = new DataInputOutputSerdeForge[hashCoercionDesc.CoercionTypes.Length];
					for (var i = 0; i < hashCoercionDesc.CoercionTypes.Length; i++) {
						serdes[i] = services.SerdeResolver.SerdeForIndexHashNonArray(hashCoercionDesc.CoercionTypes[i], statement.StatementRawInfo);
					}

					var stateMgmtSettings = services.StateMgmtSettingsProvider.GetIndex(statement.StatementRawInfo, AppliesTo.INDEX_IN);
					eventTableFactory = new PropertyHashedArrayFactoryFactoryForge(
						0,
						viewableEventType,
						props,
						hashCoercionDesc.CoercionTypes,
						serdes,
						unique,
						false,
						stateMgmtSettings);
					inKeywordMultiIdxKey = joinPropDesc.InKeywordMultiIndex.Expression;
				}
				else {
					var stateMgmtSettings = services.StateMgmtSettingsProvider.GetIndex(statement.StatementRawInfo, AppliesTo.INDEX_UNINDEXED);
					hashCoercionDesc = new CoercionDesc(false, null);
					eventTableFactory = new UnindexedEventTableFactoryFactoryForge(0, subqueryNumber, false, stateMgmtSettings);
				}
			}
			else if (hashKeys.IsEmpty() && rangeKeys.Count == 1) {
				var indexedProp = rangeKeys.Keys.First();
				var coercionRangeTypes = CoercionUtil.GetCoercionTypesRange(viewableEventType, rangeKeys, outerEventTypes);
				var serde = services.SerdeResolver.SerdeForIndexBtree(coercionRangeTypes.CoercionTypes[0], statement.StatementRawInfo);
				var stateMgmtSettings = services.StateMgmtSettingsProvider.GetIndex(statement.StatementRawInfo, AppliesTo.INDEX_SORTED);
				eventTableFactory = new PropertySortedFactoryFactoryForge(0, subqueryNumber, false, indexedProp, viewableEventType, coercionRangeTypes, serde, stateMgmtSettings);
				hashCoercionDesc = new CoercionDesc(false, null);
				rangeCoercionDesc = coercionRangeTypes;
			}
			else {
				var indexedKeyProps = hashKeys.Keys.ToArray();
				var coercionKeyTypes = SubordPropUtil.GetCoercionTypes(hashKeys.Values);
				var multiKeyPlan = MultiKeyPlanner.PlanMultiKey(coercionKeyTypes, false, statement.StatementRawInfo, services.SerdeResolver);
				additionalForgeables.AddAll(multiKeyPlan.MultiKeyForgeables);
				hashMultikeyClasses = multiKeyPlan.ClassRef;
				var indexedRangeProps = rangeKeys.Keys.ToArray();
				var coercionRangeTypes = CoercionUtil.GetCoercionTypesRange(viewableEventType, rangeKeys, outerEventTypes);
				var rangeSerdes = new DataInputOutputSerdeForge[coercionRangeTypes.CoercionTypes.Length];
				for (var i = 0; i < coercionRangeTypes.CoercionTypes.Length; i++) {
					rangeSerdes[i] = services.SerdeResolver.SerdeForIndexBtree(coercionRangeTypes.CoercionTypes[i], statement.StatementRawInfo);
				}

				eventTableFactory = new PropertyCompositeEventTableFactoryFactoryForge(
					0,
					subqueryNumber,
					false,
					indexedKeyProps,
					coercionKeyTypes,
					hashMultikeyClasses,
					indexedRangeProps,
					coercionRangeTypes.CoercionTypes,
					rangeSerdes,
					viewableEventType);
				hashCoercionDesc = CoercionUtil.GetCoercionTypesHash(viewableEventType, indexedKeyProps, hashKeyList);
				rangeCoercionDesc = coercionRangeTypes;
			}

			var subqTableLookupStrategyFactory = SubordinateTableLookupStrategyUtil.GetLookupStrategy(
				outerEventTypes,
				hashKeyList,
				hashCoercionDesc,
				hashMultikeyClasses,
				rangeKeyList,
				rangeCoercionDesc,
				inKeywordSingleIdxKeys,
				inKeywordMultiIdxKey,
				false);

			return new SubqueryIndexForgeDesc(eventTableFactory, subqTableLookupStrategyFactory, additionalForgeables);
		}

		private static StreamTypeService GetDeclaredExprTypeService(
			ExprDeclaredNode[] declaredExpressions,
			IDictionary<ExprDeclaredNode, IList<ExprDeclaredNode>> declaredExpressionCallHierarchy,
			string[] outerStreamNames,
			EventType[] outerEventTypesSelect,
			ExprSubselectNode subselect,
			string subexpressionStreamName,
			EventType eventType)
		{
			// Find that subselect within that any of the expression declarations
			foreach (var declaration in declaredExpressions) {
				var visitor = new ExprNodeSubselectDeclaredNoTraverseVisitor(declaration);
				visitor.Reset();
				declaration.AcceptNoVisitParams(visitor);
				if (!visitor.Subselects.Contains(subselect)) {
					continue;
				}

				// no type service for "alias"
				if (declaration.Prototype.IsAlias) {
					return null;
				}

				// subselect found - compute outer stream names
				// initialize from the outermost provided stream names
				IDictionary<string, int> outerStreamNamesMap = new LinkedHashMap<string, int>();
				var count = 0;
				foreach (var outerStreamName in outerStreamNames) {
					outerStreamNamesMap.Put(outerStreamName, count++);
				}

				// give each declared expression a chance to change the names (unless alias expression)
				var outerStreamNamesForSubselect = outerStreamNamesMap;
				var callers = declaredExpressionCallHierarchy.Get(declaration);
				foreach (var caller in callers) {
					outerStreamNamesForSubselect = caller.GetOuterStreamNames(outerStreamNamesForSubselect);
				}

				outerStreamNamesForSubselect = declaration.GetOuterStreamNames(outerStreamNamesForSubselect);

				// compile a new StreamTypeService for use in validating that particular subselect
				var eventTypes = new EventType[outerStreamNamesForSubselect.Count + 1];
				var streamNames = new string[outerStreamNamesForSubselect.Count + 1];
				eventTypes[0] = eventType;
				streamNames[0] = subexpressionStreamName;
				count = 0;
				foreach (var entry in outerStreamNamesForSubselect) {
					eventTypes[count + 1] = outerEventTypesSelect[entry.Value];
					streamNames[count + 1] = entry.Key;
					count++;
				}

				var availableTypes = new StreamTypeServiceImpl(eventTypes, streamNames, new bool[eventTypes.Length], false, false);
				availableTypes.RequireStreamNames = true;
				return availableTypes;
			}

			return null;
		}
	}
} // end of namespace
