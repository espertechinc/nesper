///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.join.exec.composite
{
    using DataMap = IDictionary<string, object>;
    using AnyMap = IDictionary<object, object>;

    public class CompositeIndexEnterRemoveRange
        : CompositeIndexEnterRemove
    {
        private readonly EventPropertyGetter _propertyGetter;
        private readonly Type _coercionType;
        private HashSet<EventBean> _nullKeys;
        private CompositeIndexEnterRemove _next;
    
        public CompositeIndexEnterRemoveRange(EventType eventType, String rangeProp, Type coercionType) {
            _propertyGetter = EventBeanUtility.GetAssertPropertyGetter(eventType, rangeProp);
            _coercionType = coercionType;
        }
    
        public void SetNext(CompositeIndexEnterRemove next) {
            _next = next;
        }

        public void GetAll(ICollection<EventBean> result, AnyMap parent)
        {
            if (_next == null) {
                var eventMap = parent;
                foreach (var entry in eventMap)
                {
                    result.AddAll(entry.Value as ICollection<EventBean>);
                }
            }
            else {
                var eventMap = parent;
                foreach (var entry in eventMap) {
                    _next.GetAll(result, entry.Value as AnyMap);
                }
            }
            if (_nullKeys != null) {
                result.AddAll(_nullKeys);
            }
        }
    
        public void Enter(EventBean theEvent, AnyMap parent) {
            Object sortable = _propertyGetter.Get(theEvent);
    
            if (sortable == null) {
                if (_nullKeys == null) {
                    _nullKeys = new HashSet<EventBean>();
                }
                _nullKeys.Add(theEvent);
                return;
            }
    
            sortable = EventBeanUtility.Coerce(sortable, _coercionType);
    
            // if this is a leaf, enter event
            if (_next == null) {
                var eventMap = parent;
                var events = eventMap.Get(sortable) as ICollection<EventBean>;
                if (events == null)
                {
                    events = new HashSet<EventBean>();
                    eventMap.Put(sortable, events);
                }
    
                events.Add(theEvent);
            }
            else {
                AnyMap innerIndex = (AnyMap) parent.Get(sortable);
                if (innerIndex == null) {
                    innerIndex = new OrderedDictionary<object, object>();
                    parent.Put(sortable, innerIndex);
                }
                _next.Enter(theEvent, innerIndex);
            }
        }

        public void Remove(EventBean theEvent, AnyMap parent)
        {
            Object sortable = _propertyGetter.Get(theEvent);
    
            if (sortable == null) {
                if (_nullKeys != null) {
                    _nullKeys.Remove(theEvent);
                }
                return;
            }
    
            sortable = EventBeanUtility.Coerce(sortable, _coercionType);
    
            // if this is a leaf, remove event
            if (_next == null) {
                var eventMap = parent;
                if (eventMap == null) {
                    return;
                }

                var events = eventMap.Get(sortable) as ICollection<EventBean>;
                if (events == null)
                {
                    return;
                }
    
                if (!events.Remove(theEvent))
                {
                    return;
                }
    
                if (events.IsEmpty())
                {
                    parent.Remove(sortable);
                }
            }
            else {
                var innerIndex = (AnyMap) parent.Get(sortable);
                if (innerIndex == null) {
                    return;
                }
                _next.Remove(theEvent, innerIndex);
                if (innerIndex.IsEmpty()) {
                    parent.Remove(sortable);
                }
            }
        }
    }
}
