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
    public class CompositeIndexEnterRemoveKeyed : CompositeIndexEnterRemove
    {
        private readonly EventPropertyValueGetter _hashGetter;
        private CompositeIndexEnterRemove _next;

        public CompositeIndexEnterRemoveKeyed(EventPropertyValueGetter hashGetter)
        {
            this._hashGetter = hashGetter;
        }

        public CompositeIndexEnterRemove Next {
            get => this._next;
            set => this._next = value;
        }

        public void Enter(
            EventBean theEvent,
            IDictionary<object, CompositeIndexEntry> parent)
        {
            var mk = _hashGetter.Get(theEvent);
            var innerIndex = parent.Get(mk);
            if (innerIndex == null) {
                innerIndex = new CompositeIndexEntry(new OrderedDictionary<object, CompositeIndexEntry>());
                parent.Put(mk, innerIndex);
            }

            _next.Enter(theEvent, innerIndex.AssertIndex());
        }

        public void Remove(
            EventBean theEvent,
            IDictionary<object, CompositeIndexEntry> parent)
        {
            var mk = _hashGetter.Get(theEvent);
            var innerIndex = parent.Get(mk);
            if (innerIndex == null) {
                return;
            }

            var branches = innerIndex.AssertIndex();
            _next.Remove(theEvent, branches);
            if (branches.IsEmpty()) {
                parent.Remove(mk);
            }
        }

        public void GetAll(
            ISet<EventBean> result,
            IDictionary<object, CompositeIndexEntry> parent)
        {
            //IDictionary<HashableMultiKey, IDictionary> map = parent;
            foreach (var entry in parent) {
                entry.Value.AssertIndex();
                _next.GetAll(result, entry.Value.Index);
            }
        }
    }
} // end of namespace