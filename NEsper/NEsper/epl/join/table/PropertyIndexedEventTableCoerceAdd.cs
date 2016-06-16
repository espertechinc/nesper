///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.join.table
{
    /// <summary>
    /// MapIndex that organizes events by the event property values into hash buckets. Based 
    /// on a HashMap with <seealso cref="com.espertech.esper.collection.MultiKeyUntyped"/> keys 
    /// that store the property values. <para/>Performs coercion of the index keys before 
    /// storing the keys. 
    /// <para/>
    /// Takes a list of property names as parameter. Doesn't care which event type the events 
    /// have as long as the properties exist. If the same event is added twice, the class 
    /// throws an exception on add.
    /// </summary>
    public class PropertyIndexedEventTableCoerceAdd : PropertyIndexedEventTableUnadorned
    {
        protected readonly Coercer[] Coercers;
        protected readonly Type[] CoercionTypes;

        public PropertyIndexedEventTableCoerceAdd(EventPropertyGetter[] propertyGetters, EventTableOrganization organization, Coercer[] coercers, Type[] coercionTypes)
            : base(propertyGetters, organization)
        {
            Coercers = coercers;
            CoercionTypes = coercionTypes;
        }

        protected override MultiKeyUntyped GetMultiKey(EventBean theEvent)
        {
            var keyValues = new Object[propertyGetters.Length];
            for (int i = 0; i < propertyGetters.Length; i++)
            {
                var value = propertyGetters[i].Get(theEvent);
                if ((value != null) && (value.GetType() != CoercionTypes[i]))
                {
                    if (value.IsNumber())
                    {
                        value = Coercers[i].Invoke(value);
                    }
                }
                keyValues[i] = value;
            }
            return new MultiKeyUntyped(keyValues);
        }
    }
}
