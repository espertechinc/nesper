///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.table.mgmt;

namespace com.espertech.esper.epl.table.onaction
{
    public class TableOnViewUtil
    {
        public static EventBean[] ToPublic(
            EventBean[] matching,
            TableMetadata tableMetadata,
            EventBean[] triggers,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var eventsPerStream = new EventBean[2];
            eventsPerStream[0] = triggers[0];

            var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, context);
            var events = new EventBean[matching.Length];
            for (var i = 0; i < events.Length; i++)
            {
                eventsPerStream[1] = matching[i];
                events[i] = tableMetadata.EventToPublic.Convert(matching[i], evaluateParams);
            }
            return events;
        }
    }
}
