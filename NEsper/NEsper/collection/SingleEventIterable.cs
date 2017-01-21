///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;

namespace com.espertech.esper.collection
{
    public class SingleEventIterable<T> : IEnumerable<EventBean> where T : class, EventBean
    {
        private readonly Atomic<T> _ref;
    
        public SingleEventIterable(Atomic<T> @ref)
        {
            _ref = @ref;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<EventBean> GetEnumerator()
        {
            var theEvent = _ref.Get();
            if (theEvent != null)
                yield return theEvent;
        }
    }
}
