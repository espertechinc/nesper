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
        private readonly object[] _keys;
        private CompositeIndexLookup _next;
        private readonly MultiKeyFromObjectArray _multiKeyTransform;

        public CompositeIndexLookupKeyed(object[] keys, MultiKeyFromObjectArray multiKeyTransform)
        {
            this._keys = keys;
            this._multiKeyTransform = multiKeyTransform;
        }

        public MultiKeyFromObjectArray MultiKeyTransform => _multiKeyTransform;

        public CompositeIndexLookup Next {
            set { this._next = value; }
        }

        public void Lookup(
            IDictionary<object, CompositeIndexEntry> parent,
            ISet<EventBean> result,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            var key = _multiKeyTransform.From(_keys);
            var innerEntry = parent.Get(key);
            if (innerEntry == null) {
                return;
            }

            var innerIndex = innerEntry.AssertIndex();
            _next.Lookup(innerIndex, result, postProcessor);
        }
    }
} // end of namespace