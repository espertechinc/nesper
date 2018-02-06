///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.client;

namespace com.espertech.esper.events.map
{
    /// <summary>Reader method for reading all properties of a Map event.</summary>
    public class MapEventBeanReader : EventBeanReader
    {
        private readonly MapEventPropertyGetter[] _getterArray;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="type">map to read</param>
        public MapEventBeanReader(MapEventType type)
        {
            var properties = type.PropertyNames;
            var getters = new List<MapEventPropertyGetter>();
            foreach (var property in properties)
            {
                var getter = (MapEventPropertyGetter) type.GetGetterSPI(property);
                if (getter != null) getters.Add(getter);
            }

            _getterArray = getters.ToArray();
        }

        public object[] Read(EventBean theEvent)
        {
            var underlying = (IDictionary<string, object>) theEvent.Underlying;
            var values = new object[_getterArray.Length];
            for (var i = 0; i < _getterArray.Length; i++) values[i] = _getterArray[i].GetMap(underlying);
            return values;
        }
    }
} // end of namespace