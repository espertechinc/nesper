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
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.join.exec.composite
{
    public class CompositeIndexLookupKeyed : CompositeIndexLookup
    {
        private readonly object[] keys;
        private CompositeIndexLookup next;

        public CompositeIndexLookupKeyed(object[] keys)
        {
            this.keys = keys;
        }

        public CompositeIndexLookup Next
        {
            set { this.next = value; }
        }

        private object GetKey()
        {
            if (keys.Length == 1)
            {
                return keys[0];
            }
            else
            {
                return new HashableMultiKey(keys);
            }
        }

        public void Lookup(
            IDictionary<object, CompositeIndexEntry> parent,
            ISet<EventBean> result,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            var key = GetKey();
            var innerEntry = parent.Get(key);
            if (innerEntry == null) {
                return;
            }

            var innerIndex = innerEntry.AssertIndex();
            next.Lookup(innerIndex, result, postProcessor);
        }
    }
} // end of namespace