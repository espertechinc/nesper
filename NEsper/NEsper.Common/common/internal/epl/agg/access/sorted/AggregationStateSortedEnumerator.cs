///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.agg.access.sorted
{
    public class AggregationStateSortedEnumerator : MixedEventBeanAndCollectionEnumeratorBase
    {
        private readonly IDictionary<object, object> _window;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="window">sorted map with events</param>
        /// <param name="reverse">if set to <c>true</c> [reverse].</param>
        public AggregationStateSortedEnumerator(SortedDictionary<object, object> window, bool reverse)
            : base(reverse ? Enumerable.Reverse(window.Keys) : window.Keys)
        {
            _window = window;
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="window">sorted map with events</param>
        /// <param name="reverse">if set to <c>true</c> [reverse].</param>
        public AggregationStateSortedEnumerator(OrderedDictionary<object, object> window, bool reverse)
            : base(reverse ? Enumerable.Reverse(window.Keys) : window.Keys)
        {
            _window = window;
        }

        protected override object GetValue(object iteratorKeyValue)
        {
            return _window.Get(iteratorKeyValue);
        }
    }
}
