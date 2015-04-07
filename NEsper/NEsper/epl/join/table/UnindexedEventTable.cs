///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.join.table
{
    /// <summary>
    /// Simple table of events without an index.
    /// </summary>
    public class UnindexedEventTable : EventTable
    {
        private readonly int _streamNum;
        private readonly ISet<EventBean> _eventSet = new LinkedHashSet<EventBean>();
    
        /// <summary>Ctor. </summary>
        /// <param name="streamNum">is the indexed stream's number</param>
        public UnindexedEventTable(int streamNum)
        {
            _streamNum = streamNum;
        }
    
        public void Clear()
        {
            _eventSet.Clear();
        }
    
        public void AddRemove(EventBean[] newData, EventBean[] oldData)
        {
            Instrument.With(
                i => i.QIndexAddRemove(this, newData, oldData),
                i => i.AIndexAddRemove(),
                () =>
                {
                    newData.ForEach(ev => _eventSet.Add(ev));
                    oldData.ForEach(ev => _eventSet.Remove(ev));
                });
        }
    
        public void Add(EventBean[] events)
        {
            if (events != null && events.Length > 0) {
                Instrument.With(
                    i => i.QIndexAdd(this, events),
                    i => i.AIndexAdd(),
                    () => events.ForEach(ev => _eventSet.Add(ev)));
            }
        }
    
        public void Remove(EventBean[] events)
        {
            if (events != null && events.Length > 0)
            {
                Instrument.With(
                    i => i.QIndexRemove(this, events),
                    i => i.AIndexRemove(),
                    () => events.ForEach(ev => _eventSet.Remove(ev)));
            }
        }

        public void Add(EventBean @event)
        {
            _eventSet.Add(@event);
        }

        public void Remove(EventBean @event)
        {
            _eventSet.Remove(@event);
        }

        public bool IsEmpty()
        {
            return _eventSet.IsEmpty();
        }

        /// <summary>Returns events in table. </summary>
        /// <value>all events</value>
        public ISet<EventBean> EventSet
        {
            get { return _eventSet; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<EventBean> GetEnumerator()
        {
            return _eventSet.GetEnumerator();
        }
    
        public override String ToString()
        {
            return ToQueryPlan();
        }
    
        public String ToQueryPlan()
        {
            return GetType().Name + " streamNum=" + _streamNum;
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
                    null, false, false, _streamNum, null, EventTableOrganization.EventTableOrganizationType.UNORGANIZED);
            }
        }
    }
}
