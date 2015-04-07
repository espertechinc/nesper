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

namespace com.espertech.esper.events.map
{
    using Map = IDictionary<string, object>;

    public class MapDynamicPropertyGetter : MapEventPropertyGetter
    {

        private readonly String _propertyName;

        public MapDynamicPropertyGetter(String propertyName)
        {
            _propertyName = propertyName;
        }

        public Object GetMap(IDictionary<String, Object> map)
        {
            return map.Get(_propertyName);
        }

        public bool IsMapExistsProperty(IDictionary<String, Object> map)
        {
            return map.ContainsKey(_propertyName);
        }

        public Object Get(EventBean eventBean)
        {
            var map = (Map)eventBean.Underlying;
            return map.Get(_propertyName);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            var map = (Map)eventBean.Underlying;
            return map.ContainsKey(_propertyName);
        }

        public Object GetFragment(EventBean eventBean)
        {
            return null;
        }
    }
}
