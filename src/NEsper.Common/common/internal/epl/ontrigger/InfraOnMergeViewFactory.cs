///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.lookupplansubord;
using com.espertech.esper.common.@internal.epl.namedwindow.core;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.common.@internal.epl.ontrigger
{
    /// <summary>
    ///     View for the on-delete statement that handles removing events from a named window.
    /// </summary>
    public class InfraOnMergeViewFactory : InfraOnExprBaseViewFactory
    {
        public InfraOnMergeViewFactory(
            EventType namedWindowEventType,
            InfraOnMergeHelper onMergeHelper)
            : base(namedWindowEventType)
        {
            OnMergeHelper = onMergeHelper;
        }

        public InfraOnMergeHelper OnMergeHelper { get; }

        public override InfraOnExprBaseViewResult MakeNamedWindow(
            SubordWMatchExprLookupStrategy lookupStrategy,
            NamedWindowRootViewInstance namedWindowRootViewInstance,
            AgentInstanceContext agentInstanceContext)
        {
            View view;
            if (OnMergeHelper.InsertUnmatched != null) {
                view = new OnExprViewNamedWindowMergeInsertUnmatched(
                    namedWindowRootViewInstance,
                    agentInstanceContext,
                    this);
            }
            else {
                view = new OnExprViewNamedWindowMerge(
                    lookupStrategy,
                    namedWindowRootViewInstance,
                    agentInstanceContext,
                    this);
            }

            return new InfraOnExprBaseViewResult(view, null);
        }

        public override InfraOnExprBaseViewResult MakeTable(
            SubordWMatchExprLookupStrategy lookupStrategy,
            TableInstance tableInstance,
            AgentInstanceContext agentInstanceContext)
        {
            View view;
            if (OnMergeHelper.InsertUnmatched != null) {
                view = new OnExprViewTableMergeInsertUnmatched(tableInstance, agentInstanceContext, this);
            }
            else {
                view = new OnExprViewTableMerge(lookupStrategy, tableInstance, agentInstanceContext, this);
            }

            return new InfraOnExprBaseViewResult(view, null);
        }
    }
} // end of namespace