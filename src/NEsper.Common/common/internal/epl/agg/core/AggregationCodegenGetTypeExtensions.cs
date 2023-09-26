using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    public static class AggregationCodegenGetTypeExtensions
    {
        public static Type GetReturnType(this AggregationCodegenGetType value)
        {
            switch (value) {
                case AggregationCodegenGetType.GETVALUE:
                    return typeof(object);

                case AggregationCodegenGetType.GETEVENTBEAN:
                    return typeof(EventBean);

                case AggregationCodegenGetType.GETCOLLECTIONSCALAR:
                    return typeof(ICollection<object>);

                case AggregationCodegenGetType.GETCOLLECTIONOFEVENTS:
                    return typeof(ICollection<EventBean>);

                default:
                    throw new ArgumentOutOfRangeException(nameof(value));
            }
        }

        public static string GetAccessorMethodName(this AggregationCodegenGetType value)
        {
            switch (value) {
                case AggregationCodegenGetType.GETVALUE:
                    return "getValue";

                case AggregationCodegenGetType.GETEVENTBEAN:
                    return "getEnumerableEvent";

                case AggregationCodegenGetType.GETCOLLECTIONSCALAR:
                    return "getEnumerableScalar";

                case AggregationCodegenGetType.GETCOLLECTIONOFEVENTS:
                    return "getEnumerableEvents";

                default:
                    throw new ArgumentOutOfRangeException(nameof(value));
            }
        }
    }
}