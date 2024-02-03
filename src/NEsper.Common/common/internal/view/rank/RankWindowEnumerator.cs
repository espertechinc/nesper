///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.view.rank
{
    /// <summary>
    ///     GetEnumerator for use by <seealso cref="RankWindowView" />.
    /// </summary>
    public sealed class RankWindowEnumerator : MixedEventBeanAndCollectionEnumeratorBase
    {
        private readonly IDictionary<object, object> _window;

        /// <summary>Ctor. </summary>
        /// <param name="window">sorted map with events</param>
        public RankWindowEnumerator(IDictionary<object, object> window)
            : base(window.Keys)
        {
            _window = window;
        }

        protected override object GetValue(object keyValue)
        {
            return _window.Get(keyValue);
        }
    }
}