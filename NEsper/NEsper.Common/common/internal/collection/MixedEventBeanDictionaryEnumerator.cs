using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.collection
{
    public static class MixedEventBeanDictionaryEnumerator
    {
        public static IEnumerator<EventBean> GetMultiLevelEnumerator<T>(this IDictionary<T, object> window)
        {
            foreach (var entry in window) {
                var value = entry.Value;
                if (value is EventBean) {
                    yield return (EventBean) value;
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