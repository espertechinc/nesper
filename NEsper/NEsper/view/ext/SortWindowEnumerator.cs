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
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.view.ext
{
    public class SortWindowEnumerator : MixedEventBeanAndCollectionEnumeratorBase
    {
        private readonly IDictionary<object, object> _window;

        public SortWindowEnumerator(IDictionary<Object, Object> window)
            : base(window.Keys.GetEnumerator())
        {
            _window = window;
        }

        protected override Object GetValue(Object iteratorKeyValue)
        {
            return _window.Get(iteratorKeyValue);
        }
    }
}
