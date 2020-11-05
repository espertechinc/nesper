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
using com.espertech.esper.common.@internal.context.util;

namespace com.espertech.esper.common.@internal.epl.output.view
{
    public class OutputProcessViewAfterStateImpl : OutputProcessViewAfterState
    {
        private readonly int? _afterConditionNumberOfEvents;
        private readonly long? _afterConditionTime;
        private int _afterConditionEventsFound;
        private bool _isAfterConditionSatisfied;

        public OutputProcessViewAfterStateImpl(
            long? afterConditionTime,
            int? afterConditionNumberOfEvents)
        {
            _afterConditionTime = afterConditionTime;
            _afterConditionNumberOfEvents = afterConditionNumberOfEvents;
        }

        /// <summary>
        ///     Returns true if the after-condition is satisfied.
        /// </summary>
        /// <param name="newEvents">is the view new events</param>
        /// <param name="statementContext">the statement context</param>
        /// <returns>indicator for output condition</returns>
        public bool CheckUpdateAfterCondition(
            EventBean[] newEvents,
            StatementContext statementContext)
        {
            return _isAfterConditionSatisfied ||
                   CheckAfterCondition(newEvents?.Length ?? 0, statementContext);
        }

        /// <summary>
        ///     Returns true if the after-condition is satisfied.
        /// </summary>
        /// <param name="newEvents">is the join new events</param>
        /// <param name="statementContext">the statement context</param>
        /// <returns>indicator for output condition</returns>
        public bool CheckUpdateAfterCondition(
            ISet<MultiKeyArrayOfKeys<EventBean>> newEvents,
            StatementContext statementContext)
        {
            return _isAfterConditionSatisfied ||
                   CheckAfterCondition(newEvents?.Count ?? 0, statementContext);
        }

        /// <summary>
        ///     Returns true if the after-condition is satisfied.
        /// </summary>
        /// <param name="newOldEvents">is the new and old events pair</param>
        /// <param name="statementContext">the statement context</param>
        /// <returns>indicator for output condition</returns>
        public bool CheckUpdateAfterCondition(
            UniformPair<EventBean[]> newOldEvents,
            StatementContext statementContext)
        {
            return _isAfterConditionSatisfied ||
                   CheckAfterCondition(
                       newOldEvents == null ? 0 : newOldEvents.First?.Length ?? 0,
                       statementContext);
        }

        public void Destroy()
        {
            // no action required
        }

        private bool CheckAfterCondition(
            int numOutputEvents,
            StatementContext statementContext)
        {
            if (_afterConditionTime != null) {
                var time = statementContext.TimeProvider.Time;
                if (time < _afterConditionTime) {
                    return false;
                }

                _isAfterConditionSatisfied = true;
                return true;
            }

            if (_afterConditionNumberOfEvents != null) {
                _afterConditionEventsFound += numOutputEvents;
                if (_afterConditionEventsFound <= _afterConditionNumberOfEvents) {
                    return false;
                }

                _isAfterConditionSatisfied = true;
                return true;
            }

            _isAfterConditionSatisfied = true;
            return true;
        }
    }
} // end of namespace