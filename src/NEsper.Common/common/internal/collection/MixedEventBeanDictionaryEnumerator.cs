///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.collection
{
    public static class MixedEventBeanDictionaryEnumerator
    {
        public static IEnumerator<EventBean> GetMultiLevelEnumerator<T>(this IDictionary<T, object> window)
        {
            foreach (var entry in window) {
                var value = entry.Value;
                if (value is EventBean bean) {
                    yield return bean;
                }
                else if (value is IEnumerable<EventBean> enumerable) {
                    foreach (var subValue in enumerable) {
                        yield return subValue;
                    }
                }
            }
        }

        public static IEnumerator<EventBean> For<T>(this IEnumerator<KeyValuePair<T, object>> enumerator)
        {
            while (enumerator.MoveNext()) {
                var entry = enumerator.Current;
                var value = entry.Value;
                if (value is EventBean bean) {
                    yield return bean;
                }
                else if (value is IEnumerable<EventBean> enumerable) {
                    foreach (var subValue in enumerable) {
                        yield return subValue;
                    }
                }
            }
        }
    }
}