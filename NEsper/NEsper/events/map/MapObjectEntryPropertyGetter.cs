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
using com.espertech.esper.compat.collections;
using com.espertech.esper.events.bean;

namespace com.espertech.esper.events.map
{
    using DataMap = IDictionary<string, object>;

    /// <summary>
    /// A getter that works on POCO events residing within a Map as an event property.
    /// </summary>
    public class MapObjectEntryPropertyGetter : BaseNativePropertyGetter, MapEventPropertyGetter
    {
        private readonly String _propertyMap;
        private readonly BeanEventPropertyGetter _mapEntryGetter;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="propertyMap">the property to look at</param>
        /// <param name="mapEntryGetter">the getter for the map entry</param>
        /// <param name="eventAdapterService">for producing wrappers to objects</param>
        /// <param name="returnType">type of the entry returned</param>
        /// <param name="nestedComponentType">Type of the nested component.</param>
        public MapObjectEntryPropertyGetter(
            String propertyMap,
            BeanEventPropertyGetter mapEntryGetter,
            EventAdapterService eventAdapterService,
            Type returnType,
            Type nestedComponentType)
            : base(eventAdapterService, returnType, nestedComponentType)
        {
            _propertyMap = propertyMap;
            _mapEntryGetter = mapEntryGetter;
        }

        public object GetMap(DataMap map)
        {
            // If the map does not contain the key, this is allowed and represented as null
            var value = map.Get(_propertyMap);
            if (value == null)
            {
                return null;
            }

            if (value is EventBean)
            {
                return _mapEntryGetter.Get((EventBean) value);
            }

            // Object within the map
            return _mapEntryGetter.GetBeanProp(value);
        }

        public bool IsMapExistsProperty(DataMap map)
        {
            return true;
        }

        public override Object Get(EventBean eventBean)
        {
            return GetMap(BaseNestableEventUtil.CheckedCastUnderlyingMap(eventBean));
        }
    
        public override bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }
    }
}
