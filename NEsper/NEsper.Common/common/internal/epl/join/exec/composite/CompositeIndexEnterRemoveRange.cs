///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.join.exec.composite
{
    using CompositeDictionary = IDictionary<object, CompositeIndexEntry>;

    public class CompositeIndexEnterRemoveRange : CompositeIndexEnterRemove
    {
        private readonly EventPropertyValueGetter propertyGetter;
        private HashSet<EventBean> nullKeys;
        private CompositeIndexEnterRemove next;

        public CompositeIndexEnterRemoveRange(EventPropertyValueGetter propertyGetter)
        {
            this.propertyGetter = propertyGetter;
        }

        public CompositeIndexEnterRemove Next {
            get => this.next;
            set => this.next = value;
        }

        public void GetAll(
            ISet<EventBean> result,
            CompositeDictionary parent)
        {
            if (next == null) {
                foreach (var value in parent.Values) {
                    result.AddAll(value.AssertCollection());
                }
            }
            else {
                foreach (var value in parent.Values) {
                    next.GetAll(result, value.AssertIndex());
                }
            }

            if (nullKeys != null) {
                result.AddAll(nullKeys);
            }
        }

        public void Enter(
            EventBean theEvent,
            CompositeDictionary parent)
        {
            var sortable = propertyGetter.Get(theEvent);
            if (sortable == null) {
                if (nullKeys == null) {
                    nullKeys = new HashSet<EventBean>();
                }

                nullKeys.Add(theEvent);
                return;
            }

            // if this is a leaf, enter event
            if (next == null) {
                var eventMap = parent;
                var entry = parent.Get(sortable);
                if (entry == null) {
                    entry = new CompositeIndexEntry(new HashSet<EventBean>());
                    eventMap.Put(sortable, entry);
                }
                else {
                    entry.AssertCollection();
                }

                entry.Collection.Add(theEvent);
            }
            else {
                var innerEntry = parent.Get(sortable);
                if (innerEntry == null) {
                    innerEntry = new CompositeIndexEntry(new OrderedDictionary<object, CompositeIndexEntry>());
                    parent.Put(sortable, innerEntry);
                }
                else {
                    innerEntry.AssertIndex();
                }

                next.Enter(theEvent, innerEntry.Index);
            }
        }

        public void Remove(
            EventBean theEvent,
            CompositeDictionary parent)
        {
            var sortable = propertyGetter.Get(theEvent);
            if (sortable == null) {
                nullKeys?.Remove(theEvent);
                return;
            }

            // if this is a leaf, remove event
            if (next == null) {
                var eventMap = parent;
                var entry = eventMap?.Get(sortable);
                if (entry == null) {
                    return;
                }

                var events = entry.AssertCollection();
                if (!events.Remove(theEvent)) {
                    return;
                }

                if (events.IsEmpty()) {
                    parent.Remove(sortable);
                }
            }
            else {
                var innerEntry = parent.Get(sortable);
                if (innerEntry == null) {
                    return;
                }

                var innerIndex = innerEntry.AssertIndex();
                next.Remove(theEvent, innerIndex);
                if (innerIndex.IsEmpty()) {
                    parent.Remove(sortable);
                }
            }
        }
    }
} // end of namespace