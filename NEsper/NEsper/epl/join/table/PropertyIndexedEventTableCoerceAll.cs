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
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.join.table
{
    /// <summary>
    /// MapIndex that organizes events by the event property values into hash buckets. Based 
    /// on a HashMap with <seealso cref="com.espertech.esper.collection.MultiKeyUntyped"/> keys 
    /// that store the property values. <para/>Performs coercion of the index keys before storing 
    /// the keys, and coercion of the lookup keys before lookup.
    /// <para/>
    /// Takes a list of property names as parameter. Doesn't care which event type the events have 
    /// as long as the properties exist. If the same event is added twice, the class throws an 
    /// exception on add.
    /// </summary>
    public class PropertyIndexedEventTableCoerceAll : PropertyIndexedEventTableCoerceAdd
    {
        public PropertyIndexedEventTableCoerceAll(EventPropertyGetter[] propertyGetters, EventTableOrganization organization, Coercer[] coercers, Type[] coercionType)
            : base(propertyGetters, organization, coercers, coercionType)
        {
        }
    
        /// <summary>Returns the set of events that have the same property value as the given event. </summary>
        /// <param name="keys">to compare against</param>
        /// <returns>set of events with property value, or null if none found (never returns zero-sized set)</returns>
        public override ISet<EventBean> Lookup(Object[] keys)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                var coercionType = CoercionTypes[i];
                var key = keys[i];

                if ((key != null) && (key.GetType() != coercionType))
                {
                    if (key.IsNumber())
                    {
                        key = CoercerFactory.CoerceBoxed(key, CoercionTypes[i]);
                        keys[i] = key;
                    }
                }
            }

            return propertyIndex.Get(new MultiKeyUntyped(keys));
        }
    }
}
