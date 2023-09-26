///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.join.exec.util;

namespace com.espertech.esper.common.@internal.epl.join.exec.composite
{
    public class CompositeIndexLookupFactory
    {
        public static CompositeIndexLookup Make(
            object[] keyValues,
            MultiKeyFromObjectArray multiKeyTransform,
            RangeIndexLookupValue[] rangeValues,
            Type[] rangeCoercion)
        {
            // construct chain
            IList<CompositeIndexLookup> queries = new List<CompositeIndexLookup>();
            if (keyValues != null && keyValues.Length > 0) {
                queries.Add(new CompositeIndexLookupKeyed(keyValues, multiKeyTransform));
            }

            for (var i = 0; i < rangeValues.Length; i++) {
                queries.Add(new CompositeIndexLookupRange(rangeValues[i], rangeCoercion[i]));
            }

            // Hook up as chain for remove
            CompositeIndexLookup last = null;
            foreach (var action in queries) {
                if (last != null) {
                    last.Next = action;
                }

                last = action;
            }

            return queries[0];
        }
    }
} // end of namespace