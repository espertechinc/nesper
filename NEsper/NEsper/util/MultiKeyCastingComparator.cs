///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.collection;

namespace com.espertech.esper.util
{
    /// <summary>
    /// A comparator on multikeys. The multikeys must contain the same number of values.
    /// </summary>
    [Serializable]
    public sealed class MultiKeyCastingComparator 
        : IComparer<Object>
        , MetaDefItem
    {
        private readonly IComparer<MultiKeyUntyped> _comparator;

        public MultiKeyCastingComparator(IComparer<MultiKeyUntyped> comparator)
        {
            _comparator = comparator;
        }

        public int Compare(Object firstValues, Object secondValues)
        {
            return _comparator.Compare((MultiKeyUntyped)firstValues, (MultiKeyUntyped)secondValues);
        }
    }
}
