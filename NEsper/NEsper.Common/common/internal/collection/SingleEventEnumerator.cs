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
    public class SingleEventEnumerator : IEnumerator<EventBean>
    {
        private readonly EventBean _event;
        private bool _hasEvent;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SingleEventEnumerator " /> class.
        /// </summary>
        /// <param name="event">The events.</param>
        public SingleEventEnumerator(EventBean @event)
        {
            _event = @event;
            _hasEvent = _event != null;
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            if (!_hasEvent || _event == null) {
                return false;
            }

            return true;
        }

        public void Reset()
        {
            _hasEvent = _event != null;
        }

        object IEnumerator.Current => Current;

        public EventBean Current {
            get {
                if (_event == null) {
                    throw new InvalidOperationException();
                }

                return _event;
            }
        }
    }
}