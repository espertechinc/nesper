///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.join.table
{
    /// <summary>
    /// Simple table of events without an index, based on a List implementation rather then 
    /// a set since we know there cannot be duplicates (such as a poll returning individual rows).
    /// </summary>
    public class UnindexedEventTableList : EventTable
    {
        private readonly IList<EventBean> _eventSet;
        private readonly int _streamNum;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="eventSet">is a list initializing the table</param>
        /// <param name="streamNum">The stream num.</param>
        public UnindexedEventTableList(IList<EventBean> eventSet, int streamNum)
        {
            _eventSet = eventSet;
            _streamNum = streamNum;
        }
    
        public void AddRemove(EventBean[] newData, EventBean[] oldData, ExprEvaluatorContext exprEvaluatorContext)
        {
            Instrument.With(
                i => i.QIndexAddRemove(this, newData, oldData),
                i => i.AIndexAddRemove(),
                () =>
                {
                    if (newData != null)
                    {
                        _eventSet.AddAll(newData);
                    }
                    if (oldData != null)
                    {
                        foreach (EventBean removeEvent in oldData)
                        {
                            _eventSet.Remove(removeEvent);
                        }
                    }
                });
        }
    
        public void Add(EventBean[] events, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (events != null && events.Length > 0)
            {
                Instrument.With(
                    i => i.QIndexAdd(this, events),
                    i => i.AIndexAdd(),
                    () => _eventSet.AddAll(events));
            }

        }
    
        public void Remove(EventBean[] events, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (events != null && events.Length > 0)
            {
                Instrument.With(
                    i => i.QIndexRemove(this, events),
                    i => i.AIndexRemove(),
                    () =>
                    {
                        foreach (EventBean removeEvent in events)
                        {
                            _eventSet.Remove(removeEvent);
                        }
                    });
            }
        }

        public void Add(EventBean @event, ExprEvaluatorContext exprEvaluatorContext)
        {
            _eventSet.Add(@event);
        }

        public void Remove(EventBean @event, ExprEvaluatorContext exprEvaluatorContext)
        {
            _eventSet.Remove(@event);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<EventBean> GetEnumerator()
        {
            if (_eventSet == null)
            {
                return CollectionUtil.NULL_EVENT_ITERATOR;
            }
            return _eventSet.GetEnumerator();
        }

        public bool IsEmpty()
        {
            return _eventSet.IsEmpty();
        }

        public override String ToString()
        {
            return ToQueryPlan();
        }
    
        public String ToQueryPlan() {
            return GetType().FullName;
        }
    
        public void Clear()
        {
            _eventSet.Clear();
        }

        public void Destroy()
        {
            Clear();
        }

        public int? NumberOfEvents
        {
            get { return _eventSet.Count; }
        }

        public int NumKeys
        {
            get { return 0; }
        }

        public object Index
        {
            get { return _eventSet; }
        }

        public EventTableOrganization Organization
        {
            get
            {
                return new EventTableOrganization(
                    null, false, false, _streamNum, null, EventTableOrganizationType.UNORGANIZED);
            }
        }

        public Type ProviderClass
        {
            get { return typeof (UnindexedEventTableList); }
        }
    }
}
