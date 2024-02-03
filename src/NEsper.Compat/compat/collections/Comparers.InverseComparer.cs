///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.compat.collections
{
    public static partial class Comparers
    {
        public static IComparer<T> Inverse<T>(this IComparer<T> baseComparer)
        {
            return new InverseComparer<T>(baseComparer);
        }

        public static IComparer<T> Inverse<T>()
        {
            return new InverseComparer<T>(Comparer<T>.Default);
        }

        internal class InverseComparer<T> : IComparer<T>
        {
            private readonly IComparer<T> _baseComparer;

            public InverseComparer(IComparer<T> baseComparer)
            {
                _baseComparer = baseComparer;
            }

            public int Compare(T x, T y)
            {
                return -_baseComparer.Compare(x, y);
            }
        }
    }
}