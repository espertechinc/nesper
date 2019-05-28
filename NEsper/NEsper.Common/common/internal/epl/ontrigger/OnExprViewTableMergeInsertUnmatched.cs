///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.ontrigger
{
    public class OnExprViewTableMergeInsertUnmatched : ViewSupport,
        StopCallback
    {
        private readonly AgentInstanceContext agentInstanceContext;
        private new readonly InfraOnMergeViewFactory parent;
        private readonly TableInstance tableInstance;

        public OnExprViewTableMergeInsertUnmatched(
            TableInstance tableInstance,
            AgentInstanceContext agentInstanceContext,
            InfraOnMergeViewFactory parent)
        {
            this.tableInstance = tableInstance;
            this.agentInstanceContext = agentInstanceContext;
            this.parent = parent;
        }

        public override EventType EventType => tableInstance.Table.MetaData.PublicEventType;

        public void Stop()
        {
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return EnumerationHelper.Empty<EventBean>();
        }

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            agentInstanceContext.InstrumentationProvider.QInfraOnAction(
                OnTriggerType.ON_MERGE, newData, CollectionUtil.EVENTBEANARRAY_EMPTY);

            if (newData == null) {
                agentInstanceContext.InstrumentationProvider.AInfraOnAction();
                return;
            }

            var statementResultService = agentInstanceContext.StatementResultService;
            var postResultsToListeners = statementResultService.IsMakeNatural || statementResultService.IsMakeSynthetic;
            OnExprViewTableChangeHandler changeHandlerAdded = null;
            if (postResultsToListeners) {
                changeHandlerAdded = new OnExprViewTableChangeHandler(tableInstance.Table);
            }

            var eventsPerStream =
                new EventBean[3]; // first:named window, second: trigger, third:before-update (optional)
            foreach (var trigger in newData) {
                eventsPerStream[1] = trigger;
                parent.OnMergeHelper.InsertUnmatched.Apply(
                    null, eventsPerStream, tableInstance, changeHandlerAdded, null, agentInstanceContext);

                // The on-delete listeners receive the events deleted, but only if there is interest
                if (postResultsToListeners) {
                    var postedNew = changeHandlerAdded.Events;
                    if (postedNew != null) {
                        Child.Update(postedNew, null);
                    }
                }
            }

            agentInstanceContext.InstrumentationProvider.AInfraOnAction();
        }
    }
} // end of namespace