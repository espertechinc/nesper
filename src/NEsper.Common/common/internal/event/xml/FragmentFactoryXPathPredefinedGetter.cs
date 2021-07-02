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

        private readonly EventBeanTypedEventFactory _eventBeanTypedEventFactory;
        private readonly string _eventTypeName;
        private readonly EventTypeNameResolver _eventTypeResolver;
        private readonly string _propertyName;

        private volatile EventType _eventType;

        public FragmentFactoryXPathPredefinedGetter(
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            EventTypeNameResolver eventTypeResolver,
            string eventTypeName,
            string propertyName)
        {
            this._eventBeanTypedEventFactory = eventBeanTypedEventFactory;
            this._eventTypeResolver = eventTypeResolver;
            this._eventTypeName = eventTypeName;
            this._propertyName = propertyName;
        }

        public EventBean GetEvent(XmlNode result)
        {
            if (_eventType == null) {
                var candidateEventType = _eventTypeResolver.GetTypeByName(_eventTypeName);
                if (candidateEventType == null) {
                    Log.Warn(
                        "Event type by name '" + _eventTypeName + "' was not found for property '" + _propertyName + "'");
                    return null;
                }

                if (!(candidateEventType is BaseXMLEventType)) {
                    Log.Warn(
                        "Event type by name '" +
                        _eventTypeName +
                        "' is not an XML event type for property '" +
                        _propertyName +
                        "'");
                    return null;
                }

                _eventType = candidateEventType;
            }

            return _eventBeanTypedEventFactory.AdapterForTypedDOM(result, _eventType);
        }
    }
} // end of namespace