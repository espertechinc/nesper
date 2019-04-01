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
    public sealed class ObjectCollatingComparator
        : IComparer<Object>
    {
        private readonly bool _isDescendingValue;

        [NonSerialized]
        private readonly StringComparer _collator = null;

        /// <summary>Ctor. </summary>
        /// <param name="isDescendingValue">ascending or descending</param>
        public ObjectCollatingComparator(bool isDescendingValue)
        {
            _isDescendingValue = isDescendingValue;
            _collator = StringComparer.CurrentCulture;
        }

        public int Compare(Object firstValue, Object secondValue)
        {
            return MultiKeyComparator.CompareValues(firstValue, secondValue, _isDescendingValue, _collator);
        }
    }
}