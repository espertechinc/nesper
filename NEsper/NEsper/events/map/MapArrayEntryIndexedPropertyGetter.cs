///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.events.bean;

using DataMap = System.Collections.Generic.IDictionary<string, object>;

namespace com.espertech.esper.events.map
{
    /// <summary>
    /// A getter that works on arrays residing within a Map as an event property.
    /// </summary>
    public class MapArrayEntryIndexedPropertyGetter 
        : BaseNativePropertyGetter
        , MapEventPropertyGetter
        , MapEventPropertyGetterAndIndexed
    {
        private readonly String _propertyMap;
        private readonly int _index;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="propertyMap">the property to use for the map lookup</param>
        /// <param name="index">the index to fetch the array element for</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        /// <param name="returnType">type of the entry returned</param>
        public MapArrayEntryIndexedPropertyGetter(String propertyMap, int index, EventAdapterService eventAdapterService, Type returnType)
            : base(eventAdapterService, returnType, null)
        {
            _propertyMap = propertyMap;
            _index = index;
        }

        public Object GetMap(DataMap map)
        {
            return GetMapInternal(map, _index);
        }

        public Object GetMapInternal(DataMap map, int index)
        {
            var value = map.Get(_propertyMap);
            return BaseNestableEventUtil.GetIndexedValue(value, index);
        }


        public bool IsMapExistsProperty(DataMap map)
        {
            return map.ContainsKey(_propertyMap);
        }

        public Object Get(EventBean eventBean, int index)
        {
            var map = BaseNestableEventUtil.CheckedCastUnderlyingMap(eventBean);
            return GetMapInternal(map, index);
        }

        public override Object Get(EventBean eventBean)
        {
            return GetMap(BaseNestableEventUtil.CheckedCastUnderlyingMap(eventBean));
        }

        public override bool IsExistsProperty(EventBean eventBean)
        {
            var map = BaseNestableEventUtil.CheckedCastUnderlyingMap(eventBean);
            return map.ContainsKey(_propertyMap);
        }
    }
}
