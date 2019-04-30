///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.collection
{
    public class ArrayEventEnumerator : IEnumerator<EventBean>
    {
        private readonly EventBean[] _events;
        private int _index;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ArrayEventEnumerator" /> class.
        /// </summary>
        /// <param name="events">The events.</param>
        public ArrayEventEnumerator(EventBean[] events)
        {
            _events = events;
            _index = -1;
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            if (_events == null) {
                return false;
            }

            if (_index >= _events.Length) {
                return false;
            }

            _index++;
            if (_index >= _events.Length) {
                return false;
            }

            return true;
        }

        public void Reset()
        {
            _index = -1;
        }

        object IEnumerator.Current => Current;

        public EventBean Current {
            get {
                if (_events == null) {
                    throw new InvalidOperationException();
                }

                if (_index >= _events.Length) {
                    throw new InvalidOperationException();
                }

                return _events[_index];
            }
        }
    }
}