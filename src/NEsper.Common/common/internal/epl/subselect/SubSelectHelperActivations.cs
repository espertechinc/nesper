///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.activator;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.subquery;
using com.espertech.esper.common.@internal.epl.namedwindow.path;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.subselect
{
	public class SubSelectHelperActivations
	{

		public static SubSelectActivationDesc CreateSubSelectActivation(
			bool fireAndForget,
			IList<FilterSpecCompiled> filterSpecCompileds,
			IList<NamedWindowConsumerStreamSpec> namedWindowConsumers,
			StatementBaseInfo statement,
			StatementCompileTimeServices services)
		{
			IDictionary<ExprSubselectNode, SubSelectActivationPlan> result = new LinkedHashMap<ExprSubselectNode, SubSelectActivationPlan>();
			IList<StmtClassForgeableFactory> additionalForgeables = new List<StmtClassForgeableFactory>();

			// Process all subselect expression nodes
			foreach (var subselect in statement.StatementSpec.SubselectNodes) {
				var statementSpec = subselect.StatementSpecCompiled;
				var streamSpec = statementSpec.StreamSpecs[0];
				var subqueryNumber = subselect.SubselectNumber;
				if (subqueryNumber == -1) {
					throw new IllegalStateException("Unexpected subquery");
				}

				var args = new ViewFactoryForgeArgs(-1, true, subqueryNumber, streamSpec.Options, null, statement.StatementRawInfo, services);

				if (streamSpec is FilterStreamSpecCompiled) {
					if (services.IsFireAndForget) {
						throw new ExprValidationException("Fire-and-forget queries only allow subqueries against named windows and tables");
					}

					var filterStreamSpec = (FilterStreamSpecCompiled) statementSpec.StreamSpecs[0];

					// Register filter, create view factories
					ViewableActivatorForge activatorDeactivator;
					if (fireAndForget) {
						activatorDeactivator = new ViewableActivatorSubselectNoneForge(filterStreamSpec.FilterSpecCompiled.FilterForEventType);
					}
					else {
						activatorDeactivator = new ViewableActivatorFilterForge(filterStreamSpec.FilterSpecCompiled, false, null, true, subqueryNumber);
					}

					var viewForgeDesc = ViewFactoryForgeUtil.CreateForges(
						streamSpec.ViewSpecs,
						args,
						filterStreamSpec.FilterSpecCompiled.ResultEventType);
					var forges = viewForgeDesc.Forges;
					additionalForgeables.AddAll(viewForgeDesc.MultikeyForges);
					var eventType = forges.IsEmpty() ? filterStreamSpec.FilterSpecCompiled.ResultEventType : forges[forges.Count - 1].EventType;
					subselect.RawEventType = eventType;
					filterSpecCompileds.Add(filterStreamSpec.FilterSpecCompiled);

					// Add lookup to list, for later starts
					result.Put(
						subselect,
						new SubSelectActivationPlan(filterStreamSpec.FilterSpecCompiled.ResultEventType, forges, activatorDeactivator, streamSpec));
				}
				else if (streamSpec is TableQueryStreamSpec) {
					var table = (TableQueryStreamSpec) streamSpec;
					var filter = ExprNodeUtilityMake.ConnectExpressionsByLogicalAndWhenNeeded(table.FilterExpressions);
					ViewableActivatorForge viewableActivator;
					if (fireAndForget) {
						viewableActivator = new ViewableActivatorSubselectNoneForge(table.Table.PublicEventType);
					}
					else {
						viewableActivator = new ViewableActivatorTableForge(table.Table, filter);
					}

					result.Put(
						subselect,
						new SubSelectActivationPlan(table.Table.InternalEventType, EmptyList<ViewFactoryForge>.Instance, viewableActivator, streamSpec));
					subselect.RawEventType = table.Table.InternalEventType;
				}
				else {
					var namedSpec = (NamedWindowConsumerStreamSpec) statementSpec.StreamSpecs[0];
					namedWindowConsumers.Add(namedSpec);
					var nwinfo = namedSpec.NamedWindow;

					var namedWindowType = nwinfo.EventType;
					if (namedSpec.OptPropertyEvaluator != null) {
						namedWindowType = namedSpec.OptPropertyEvaluator.FragmentEventType;
					}

					// if named-window index sharing is disabled (the default) or filter expressions are provided then consume the insert-remove stream
					var disableIndexShare = HintEnum.DISABLE_WINDOW_SUBQUERY_INDEXSHARE.GetHint(statement.StatementRawInfo.Annotations) != null;
					var processorDisableIndexShare = !namedSpec.NamedWindow.IsEnableIndexShare;
					if (disableIndexShare && namedSpec.NamedWindow.IsVirtualDataWindow) {
						disableIndexShare = false;
					}

					if ((!namedSpec.FilterExpressions.IsEmpty() || processorDisableIndexShare || disableIndexShare) && (!services.IsFireAndForget)) {
						ExprNode filterEvaluator = null;
						if (!namedSpec.FilterExpressions.IsEmpty()) {
							filterEvaluator = ExprNodeUtilityMake.ConnectExpressionsByLogicalAndWhenNeeded(namedSpec.FilterExpressions);
						}

						ViewableActivatorForge activatorNamedWindow;
						if (fireAndForget) {
							activatorNamedWindow = new ViewableActivatorSubselectNoneForge(namedWindowType);
						} else {
							activatorNamedWindow = new ViewableActivatorNamedWindowForge(namedSpec, nwinfo, filterEvaluator, null, true, namedSpec.OptPropertyEvaluator);
						}

						var viewForgeDesc = ViewFactoryForgeUtil.CreateForges(streamSpec.ViewSpecs, args, namedWindowType);
						var forges = viewForgeDesc.Forges;
						additionalForgeables.AddAll(viewForgeDesc.MultikeyForges);
						subselect.RawEventType = forges.IsEmpty() ? namedWindowType : forges[forges.Count - 1].EventType;
						result.Put(subselect, new SubSelectActivationPlan(namedWindowType, forges, activatorNamedWindow, streamSpec));
					}
					else {
						// else if there are no named window stream filter expressions and index sharing is enabled
						var viewForgeDesc = ViewFactoryForgeUtil.CreateForges(streamSpec.ViewSpecs, args, namedWindowType);
						var forges = viewForgeDesc.Forges;
						additionalForgeables.AddAll(viewForgeDesc.MultikeyForges);
						subselect.RawEventType = namedWindowType;
						ViewableActivatorForge activatorNamedWindow = new ViewableActivatorSubselectNoneForge(namedWindowType);
						result.Put(subselect, new SubSelectActivationPlan(namedWindowType, forges, activatorNamedWindow, streamSpec));
					}
				}
			}

			return new SubSelectActivationDesc(result, additionalForgeables);
		}
	}
} // end of namespace
