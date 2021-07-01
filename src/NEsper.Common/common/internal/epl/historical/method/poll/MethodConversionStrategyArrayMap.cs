///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;

namespace com.espertech.esper.common.@internal.epl.historical.method.poll
{
    public class MethodConversionStrategyArrayMap : MethodConversionStrategyArray
    {
        protected override EventBean GetEventBean(
            object value,
            AgentInstanceContext agentInstanceContext)
        {
            return agentInstanceContext.EventBeanTypedEventFactory.AdapterForTypedMap(
                (IDictionary<string, object>) value,
                eventType);
        }
    }
} // end of namespace