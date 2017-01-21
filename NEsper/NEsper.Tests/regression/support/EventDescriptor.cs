///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.regression.support
{
    public class EventDescriptor
    {
        private IDictionary<String, Object> eventProperties;
    
        public EventDescriptor()
        {
            eventProperties = new Dictionary<String, Object>();
        }

        public IDictionary<string, object> EventProperties
        {
            get { return eventProperties; }
        }

        public void Put(String propertyName, Object value)
        {
            eventProperties.Put(propertyName, value);
        }
    }
}
