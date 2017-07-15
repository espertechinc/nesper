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

namespace com.espertech.esper.events.map
{
    public class MapNestedEntryPropertyGetterPropertyProvidedDynamic : MapNestedEntryPropertyGetterBase
    {
        private readonly EventPropertyGetter _nestedGetter;

        public MapNestedEntryPropertyGetterPropertyProvidedDynamic(
            string propertyMap,
            EventType fragmentType,
            EventAdapterService eventAdapterService,
            EventPropertyGetter nestedGetter)
            : base(propertyMap, fragmentType, eventAdapterService)
        {
            _nestedGetter = nestedGetter;
        }
    
        public override Object HandleNestedValue(Object value)
        {
            if (!(value is IDictionary<string, object>))
            {
                return null;
            }
            if (_nestedGetter is MapEventPropertyGetter) {
                return ((MapEventPropertyGetter) _nestedGetter).GetMap((IDictionary<string, Object>) value);
            }
            return null;
        }
    
        public override bool IsExistsProperty(EventBean eventBean)
        {
            var map = BaseNestableEventUtil.CheckedCastUnderlyingMap(eventBean);
            var value = map.Get(base.PropertyMap);
            if (value == null || !(value is IDictionary<string, object>))
            {
                return false;
            }
            if (_nestedGetter is MapEventPropertyGetter) {
                return ((MapEventPropertyGetter) _nestedGetter).IsMapExistsProperty((IDictionary<string, object>) value);
            }
            return false;
        }
    
        public override Object HandleNestedValueFragment(Object value) {
            return null;
        }
    }
} // end of namespace
