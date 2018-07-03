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

namespace com.espertech.esper.view.window
{
    public class ExpressionWindowTimestampEventPairEnumerator
        : IEnumerator<EventBean>
    {
        private readonly IEnumerator<ExpressionWindowTimestampEventPair> _events;

        public ExpressionWindowTimestampEventPairEnumerator(IEnumerator<ExpressionWindowTimestampEventPair> events)
        {
            _events = events;
        }

        #region IEnumerator<EventBean> Members

        public bool MoveNext()
        {
            return _events.MoveNext();
        }

        public void Dispose()
        {
            _events.Dispose();
        }

        public void Reset()
        {
            _events.Reset();
        }

        object IEnumerator.Current => Current;

        public EventBean Current => _events.Current.TheEvent;

        #endregion
    }
}