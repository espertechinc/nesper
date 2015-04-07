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

namespace com.espertech.esper.events.map
{
    /// <summary>
    /// Reader method for reading all properties of a Map event.
    /// </summary>
    public class MapEventBeanReader : EventBeanReader
    {
        private MapEventPropertyGetter[] getterArray;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="type">map to read</param>
        public MapEventBeanReader(MapEventType type)
        {
            var properties = type.PropertyNames;
            var getters = new List<MapEventPropertyGetter>();
            foreach (String property in properties)
            {
                var getter = type.GetGetter(property) as MapEventPropertyGetter;
                if (getter != null)
                {
                    getters.Add(getter);
                }
            }
            getterArray = getters.ToArray();
        }
    
        public Object[] Read(EventBean theEvent)
        {
            var underlying = (IDictionary<String, Object>) theEvent.Underlying;
            var values = new Object[getterArray.Length];
            for (int i = 0; i < getterArray.Length; i++)
            {
                values[i] = getterArray[i].GetMap(underlying);
            }
            return values;
        }
    }
}
