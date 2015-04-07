///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.events.map;

namespace com.espertech.esper.events.arr
{
    /// <summary>A getter that works on EventBean events residing within a Map as an event property. </summary>
    public class ObjectArrayNestedEntryPropertyGetterArrayMap : ObjectArrayNestedEntryPropertyGetterBase {
    
        private readonly int _index;
        private readonly MapEventPropertyGetter _getter;
    
        public ObjectArrayNestedEntryPropertyGetterArrayMap(int propertyIndex, EventType fragmentType, EventAdapterService eventAdapterService, int index, MapEventPropertyGetter getter)
            : base(propertyIndex, fragmentType, eventAdapterService)
        {
            _index = index;
            _getter = getter;
        }
    
        public override Object HandleNestedValue(Object value) {
            return BaseNestableEventUtil.HandleNestedValueArrayWithMap(value, _index, _getter);
        }
    
        public override Object HandleNestedValueFragment(Object value) {
            return BaseNestableEventUtil.HandleNestedValueArrayWithMapFragment(value, _index, _getter, EventAdapterService, FragmentType);
        }
    }
}
