///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.lookupplansubord;
using com.espertech.esper.common.@internal.epl.namedwindow.core;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.ontrigger
{
    /// <summary>
    ///     View for the on-select statement that handles selecting events from a named window.
    /// </summary>
    public class OnExprViewNamedWindowSelect : OnExprViewNameWindowBase
    {
        private readonly bool audit;
        private readonly bool isDelete;
        private readonly ISet<MultiKey<EventBean>> oldEvents = new HashSet<MultiKey<EventBean>>();
        private new readonly InfraOnSelectViewFactory parent;
        private readonly ResultSetProcessor resultSetProcessor;
        private readonly TableInstance tableInstanceInsertInto;

        public OnExprViewNamedWindowSelect(
            SubordWMatchExprLookupStrategy lookupStrategy,
            NamedWindowRootViewInstance rootView,
            AgentInstanceContext agentInstanceContext,
            InfraOnSelectViewFactory parent,
            ResultSetProcessor resultSetProcessor,
            bool audit,
            bool isDelete,
            TableInstance tableInstanceInsertInto)
            :
            base(lookupStrategy, rootView, agentInstanceContext)
        {
            this.parent = parent;
            this.resultSetProcessor = resultSetProcessor;
            this.audit = audit;
            this.isDelete = isDelete;
            this.tableInstanceInsertInto = tableInstanceInsertInto;
        }

        public override void HandleMatching(
            EventBean[] triggerEvents,
            EventBean[] matchingEvents)
        {
            agentInstanceContext.InstrumentationProvider.QInfraOnAction(
                OnTriggerType.ON_SELECT,
                triggerEvents,
                matchingEvents);

            // clear state from prior results
            resultSetProcessor.Clear();

            // build join result
            // use linked hash set to retain order of join results for last/first/window to work most intuitively
            var newEvents = BuildJoinResult(triggerEvents, matchingEvents);

            // process matches
            var pair = resultSetProcessor.ProcessJoinResult(newEvents, oldEvents, false);
            var newData = pair != null ? pair.First : null;

            // handle distinct and insert
            newData = InfraOnSelectUtil.HandleDistintAndInsert(
                newData,
                parent,
                agentInstanceContext,
                tableInstanceInsertInto,
                audit);

            // The on-select listeners receive the events selected
            if (newData != null && newData.Length > 0) {
                if (Child != null) {
                    // And post only if we have listeners/subscribers that need the data
                    var statementResultService = agentInstanceContext.StatementResultService;
                    if (statementResultService.IsMakeNatural || statementResultService.IsMakeSynthetic) {
                        Child.Update(newData, null);
                    }
                }
            }

            // clear state from prior results
            resultSetProcessor.Clear();

            // Events to delete are indicated via old data
            if (isDelete) {
                rootView.Update(null, matchingEvents);
            }

            agentInstanceContext.InstrumentationProvider.AInfraOnAction();
        }

        public static ISet<MultiKey<EventBean>> BuildJoinResult(
            EventBean[] triggerEvents,
            EventBean[] matchingEvents)
        {
            var events = new LinkedHashSet<MultiKey<EventBean>>();
            for (var i = 0; i < triggerEvents.Length; i++) {
                var triggerEvent = triggerEvents[0];
                if (matchingEvents != null) {
                    for (var j = 0; j < matchingEvents.Length; j++) {
                        var eventsPerStream = new EventBean[2];
                        eventsPerStream[0] = matchingEvents[j];
                        eventsPerStream[1] = triggerEvent;
                        events.Add(new MultiKey<EventBean>(eventsPerStream));
                    }
                }
            }

            return events;
        }

        public override EventType EventType {
            get {
                if (resultSetProcessor != null) {
                    return resultSetProcessor.ResultEventType;
                }

                return rootView.EventType;
            }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return CollectionUtil.NULL_EVENT_ITERATOR;
        }
    }
} // end of namespace