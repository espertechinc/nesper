///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using com.espertech.esper.client;
using com.espertech.esper.compat;

namespace com.espertech.esper.supportregression.plugineventrep
{
    public class MyPlugInPropertiesEventBean : EventBean
    {
        private readonly MyPlugInPropertiesEventType eventType;
        private readonly Properties properties;
    
        public MyPlugInPropertiesEventBean(MyPlugInPropertiesEventType eventType, Properties properties)
        {
            this.eventType = eventType;
            this.properties = properties;
        }

        public EventType EventType
        {
            get { return eventType; }
        }

        public object this[string property]
        {
            get { return Get(property); }
        }

        public Object Get(String property)
        {
            EventPropertyGetter getter = eventType.GetGetter(property);
            if (getter != null)
            {
                return getter.Get(this);
            }
            return null;
        }

        public object Underlying
        {
            get { return properties; }
        }

        public Properties Properties
        {
            get { return properties; }
        }

        public Object GetFragment(String property)
        {
            EventPropertyGetter getter = eventType.GetGetter(property);
            if (getter != null)
            {
                return getter.GetFragment(this);
            }
            return null;
        }
    }
}
