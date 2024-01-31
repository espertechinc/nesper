///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.util
{
    /// <summary>
    /// A comparator on objects that takes a bool array for ascending/descending.
    /// </summary>
    public sealed class ObjectCollatingComparator
        : IComparer<object>
    {
        private readonly bool _isDescendingValue;

        [JsonIgnore]
        [NonSerialized]
        private readonly IComparer<object> _collator = null;

        /// <summary>Ctor. </summary>
        /// <param name="isDescendingValue">ascending or descending</param>
        public ObjectCollatingComparator(bool isDescendingValue)
        {
            _isDescendingValue = isDescendingValue;
            _collator = Comparers.Collating();
        }

        public int Compare(
            object firstValue,
            object secondValue)
        {
            return CollectionUtil.CompareValuesCollated(firstValue, secondValue, _isDescendingValue, _collator);
        }
    }
}