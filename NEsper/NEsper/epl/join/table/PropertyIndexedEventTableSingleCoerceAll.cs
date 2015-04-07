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
using com.espertech.esper.events;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.join.table
{
    public class PropertyIndexedEventTableSingleCoerceAll : PropertyIndexedEventTableSingleCoerceAdd
    {
        private readonly Type _coercionType;
    
        public PropertyIndexedEventTableSingleCoerceAll(EventPropertyGetter propertyGetter, EventTableOrganization organization, Coercer coercer, Type coercionType)
            : base(propertyGetter, organization, coercer, coercionType)
        {
            _coercionType = coercionType;
        }
    
        /// <summary>Returns the set of events that have the same property value as the given event. </summary>
        /// <returns>set of events with property value, or null if none found (never returns zero-sized set)</returns>
        public override ISet<EventBean> Lookup(Object key)
        {
            key = EventBeanUtility.Coerce(key, _coercionType);
            return PropertyIndex.Get(key);
        }
    }
}
