namespace com.espertech.esper.common.@internal.@event.core
{
    ///////////////////////////////////////////////////////////////////////////////////////
    // Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
    // http://esper.codehaus.org                                                          /
    // ---------------------------------------------------------------------------------- /
    // The software in this package is published under the terms of the GPL license       /
    // a copy of which has been included with this distribution in the license.txt file.  /
    ///////////////////////////////////////////////////////////////////////////////////////


    public interface SendableEvent
    {
        object Underlying { get; }
        void Send(EventServiceSendEventCommon eventService);
    }
} // end of namespace