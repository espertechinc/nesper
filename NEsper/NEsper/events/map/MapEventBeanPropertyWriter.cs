///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.events.map
{
    public class MapEventBeanPropertyWriter : EventPropertyWriter
    {
        private readonly String _propertyName;
    
        public MapEventBeanPropertyWriter(String propertyName) {
            _propertyName = propertyName;
        }

        public string PropertyName
        {
            get { return _propertyName; }
        }

        public virtual void Write(Object value, EventBean target)
        {
            MappedEventBean map = (MappedEventBean) target;
            Write(value, map.Properties);
        }
    
        public virtual void Write(Object value, IDictionary<String, Object> map) {
            map.Put(_propertyName, value);
        }
    }
}
