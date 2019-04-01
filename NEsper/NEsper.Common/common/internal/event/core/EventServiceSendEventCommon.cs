///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Xml;

namespace com.espertech.esper.common.@internal.@event.core
{
    public interface EventServiceSendEventCommon
    {
        void SendEventObjectArray(object[] @event, string eventTypeName);

        void SendEventBean(object @event, string eventTypeName);

        void SendEventMap(IDictionary<string, object> @event, string eventTypeName);

        void SendEventXMLDOM(XmlNode node, string eventTypeName);

        /// <summary>
        ///     Send an event represented by a Avro GenericData.Record to the event stream processing runtime.
        ///     <para />
        ///     Use the route method for sending events into the runtime from within UpdateListener code,
        ///     to avoid the possibility of a stack overflow due to nested calls to sendEvent
        ///     (except with the outbound-threading configuration), see {@link EventServiceRouteEventCommon#routeEventAvro(Object,
        ///     String)}}).
        /// </summary>
        /// <param name="avroGenericDataDotRecord">is the event to sent to the runtime</param>
        /// <param name="avroEventTypeName">event type name</param>
        /// <throws>EPException is thrown when the processing of the event lead to an error</throws>
        void SendEventAvro(object avroGenericDataDotRecord, string avroEventTypeName);
    }
} // end of namespace