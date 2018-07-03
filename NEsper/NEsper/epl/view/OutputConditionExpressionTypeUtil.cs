///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.epl.view
{
    public class OutputConditionExpressionTypeUtil
    {
        public readonly static LinkedHashMap<String, Object> TYPEINFO;
    
        static OutputConditionExpressionTypeUtil()
        {
            TYPEINFO = new LinkedHashMap<String, Object>();
            TYPEINFO.Put("count_insert", typeof(int));
            TYPEINFO.Put("count_remove", typeof(int));
            TYPEINFO.Put("count_insert_total", typeof(int));
            TYPEINFO.Put("count_remove_total", typeof(int));
            TYPEINFO.Put("last_output_timestamp", typeof(long));
        }

        public static object[] OAPrototype => new Object[TYPEINFO.Count];

        public static void Populate(Object[] builtinProperties, int totalNewEventsCount, int totalOldEventsCount,
                                    int totalNewEventsSum, int totalOldEventsSum, long? lastOutputTimestamp)
        {
            builtinProperties[0] = totalNewEventsCount;
            builtinProperties[1] = totalOldEventsCount;
            builtinProperties[2] = totalNewEventsSum;
            builtinProperties[3] = totalOldEventsSum;
            builtinProperties[4] = lastOutputTimestamp;
        }
    }
}
