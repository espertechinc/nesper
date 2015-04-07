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
using com.espertech.esper.events.bean;

namespace com.espertech.esper.events.map
{
    /// <summary>
    /// A getter that works on PONO events residing within a Map as an event property.
    /// </summary>
    public class MapArrayBeanEntryIndexedPropertyGetter
        : BaseNativePropertyGetter
        , MapEventPropertyGetter
    {
        private readonly String _propertyMap;
        private readonly int _index;
        private readonly BeanEventPropertyGetter _nestedGetter;

        /// <summary>Ctor. </summary>
        /// <param name="propertyMap">the property to look at</param>
        /// <param name="nestedGetter">the getter for the map entry</param>
        /// <param name="eventAdapterService">for producing wrappers to objects</param>
        /// <param name="index">the index to fetch the array element for</param>
        /// <param name="returnType">type of the entry returned</param>
        public MapArrayBeanEntryIndexedPropertyGetter(String propertyMap,
                                                      int index,
                                                      BeanEventPropertyGetter nestedGetter,
                                                      EventAdapterService eventAdapterService,
                                                      Type returnType)
            : base(eventAdapterService, returnType, null)
        {
            _propertyMap = propertyMap;
            _index = index;
            _nestedGetter = nestedGetter;
        }
    
        public Object GetMap(IDictionary<String, Object> map)
        {
            // If the map does not contain the key, this is allowed and represented as null
            Object value = map.Get(_propertyMap);
            return BaseNestableEventUtil.GetBeanArrayValue(_nestedGetter, value, _index);
        }
    
        public bool IsMapExistsProperty(IDictionary<String, Object> map)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }
    
        public override Object Get(EventBean obj)
        {
            IDictionary<String, Object> map = BaseNestableEventUtil.CheckedCastUnderlyingMap(obj);
            return GetMap(map);
        }
    
        public override bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }
    }
}
