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
	public class SubSelectHelperActivations {

	    public static IDictionary<ExprSubselectNode, SubSelectActivationPlan> CreateSubSelectActivation(IList<FilterSpecCompiled> filterSpecCompileds, IList<NamedWindowConsumerStreamSpec> namedWindowConsumers, StatementBaseInfo statement, StatementCompileTimeServices services)
	            {
	        IDictionary<ExprSubselectNode, SubSelectActivationPlan> result = new LinkedHashMap<ExprSubselectNode,  SubSelectActivationPlan>();

	        // Process all subselect expression nodes
	        foreach (ExprSubselectNode subselect in statement.StatementSpec.SubselectNodes) {
	            StatementSpecCompiled statementSpec = subselect.StatementSpecCompiled;
	            StreamSpecCompiled streamSpec = statementSpec.StreamSpecs[0];
	            int subqueryNumber = subselect.SubselectNumber;
	            if (subqueryNumber == -1) {
	                throw new IllegalStateException("Unexpected subquery");
	            }
	            ViewFactoryForgeArgs args = new ViewFactoryForgeArgs(-1, true, subqueryNumber, streamSpec.Options, null, statement.StatementRawInfo, services);

	            if (streamSpec is FilterStreamSpecCompiled) {
	                FilterStreamSpecCompiled filterStreamSpec = (FilterStreamSpecCompiled) statementSpec.StreamSpecs[0];

	                // Register filter, create view factories
	                ViewableActivatorForge activatorDeactivator = new ViewableActivatorFilterForge(filterStreamSpec.FilterSpecCompiled, false, null, true, subqueryNumber);
	                IList<ViewFactoryForge> forges = ViewFactoryForgeUtil.CreateForges(streamSpec.ViewSpecs, args, filterStreamSpec.FilterSpecCompiled.ResultEventType);
	                EventType eventType = forges.IsEmpty() ? filterStreamSpec.FilterSpecCompiled.ResultEventType : forges.Get(forges.Count - 1).EventType;
	                subselect.RawEventType = eventType;
	                filterSpecCompileds.Add(filterStreamSpec.FilterSpecCompiled);

	                // Add lookup to list, for later starts
	                result.Put(subselect, new SubSelectActivationPlan(filterStreamSpec.FilterSpecCompiled.ResultEventType, forges, activatorDeactivator, streamSpec));
	            } else if (streamSpec is TableQueryStreamSpec) {
	                TableQueryStreamSpec table = (TableQueryStreamSpec) streamSpec;
	                ExprNode filter = ExprNodeUtilityMake.ConnectExpressionsByLogicalAndWhenNeeded(table.FilterExpressions);
	                ViewableActivatorForge viewableActivator = new ViewableActivatorTableForge(table.Table, filter);
	                result.Put(subselect, new SubSelectActivationPlan(table.Table.InternalEventType, Collections.GetEmptyList<object>(), viewableActivator, streamSpec));
	                subselect.RawEventType = table.Table.InternalEventType;
	            } else {
	                NamedWindowConsumerStreamSpec namedSpec = (NamedWindowConsumerStreamSpec) statementSpec.StreamSpecs[0];
	                namedWindowConsumers.Add(namedSpec);
	                NamedWindowMetaData nwinfo = namedSpec.NamedWindow;

	                EventType namedWindowType = nwinfo.EventType;
	                if (namedSpec.OptPropertyEvaluator != null) {
	                    namedWindowType = namedSpec.OptPropertyEvaluator.FragmentEventType;
	                }

	                // if named-window index sharing is disabled (the default) or filter expressions are provided then consume the insert-remove stream
	                bool disableIndexShare = HintEnum.DISABLE_WINDOW_SUBQUERY_INDEXSHARE.GetHint(statement.StatementRawInfo.Annotations) != null;
	                bool processorDisableIndexShare = !namedSpec.NamedWindow.IsEnableIndexShare;
	                if (disableIndexShare && namedSpec.NamedWindow.IsVirtualDataWindow) {
	                    disableIndexShare = false;
	                }

	                if (!namedSpec.FilterExpressions.IsEmpty() || processorDisableIndexShare || disableIndexShare) {
	                    ViewableActivatorForge activatorNamedWindow = new ViewableActivatorNamedWindowForge(namedSpec, nwinfo, null, null, true, namedSpec.OptPropertyEvaluator);
	                    IList<ViewFactoryForge> forges = ViewFactoryForgeUtil.CreateForges(streamSpec.ViewSpecs, args, namedWindowType);
	                    subselect.RawEventType = forges.IsEmpty() ? namedWindowType : forges.Get(forges.Count - 1).EventType;
	                    result.Put(subselect, new SubSelectActivationPlan(namedWindowType, forges, activatorNamedWindow, streamSpec));
	                } else {
	                    // else if there are no named window stream filter expressions and index sharing is enabled
	                    IList<ViewFactoryForge> forges = ViewFactoryForgeUtil.CreateForges(streamSpec.ViewSpecs, args, namedWindowType);
	                    subselect.RawEventType = namedWindowType;
	                    ViewableActivatorForge activatorNamedWindow = new ViewableActivatorSubselectNoneForge(namedWindowType);
	                    result.Put(subselect, new SubSelectActivationPlan(namedWindowType, forges, activatorNamedWindow, streamSpec));
	                }
	            }
	        }

	        return result;
	    }
	}
} // end of namespace