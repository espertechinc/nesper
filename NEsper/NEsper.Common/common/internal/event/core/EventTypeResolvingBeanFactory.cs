///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.@event.core
{
    public interface EventTypeResolvingBeanFactory
    {
        EventBean AdapterForObjectArray(
            object[] theEvent,
            string eventTypeName);

        EventBean AdapterForBean(
            object data,
            string eventTypeName);

        EventBean AdapterForMap(
            IDictionary<string, object> map,
            string eventTypeName);

        EventBean AdapterForXMLDOM(
            XmlNode node,
            string eventTypeName);

        EventBean AdapterForXML(
            XNode node,
            string eventTypeName);

        EventBean AdapterForAvro(
            object avroGenericDataDotRecord,
            string eventTypeName);
    }
} // end of namespace