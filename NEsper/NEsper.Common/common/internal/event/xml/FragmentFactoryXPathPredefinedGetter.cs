///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;
using System.Xml;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.@event.xml
{
    /// <summary>
    ///     Fragment factory for use with XPath explicit properties.
    /// </summary>
    public class FragmentFactoryXPathPredefinedGetter : FragmentFactory
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly EventBeanTypedEventFactory eventBeanTypedEventFactory;
        private readonly string eventTypeName;
        private readonly EventTypeNameResolver eventTypeResolver;
        private readonly string propertyName;

        private volatile EventType eventType;

        public FragmentFactoryXPathPredefinedGetter(
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            EventTypeNameResolver eventTypeResolver,
            string eventTypeName,
            string propertyName)
        {
            this.eventBeanTypedEventFactory = eventBeanTypedEventFactory;
            this.eventTypeResolver = eventTypeResolver;
            this.eventTypeName = eventTypeName;
            this.propertyName = propertyName;
        }

        public EventBean GetEvent(XmlNode result)
        {
            if (eventType == null) {
                var candidateEventType = eventTypeResolver.GetTypeByName(eventTypeName);
                if (candidateEventType == null) {
                    Log.Warn(
                        "Event type by name '" + eventTypeName + "' was not found for property '" + propertyName + "'");
                    return null;
                }

                if (!(candidateEventType is BaseXMLEventType)) {
                    Log.Warn(
                        "Event type by name '" + eventTypeName + "' is not an XML event type for property '" +
                        propertyName + "'");
                    return null;
                }

                eventType = candidateEventType;
            }

            return eventBeanTypedEventFactory.AdapterForTypedDOM(result, eventType);
        }
    }
} // end of namespace