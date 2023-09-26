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
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.collection
{
    public class SingleEventEnumerator : IEnumerator<EventBean>
    {
        private readonly EventBean _event;
        private State _consumerState;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SingleEventEnumerator " /> class.
        /// </summary>
        /// <param name="event">The events.</param>
        public SingleEventEnumerator(EventBean @event)
        {
            _event = @event;
            _consumerState = _event != null
                ? State.NOT_CONSUMED
                : State.CONSUMED;
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            switch (_consumerState) {
                case State.CONSUMED:
                case State.CONSUMING:
                    _consumerState = State.CONSUMED;
                    return false;

                default:
                    _consumerState = State.CONSUMING;
                    return true;
            }
        }

        public void Reset()
        {
            _consumerState = _event != null
                ? State.NOT_CONSUMED
                : State.CONSUMED;
        }

        object IEnumerator.Current => Current;

        public EventBean Current {
            get {
                switch (_consumerState) {
                    case State.CONSUMED:
                        throw new InvalidOperationException();

                    case State.CONSUMING:
                        return _event;

                    default:
                        throw new IllegalStateException("enumerator has not been advanced");
                }
            }
        }

        private enum State
        {
            NOT_CONSUMED,
            CONSUMING,
            CONSUMED
        }
    }
}