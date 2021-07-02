///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.view.expression
{
    public class ExpressionWindowTimestampEventPair
    {
        private readonly long _timestamp;
        private readonly EventBean _theEvent;

        public ExpressionWindowTimestampEventPair(
            long timestamp,
            EventBean theEvent)
        {
            this._timestamp = timestamp;
            this._theEvent = theEvent;
        }

        public long Timestamp {
            get => _timestamp;
        }

        public EventBean TheEvent {
            get => _theEvent;
        }
    }
} // end of namespace