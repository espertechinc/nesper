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
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.events;
using com.espertech.esper.events.arr;

namespace com.espertech.esper.epl.join.table
{
    public class SingleReferenceEventTable : EventTable, EventTableAsSet
    {
        private readonly EventTableOrganization organization;
        private readonly Atomic<ObjectArrayBackedEventBean> eventReference;
    
        public SingleReferenceEventTable(EventTableOrganization organization, Atomic<ObjectArrayBackedEventBean> eventReference) {
            this.organization = organization;
            this.eventReference = eventReference;
        }
    
        public void AddRemove(EventBean[] newData, EventBean[] oldData) {
            throw new UnsupportedOperationException();
        }
    
        public void Add(EventBean[] events) {
            throw new UnsupportedOperationException();
        }
    
        public void Add(EventBean @event) {
            throw new UnsupportedOperationException();
        }
    
        public void Remove(EventBean[] events) {
            throw new UnsupportedOperationException();
        }
    
        public void Remove(EventBean @event) {
            throw new UnsupportedOperationException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<EventBean> GetEnumerator()
        {
            var eventBean = eventReference.Get();
            if (eventBean != null)
                yield return eventBean;
        }

        public bool IsEmpty()
        {
            return eventReference.Get() == null;
        }

        public void Clear()
        {
            throw new UnsupportedOperationException();
        }
    
        public string ToQueryPlan()
        {
            return "single-reference";
        }

        public int? NumberOfEvents
        {
            get { return eventReference.Get() == null ? 0 : 1; }
        }

        public int NumKeys
        {
            get { return 0; }
        }

        public object Index
        {
            get { return null; }
        }

        public EventTableOrganization Organization
        {
            get { return organization; }
        }

        public ISet<EventBean> AllValues() {
            EventBean @event = eventReference.Get();
            if (@event != null) {
                return Collections.SingletonSet<EventBean>(@event);
            }
            return Collections.GetEmptySet<EventBean>();
        }
    }
}
