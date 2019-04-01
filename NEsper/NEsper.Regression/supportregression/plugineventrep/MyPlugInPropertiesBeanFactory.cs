///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using com.espertech.esper.client;
using com.espertech.esper.compat;

namespace com.espertech.esper.supportregression.plugineventrep
{
    public class MyPlugInPropertiesBeanFactory
    {
        private readonly IList<MyPlugInPropertiesEventType> _knownTypes;
    
        public MyPlugInPropertiesBeanFactory(IList<MyPlugInPropertiesEventType> types)
        {
            _knownTypes = types;
        }
    
        public EventBean Create(Object theEvent, Uri resolutionURI)
        {
            Properties properties = (Properties) theEvent;
    
            // use the known types to determine the type of the object
            foreach (MyPlugInPropertiesEventType type in _knownTypes)
            {
                // if there is one property the event does not contain, then its not the right type
                bool hasAllProperties = true;
                foreach (String prop in type.PropertyNames)
                {
                    if (!properties.ContainsKey(prop))
                    {
                        hasAllProperties = false;
                        break;
                    }
                }
    
                if (hasAllProperties)
                {
                    return new MyPlugInPropertiesEventBean(type, properties);
                }
            }
    
            return null; // none match, unknown event
        }
    }
}
