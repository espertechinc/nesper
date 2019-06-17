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
        public static IComparer<T> Default<T>()
        {
            return new DefaultComparer<T>();
        }

        public class DefaultComparer<T> : IComparer<T>
        {
            public int Compare(T x, T y)
            {
                return ((IComparable) x).CompareTo(y);
            }
        }
    }
}