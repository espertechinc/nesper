///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.output.condition
{
    public class OutputConditionExpressionTypeUtil
    {
        public static readonly LinkedHashMap<string, object> TYPEINFO;

        static OutputConditionExpressionTypeUtil()
        {
            TYPEINFO = new LinkedHashMap<string, object>();
            TYPEINFO.Put("count_insert", typeof(int));
            TYPEINFO.Put("count_remove", typeof(int));
            TYPEINFO.Put("count_insert_total", typeof(int));
            TYPEINFO.Put("count_remove_total", typeof(int));
            TYPEINFO.Put("last_output_timestamp", typeof(long));
        }

        public static object[] OAPrototype => new object[TYPEINFO.Count];

        public static void Populate(
            object[] builtinProperties,
            int totalNewEventsCount,
            int totalOldEventsCount,
            int totalNewEventsSum,
            int totalOldEventsSum,
            long? lastOutputTimestamp)
        {
            builtinProperties[0] = totalNewEventsCount;
            builtinProperties[1] = totalOldEventsCount;
            builtinProperties[2] = totalNewEventsSum;
            builtinProperties[3] = totalOldEventsSum;
            builtinProperties[4] = lastOutputTimestamp;
        }

        public static EventType GetBuiltInEventType(
            string moduleName,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            var metadata = new EventTypeMetadata(
                "anonymous",
                moduleName,
                EventTypeTypeClass.STREAM,
                EventTypeApplicationType.OBJECTARR,
                NameAccessModifier.TRANSIENT,
                EventTypeBusModifier.NONBUS,
                false,
                EventTypeIdPair.Unassigned());
            return BaseNestableEventUtil.MakeOATypeCompileTime(
                metadata,
                TYPEINFO,
                null,
                null,
                null,
                null,
                beanEventTypeFactory,
                null);
        }
    }
}