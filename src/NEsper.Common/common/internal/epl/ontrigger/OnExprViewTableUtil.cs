///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.table.core;

namespace com.espertech.esper.common.@internal.epl.ontrigger
{
    public class OnExprViewTableUtil
    {
        public static EventBean[] ToPublic(
            EventBean[] matching,
            Table table,
            EventBean[] triggers,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var eventsPerStream = new EventBean[2];
            eventsPerStream[0] = triggers[0];

            var events = new EventBean[matching.Length];
            for (var i = 0; i < events.Length; i++) {
                eventsPerStream[1] = matching[i];
                events[i] = table.EventToPublic.Convert(matching[i], eventsPerStream, isNewData, context);
            }

            return events;
        }
    }
} // end of namespace