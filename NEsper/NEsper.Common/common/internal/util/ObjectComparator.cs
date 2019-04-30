///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.util
{
    /// <summary>
    /// A comparator on objects that takes a bool array for ascending/descending.
    /// </summary>
    [Serializable]
    public sealed class ObjectComparator
        : IComparer<object>
    {
        private readonly bool _isDescendingValue;

        /// <summary>Ctor. </summary>
        /// <param name="isDescendingValue">ascending or descending</param>
        public ObjectComparator(bool isDescendingValue)
        {
            _isDescendingValue = isDescendingValue;
        }

        public int Compare(
            object firstValue,
            object secondValue)
        {
            return CollectionUtil.CompareValues(firstValue, secondValue, _isDescendingValue);
        }
    }
}