///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.dataflow.util
{
    public class OperatorDependencyUtil
    {
        public static ICollection<int> Roots(IDictionary<int, OperatorDependencyEntry> dependencyEntryMap)
        {
            ICollection<int> roots = new HashSet<int>();
            foreach (KeyValuePair<int, OperatorDependencyEntry> entry in dependencyEntryMap)
            {
                if (entry.Value.Incoming.IsEmpty())
                {
                    roots.Add(entry.Key);
                }
            }
            return roots;
        }
    }
}
