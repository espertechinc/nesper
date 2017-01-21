///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.view.window
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
                    return ("current_count");
                case ExpressionViewOAFieldEnum.OLDEST_TIMESTAMP:
                    return ("oldest_timestamp");
                case ExpressionViewOAFieldEnum.NEWEST_TIMESTAMP:
                    return ("newest_timestamp");
                case ExpressionViewOAFieldEnum.EXPIRED_COUNT:
                    return ("expired_count");
                case ExpressionViewOAFieldEnum.VIEW_REFERENCE:
                    return ("view_reference");
                case ExpressionViewOAFieldEnum.NEWEST_EVENT:
                    return ("newest_event");
                case ExpressionViewOAFieldEnum.OLDEST_EVENT:
                    return ("oldest_event");  
            }

            throw new ArgumentException();
        }

        public static LinkedHashMap<String, Object> AsMapOfTypes(EventType eventType)
        {
            LinkedHashMap<String, Object> builtinTypeDef = new LinkedHashMap<String, Object>();
            builtinTypeDef.Put(ExpressionViewOAFieldEnum.CURRENT_COUNT.GetFieldName(), typeof(int));
            builtinTypeDef.Put(ExpressionViewOAFieldEnum.OLDEST_TIMESTAMP.GetFieldName(), typeof(long));
            builtinTypeDef.Put(ExpressionViewOAFieldEnum.NEWEST_TIMESTAMP.GetFieldName(), typeof(long));
            builtinTypeDef.Put(ExpressionViewOAFieldEnum.EXPIRED_COUNT.GetFieldName(), typeof(int));
            builtinTypeDef.Put(ExpressionViewOAFieldEnum.VIEW_REFERENCE.GetFieldName(), typeof(Object));
            builtinTypeDef.Put(ExpressionViewOAFieldEnum.NEWEST_EVENT.GetFieldName(), eventType);
            builtinTypeDef.Put(ExpressionViewOAFieldEnum.OLDEST_EVENT.GetFieldName(), eventType);
            return builtinTypeDef;
        }
    
        public static void Populate(Object[] properties,
                                    int windowSize,
                                    long oldestEventTimestamp,
                                    long newestEventTimestamp,
                                    Object viewReference,
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
    
        public static Object[] GetPrototypeOA()
        {
            return new Object[EnumHelper.CountValues<ExpressionViewOAFieldEnum>()];
        }
    }
}
