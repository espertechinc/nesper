///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.regressionlib.support.util
{
    public class ArrayHandlingUtil
    {
        public static EventBean[] Reorder(
            string key,
            EventBean[] events)
        {
            return Reorder(new[] {key}, events);
        }

        public static EventBean[] Reorder(
            string[] keys,
            EventBean[] events)
        {
            var result = new EventBean[events.Length];
            Array.Copy(events, 0, result, 0, result.Length);
            var mkcomparator = new ComparatorHashableMultiKey(new bool[keys.Length]);
            Array.Sort(
                result,
                (
                    o1,
                    o2) => {
                    var mk1 = GetMultiKey(o1, keys);
                    var mk2 = GetMultiKey(o2, keys);
                    return mkcomparator.Compare(mk1, mk2);
                });
            return result;
        }

        public static HashableMultiKey GetMultiKey(
            EventBean theEvent,
            string[] keys)
        {
            var mk = new object[keys.Length];
            for (var i = 0; i < keys.Length; i++) {
                mk[i] = theEvent.Get(keys[i]);
            }

            return new HashableMultiKey(mk);
        }

        public static object[][] GetUnderlyingEvents(
            EventBean[] events,
            string[] keys)
        {
            IList<object[]> resultList = new List<object[]>();

            for (var i = 0; i < events.Length; i++) {
                var row = new object[keys.Length];
                for (var j = 0; j < keys.Length; j++) {
                    row[j] = events[i].Get(keys[j]);
                }

                resultList.Add(row);
            }

            var results = new object[resultList.Count][];
            var count = 0;
            foreach (var row in resultList) {
                results[count++] = row;
            }

            return results;
        }
    }
} // end of namespace