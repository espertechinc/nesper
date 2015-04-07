///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;

namespace com.espertech.esper.epl.view
{
    public abstract class OutputProcessViewBaseWAfter : OutputProcessViewBase
    {
        private readonly int? _afterConditionNumberOfEvents;
        private readonly long? _afterConditionTime;
        private int _afterConditionEventsFound;

        protected bool IsAfterConditionSatisfied;

        protected OutputProcessViewBaseWAfter(ResultSetProcessor resultSetProcessor,
                                              long? afterConditionTime,
                                              int? afterConditionNumberOfEvents,
                                              bool afterConditionSatisfied)
            : base(resultSetProcessor)
        {
            _afterConditionTime = afterConditionTime;
            _afterConditionNumberOfEvents = afterConditionNumberOfEvents;
            IsAfterConditionSatisfied = afterConditionSatisfied;
        }

        /// <summary>
        /// Returns true if the after-condition is satisfied.
        /// </summary>
        /// <param name="newEvents">is the view new events</param>
        /// <param name="statementContext">The statement context.</param>
        /// <returns>indicator for output condition</returns>
        public bool CheckAfterCondition(EventBean[] newEvents,
                                        StatementContext statementContext)
        {
            return IsAfterConditionSatisfied ||
                   CheckAfterCondition(newEvents == null ? 0 : newEvents.Length, statementContext);
        }

        /// <summary>
        /// Returns true if the after-condition is satisfied.
        /// </summary>
        /// <param name="newEvents">is the join new events</param>
        /// <param name="statementContext">The statement context.</param>
        /// <returns>indicator for output condition</returns>
        public bool CheckAfterCondition(ICollection<MultiKey<EventBean>> newEvents,
                                        StatementContext statementContext)
        {
            return IsAfterConditionSatisfied ||
                   CheckAfterCondition(newEvents == null ? 0 : newEvents.Count, statementContext);
        }

        /// <summary>
        /// Returns true if the after-condition is satisfied.
        /// </summary>
        /// <param name="newOldEvents">is the new and old events pair</param>
        /// <param name="statementContext">The statement context.</param>
        /// <returns>indicator for output condition</returns>
        public bool CheckAfterCondition(UniformPair<EventBean[]> newOldEvents,
                                        StatementContext statementContext)
        {
            return IsAfterConditionSatisfied ||
                   CheckAfterCondition(
                       newOldEvents == null ? 0 : (newOldEvents.First == null ? 0 : newOldEvents.First.Length),
                       statementContext);
        }

        private bool CheckAfterCondition(int numOutputEvents,
                                         StatementContext statementContext)
        {
            if (_afterConditionTime != null)
            {
                long time = statementContext.TimeProvider.Time;
                if (time < _afterConditionTime)
                {
                    return false;
                }

                IsAfterConditionSatisfied = true;
                return true;
            }
            if (_afterConditionNumberOfEvents != null)
            {
                _afterConditionEventsFound += numOutputEvents;
                if (_afterConditionEventsFound <= _afterConditionNumberOfEvents)
                {
                    return false;
                }

                IsAfterConditionSatisfied = true;
                return true;
            }
            IsAfterConditionSatisfied = true;
            return true;
        }
    }
}