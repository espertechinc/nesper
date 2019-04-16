///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.map
{
    public class MapEventBeanPropertyWriterMapProp : MapEventBeanPropertyWriter
    {
        private readonly string _key;

        public MapEventBeanPropertyWriterMapProp(
            string propertyName,
            string key)
            : base(propertyName)
        {
            _key = key;
        }

        public override void Write(
            object value,
            IDictionary<string, object> map)
        {
            var mapEntry = (IDictionary<string, object>) map.Get(PropertyName);
            if (mapEntry != null) {
                mapEntry.Put(_key, value);
            }
        }
    }
}