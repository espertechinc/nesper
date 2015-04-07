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
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.epl.join.exec.composite
{
    using DataMap = IDictionary<string, object>;

    public class CompositeIndexLookupKeyed 
        : CompositeIndexLookup
    {
        private readonly Object[] _keys;
        private CompositeIndexLookup _next;
    
        public CompositeIndexLookupKeyed(Object[] keys) {
            _keys = keys;
        }

        public CompositeIndexLookup Next
        {
            set { _next = value; }
        }

        public void Lookup(IDictionary<object,object> parent, ICollection<EventBean> result)
        {
            var mk = new MultiKeyUntyped(_keys);
            var innerIndex = (IDictionary<object, object>)parent.Get(mk);
            if (innerIndex == null) {
                return;
            }
            _next.Lookup(innerIndex, result);        
        }
    }
}
