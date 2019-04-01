///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.map
{
    public class MapEventBeanPropertyWriterMapProp : MapEventBeanPropertyWriter
    {
        private readonly String _key;

        public MapEventBeanPropertyWriterMapProp(String propertyName, String key)
            : base(propertyName)
        {
            _key = key;
        }

        public override void Write(Object value, IDictionary<String, Object> map)
        {
            var mapEntry = (IDictionary<String, Object>)map.Get(PropertyName);
            if (mapEntry != null)
            {
                mapEntry.Put(_key, value);
            }
        }
    }
}