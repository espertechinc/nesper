///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;

namespace com.espertech.esper.view.window
{
    public class ExpressionWindowTimestampEventPair
    {
        public ExpressionWindowTimestampEventPair(long timestamp, EventBean theEvent)
        {
            Timestamp = timestamp;
            TheEvent = theEvent;
        }

        public long Timestamp { get; private set; }

        public EventBean TheEvent { get; private set; }
    }
}