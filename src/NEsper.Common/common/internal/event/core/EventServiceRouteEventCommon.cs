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
    internal interface EventServiceRouteEventCommon
    {
        void RouteEventObjectArray(
            object[] @event,
            string eventTypeName);

        void RouteEventBean(
            object @event,
            string eventTypeName);

        void RouteEventMap(
            IDictionary<string, object> @event,
            string eventTypeName);

        void RouteEventXMLDOM(
            XmlNode node,
            string eventTypeName);

        void RouteEventAvro(
            object avroGenericDataDotRecord,
            string eventTypeName);
    }
} // end of namespace