///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.lookupplansubord;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.ontrigger
{
    public abstract class OnExprViewTableBase : ViewSupport,
        StopCallback
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        internal readonly bool acquireWriteLock;
        internal readonly AgentInstanceContext agentInstanceContext;

        internal readonly SubordWMatchExprLookupStrategy lookupStrategy;
        internal readonly TableInstance tableInstance;

        internal OnExprViewTableBase(
            SubordWMatchExprLookupStrategy lookupStrategy,
            TableInstance tableInstance,
            AgentInstanceContext agentInstanceContext,
            bool acquireWriteLock)
        {
            this.lookupStrategy = lookupStrategy;
            this.tableInstance = tableInstance;
            this.agentInstanceContext = agentInstanceContext;
            this.acquireWriteLock = acquireWriteLock;
        }

        /// <summary>
        ///     returns expr context.
        /// </summary>
        /// <returns>context</returns>
        public ExprEvaluatorContext ExprEvaluatorContext => agentInstanceContext;

        public override EventType EventType => tableInstance.Table.MetaData.PublicEventType;

        public void Stop()
        {
            Log.Debug(".stop");
        }

        public abstract void HandleMatching(
            EventBean[] triggerEvents,
            EventBean[] matchingEvents);

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            if (newData == null) {
                return;
            }

            if (acquireWriteLock) {
                using (tableInstance.TableLevelRWLock.WriteLock.Acquire()) {
                    ProcessLocked(newData);
                }
            }
            else {
                using (tableInstance.TableLevelRWLock.ReadLock.Acquire()) {
                    ProcessLocked(newData);
                }
            }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return CollectionUtil.NULL_EVENT_ITERATOR;
        }

        private void ProcessLocked(EventBean[] newData)
        {
            if (newData.Length == 1) {
                Process(newData);
                return;
            }

            var eventsPerStream = new EventBean[1];
            foreach (var @event in newData) {
                eventsPerStream[0] = @event;
                Process(eventsPerStream);
            }
        }

        private void Process(EventBean[] events)
        {
            var eventsFound = lookupStrategy.Lookup(events, agentInstanceContext);
            HandleMatching(events, eventsFound);
        }
    }
} // end of namespace