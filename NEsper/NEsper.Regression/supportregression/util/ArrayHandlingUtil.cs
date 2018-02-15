///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.util;

namespace com.espertech.esper.supportregression.util
{
    public class ArrayHandlingUtil
    {
        public static EventBean[] Reorder(String key, EventBean[] events) {
            return Reorder(new string[] {key}, events);
        }

        public static EventBean[] Reorder(String[] keys, EventBean[] events) {
            var result = new EventBean[events.Length];
            Array.Copy(events, 0, result, 0, result.Length);

            var mkcomparator = new MultiKeyComparator(new bool[keys.Length]);
            var comparer = new StandardComparer<EventBean>(
                (o1, o2) =>
                {
                    MultiKeyUntyped mk1 = GetMultiKey(o1, keys);
                    MultiKeyUntyped mk2 = GetMultiKey(o2, keys);
                    return mkcomparator.Compare(mk1, mk2);
                });

            Array.Sort(result, comparer);
            return result;
        }
    
        public static MultiKeyUntyped GetMultiKey(EventBean theEvent, String[] keys) {
            object[] mk = new Object[keys.Length];
            for (int i = 0; i < keys.Length; i++) {
                mk[i] = theEvent.Get(keys[i]);
            }
            return new MultiKeyUntyped(mk);
        }
        public static object[][] GetUnderlyingEvents(EventBean[] events, String[] keys)
        {
            return events
                .Select(eventBean => keys.Select(eventBean.Get).ToArray())
                .ToArray();
        }
    }
}
