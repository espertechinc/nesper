///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.view.expression
{
    public enum ExpressionViewOAFieldEnum
    {
        CURRENT_COUNT,
        OLDEST_TIMESTAMP,
        NEWEST_TIMESTAMP,
        EXPIRED_COUNT,
        VIEW_REFERENCE,
        NEWEST_EVENT,
        OLDEST_EVENT
    }

    public static class ExpressionViewOAFieldEnumExtensions
    {
        public static string GetFieldName(this ExpressionViewOAFieldEnum value)
        {
            switch (value)
            {
                case ExpressionViewOAFieldEnum.CURRENT_COUNT:
                    return "current_count";

                case ExpressionViewOAFieldEnum.OLDEST_TIMESTAMP:
                    return "oldest_timestamp";

                case ExpressionViewOAFieldEnum.NEWEST_TIMESTAMP:
                    return "newest_timestamp";

                case ExpressionViewOAFieldEnum.EXPIRED_COUNT:
                    return "expired_count";

                case ExpressionViewOAFieldEnum.VIEW_REFERENCE:
                    return "view_reference";

                case ExpressionViewOAFieldEnum.NEWEST_EVENT:
                    return "newest_event";

                case ExpressionViewOAFieldEnum.OLDEST_EVENT:
                    return "oldest_event";
            }

            throw new ArgumentException("invalid value", nameof(value));
        }

        public static LinkedHashMap<string, object> AsMapOfTypes(EventType eventType)
        {
            var builtinTypeDef = new LinkedHashMap<string, object>();
            builtinTypeDef.Put(GetFieldName(ExpressionViewOAFieldEnum.CURRENT_COUNT), typeof(int?));
            builtinTypeDef.Put(GetFieldName(ExpressionViewOAFieldEnum.OLDEST_TIMESTAMP), typeof(long?));
            builtinTypeDef.Put(GetFieldName(ExpressionViewOAFieldEnum.NEWEST_TIMESTAMP), typeof(long?));
            builtinTypeDef.Put(GetFieldName(ExpressionViewOAFieldEnum.EXPIRED_COUNT), typeof(int?));
            builtinTypeDef.Put(GetFieldName(ExpressionViewOAFieldEnum.VIEW_REFERENCE), typeof(object));
            builtinTypeDef.Put(GetFieldName(ExpressionViewOAFieldEnum.NEWEST_EVENT), eventType);
            builtinTypeDef.Put(GetFieldName(ExpressionViewOAFieldEnum.OLDEST_EVENT), eventType);
            return builtinTypeDef;
        }

        public static void Populate(
            object[] properties,
            int windowSize,
            long oldestEventTimestamp,
            long newestEventTimestamp,
            object viewReference,
            int expiredCount,
            EventBean oldestEvent,
            EventBean newestEvent)
        {
            properties[(int) ExpressionViewOAFieldEnum.CURRENT_COUNT] = windowSize;
            properties[(int) ExpressionViewOAFieldEnum.OLDEST_TIMESTAMP] = oldestEventTimestamp;
            properties[(int) ExpressionViewOAFieldEnum.NEWEST_TIMESTAMP] = newestEventTimestamp;
            properties[(int) ExpressionViewOAFieldEnum.VIEW_REFERENCE] = viewReference;
            properties[(int) ExpressionViewOAFieldEnum.EXPIRED_COUNT] = expiredCount;
            properties[(int) ExpressionViewOAFieldEnum.OLDEST_EVENT] = oldestEvent;
            properties[(int) ExpressionViewOAFieldEnum.NEWEST_EVENT] = newestEvent;
        }

        public static object[] GetPrototypeOA()
        {
            int valuesCount = EnumHelper.GetValues<ExpressionViewOAFieldEnum>().Count();
            return new object[valuesCount];
        }
    }
} // end of namespace