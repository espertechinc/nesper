///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.supportregression.plugineventrep
{
    public class MyPlugInPropertiesEventType : EventType
    {
        private readonly String _name;
        private readonly int _eventTypeId;
        private readonly ICollection<String> _properties;
        private readonly IDictionary<String, EventPropertyDescriptor> _descriptors;
    
        public MyPlugInPropertiesEventType(String name, int eventTypeId, ICollection<String> properties, IDictionary<String, EventPropertyDescriptor> descriptors)
        {
            _name = name;
            _eventTypeId = eventTypeId;
            _properties = properties;
            _descriptors = descriptors;
        }
    
        public Type GetPropertyType(String property)
        {
            if (!IsProperty(property))
            {
                return null;
            }
            return typeof(string);
        }

        public Type UnderlyingType
        {
            get { return typeof(Properties); }
        }

        public int EventTypeId
        {
            get { return _eventTypeId; }
        }

        public EventPropertyGetter GetGetter(String property)
        {
            string propertyName = property;
            return new ProxyEventPropertyGetter
            {
                ProcGet = eventBean =>
                {
                    var propBean = (MyPlugInPropertiesEventBean) eventBean;
                    return propBean.Properties.Get(propertyName);
                },
                ProcIsExistsProperty = eventBean =>
                {
                    var propBean = (MyPlugInPropertiesEventBean) eventBean;
                    return propBean.Properties.Get(propertyName) != null;
                },
                ProcGetFragment = eventBean => null
            };
        }

        public string[] PropertyNames
        {
            get { return _properties.ToArray(); }
        }

        public bool IsProperty(String property)
        {
            return _properties.Contains(property);
        }

        public EventType[] SuperTypes
        {
            get { return null; }
        }

        public EventType[] DeepSuperTypes
        {
            get { return null; }
        }

        public string Name
        {
            get { return _name; }
        }

        public IList<EventPropertyDescriptor> PropertyDescriptors
        {
            get
            {
                ICollection<EventPropertyDescriptor> descriptorColl = _descriptors.Values;
                return descriptorColl.ToArray();
            }
        }

        public EventPropertyDescriptor GetPropertyDescriptor(String propertyName)
        {
            return _descriptors.Get(propertyName);
        }
    
        public FragmentEventType GetFragmentType(String property)
        {
            return null;  // sample does not provide any fragments
        }
    
        public EventPropertyGetterMapped GetGetterMapped(String mappedProperty) {
            return null;    // sample does not provide a getter for mapped properties
        }

        public EventPropertyGetterIndexed GetGetterIndexed(string indexedProperty)
        {
            return null;    // sample does not provide a getter for indexed properties
        }

        public string StartTimestampPropertyName
        {
            get { return null; }
        }

        public string EndTimestampPropertyName
        {
            get { return null; }
        }
    }
}
