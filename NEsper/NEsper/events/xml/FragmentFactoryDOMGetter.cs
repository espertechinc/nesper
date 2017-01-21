///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Xml;
using System.Xml.Linq;
using com.espertech.esper.client;

namespace com.espertech.esper.events.xml
{
    /// <summary>
    /// Factory for fragments for DOM getters.
    /// </summary>
    public class FragmentFactoryDOMGetter : FragmentFactory
    {
        private readonly EventAdapterService _eventAdapterService;
        private readonly BaseXMLEventType _xmlEventType;
        private readonly String _propertyExpression;
    
        private volatile EventType _fragmentType;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="eventAdapterService">for event type lookup</param>
        /// <param name="xmlEventType">the originating type</param>
        /// <param name="propertyExpression">property expression</param>
        public FragmentFactoryDOMGetter(EventAdapterService eventAdapterService, BaseXMLEventType xmlEventType, String propertyExpression)
        {
            _eventAdapterService = eventAdapterService;
            _xmlEventType = xmlEventType;
            _propertyExpression = propertyExpression;
        }

        public EventBean GetEvent(XObject result)
        {
            if (_fragmentType == null)
            {
                FragmentEventType type = _xmlEventType.GetFragmentType(_propertyExpression);
                if (type == null)
                {
                    return null;
                }
                _fragmentType = type.FragmentType;
            }

            return _eventAdapterService.AdapterForTypedDOM(result, _fragmentType);
        }
    
        public EventBean GetEvent(XmlNode result)
        {
            if (_fragmentType == null)
            {
                FragmentEventType type = _xmlEventType.GetFragmentType(_propertyExpression);
                if (type == null)
                {
                    return null;
                }
                _fragmentType = type.FragmentType;
            }
    
            return _eventAdapterService.AdapterForTypedDOM(result, _fragmentType);
        }    
    }
}
