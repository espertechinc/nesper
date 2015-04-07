///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Xml;
using System.Xml.Linq;
using com.espertech.esper.client;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.events.xml
{
    /// <summary>
    /// Fragment factory for use with XPath explicit properties.
    /// </summary>
    public class FragmentFactoryXPathPredefinedGetter : FragmentFactory
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        private readonly EventAdapterService _eventAdapterService;
        private readonly String _eventTypeName;
        private readonly String _propertyName;
    
        private volatile EventType _eventType;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="eventAdapterService">for event type lookup</param>
        /// <param name="eventTypeName">name to look up</param>
        /// <param name="propertyName">property</param>
        public FragmentFactoryXPathPredefinedGetter(EventAdapterService eventAdapterService, String eventTypeName, String propertyName)
        {
            _eventAdapterService = eventAdapterService;
            _eventTypeName = eventTypeName;
            _propertyName = propertyName;
        }
    
        public EventBean GetEvent(XmlNode result)
        {
            if (_eventType == null)
            {
                EventType candidateEventType = _eventAdapterService.GetEventTypeByName(_eventTypeName);
                if (candidateEventType == null)
                {
                    Log.Warn("Event type by name '" + _eventTypeName + "' was not found for property '" + _propertyName + "'");
                    return null;
                }
                if (!(candidateEventType is BaseXMLEventType))
                {
                    Log.Warn("Event type by name '" + _eventTypeName + "' is not an XML event type for property '" + _propertyName + "'");
                    return null;
                }
                _eventType = candidateEventType;
            }
    
            return _eventAdapterService.AdapterForTypedDOM(result, _eventType);
        }

        public EventBean GetEvent(XObject result)
        {
            if (_eventType == null)
            {
                EventType candidateEventType = _eventAdapterService.GetEventTypeByName(_eventTypeName);
                if (candidateEventType == null)
                {
                    Log.Warn("Event type by name '" + _eventTypeName + "' was not found for property '" + _propertyName + "'");
                    return null;
                }
                if (!(candidateEventType is BaseXMLEventType))
                {
                    Log.Warn("Event type by name '" + _eventTypeName + "' is not an XML event type for property '" + _propertyName + "'");
                    return null;
                }
                _eventType = candidateEventType;
            }

            return _eventAdapterService.AdapterForTypedDOM(result, _eventType);
        }
    }
}
