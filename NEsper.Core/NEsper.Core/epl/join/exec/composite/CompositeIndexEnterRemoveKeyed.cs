///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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

    public class CompositeIndexEnterRemoveKeyed 
        : CompositeIndexEnterRemove
    {
        private readonly IList<EventPropertyGetter> _propertyGetters;
        private readonly IList<Type> _keyCoercionTypes;
        private CompositeIndexEnterRemove _next;
    
        public CompositeIndexEnterRemoveKeyed(EventType eventType, IList<string> keysProps, IList<Type> keyCoercionTypes) {
            _keyCoercionTypes = keyCoercionTypes;
            _propertyGetters = new EventPropertyGetter[keysProps.Count];
            for (int i = 0; i < keysProps.Count; i++)
            {
                _propertyGetters[i] = EventBeanUtility.GetAssertPropertyGetter(eventType, keysProps[i]);
            }
        }
    
        public void SetNext(CompositeIndexEnterRemove next)
        {
            _next = next;
        }
    
        public void Enter(EventBean theEvent, AnyMap parent) {
            var mk = EventBeanUtility.GetMultiKey(theEvent, _propertyGetters, _keyCoercionTypes);
            var innerIndex = (AnyMap)parent.Get(mk);
            if (innerIndex == null) {
                innerIndex = new OrderedDictionary<Object, Object>();
                parent.Put(mk, innerIndex);
            }
            _next.Enter(theEvent, innerIndex);
        }

        public void Remove(EventBean theEvent, AnyMap parent)
        {
            var mk = EventBeanUtility.GetMultiKey(theEvent, _propertyGetters, _keyCoercionTypes);
            var innerIndex = (AnyMap)parent.Get(mk);
            if (innerIndex == null) {
                return;
            }
            _next.Remove(theEvent, innerIndex);
            if (innerIndex.IsEmpty()) {
                parent.Remove(mk);
            }
        }

        public void GetAll(ICollection<EventBean> result, AnyMap parent)
        {
            var map = parent;
            foreach (var entry in map)
            {
                _next.GetAll(result, entry.Value as AnyMap);
            }
        }
    }
}
