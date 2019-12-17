///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.compat.collections
{
    public static partial class Comparers
    {
        public static IComparer<object> Collating()
        {
            return new CollatingComparer();
        }

        public class CollatingComparer : IComparer<object>
        {
            public int Compare(object o1, object o2)
            {
                if ((o1 is string s1) && (o2 is string s2)) {
                    return StringComparer.CurrentCulture.Compare(s1, s2);
                }
                else if ((o1 is IComparable c1) && (o2 is IComparable c2)) {
                    return c1.CompareTo(c2);
                }
                else {
                    throw new ArgumentException("unable to compare non-comparable values");
                }
            }
        }
    }
}