///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.epl.join.exec.@base;

namespace com.espertech.esper.epl.join.exec.composite
{
    public class CompositeIndexLookupFactory
    {
        public static CompositeIndexLookup Make(Object[] keyValues, IList<RangeIndexLookupValue> rangeValues, IList<Type> rangeCoercion)
        {
            // construct chain
            var queries = new List<CompositeIndexLookup>();
            if (keyValues != null && keyValues.Length > 0) {
                queries.Add(new CompositeIndexLookupKeyed(keyValues));
            }
            for (int i = 0; i < rangeValues.Count; i++) {
                queries.Add(new CompositeIndexLookupRange(rangeValues[i], rangeCoercion[i]));
            }
    
            // Hook up as chain for remove
            CompositeIndexLookup last = null;
            foreach (CompositeIndexLookup action in queries) {
                if (last != null) {
                    last.SetNext(action);
                }
                last = action;
            }
            return queries[0];
        }
    }
}
