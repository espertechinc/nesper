///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.@event.map
{
    /// <summary>
    ///     Reader method for reading all properties of a Map event.
    /// </summary>
    public class MapEventBeanReader : EventBeanReader
    {
        private readonly MapEventPropertyGetter[] getterArray;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="type">map to read</param>
        public MapEventBeanReader(MapEventType type)
        {
            var properties = type.PropertyNames;
            IList<MapEventPropertyGetter> getters = new List<MapEventPropertyGetter>();
            foreach (var property in properties) {
                var getter = (MapEventPropertyGetter) type.GetGetterSPI(property);
                if (getter != null) {
                    getters.Add(getter);
                }
            }

            getterArray = getters.ToArray();
        }

        public object[] Read(EventBean theEvent)
        {
            var underlying = (IDictionary<string, object>) theEvent.Underlying;
            var values = new object[getterArray.Length];
            for (var i = 0; i < getterArray.Length; i++) {
                values[i] = getterArray[i].GetMap(underlying);
            }

            return values;
        }
    }
} // end of namespace