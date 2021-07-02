///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.filterspec;

namespace com.espertech.esper.common.@internal.context.controller.condition
{
    public interface ContextControllerEndConditionMatchEventProvider
    {
        void PopulateEndConditionFromTrigger(
            MatchedEventMap map,
            EventBean triggeringEvent);
        
        void PopulateEndConditionFromTrigger(
            MatchedEventMap map,
            IDictionary<string, object> triggeringPattern,
            EventBeanTypedEventFactory eventBeanTypedEventFactory);
    }

    public class ProxyContextControllerEndConditionMatchEventProvider : ContextControllerEndConditionMatchEventProvider
    {
        public Action<MatchedEventMap, EventBean> ProcPopulateEndConditionFromTriggerWithEventBean;
        public Action<MatchedEventMap, IDictionary<string, object>, EventBeanTypedEventFactory> ProcPopulateEndConditionFromTriggerWithPattern;

        public ProxyContextControllerEndConditionMatchEventProvider()
        {
        }

        public ProxyContextControllerEndConditionMatchEventProvider(
            Action<MatchedEventMap, EventBean> procPopulateEndConditionFromTriggerWithEventBean,
            Action<MatchedEventMap, IDictionary<string, object>, EventBeanTypedEventFactory> procPopulateEndConditionFromTriggerWithPattern)
        {
            ProcPopulateEndConditionFromTriggerWithEventBean = procPopulateEndConditionFromTriggerWithEventBean;
            ProcPopulateEndConditionFromTriggerWithPattern = procPopulateEndConditionFromTriggerWithPattern;
        }

        public void PopulateEndConditionFromTrigger(
            MatchedEventMap map,
            EventBean triggeringEvent)
        {
            ProcPopulateEndConditionFromTriggerWithEventBean?.Invoke(map, triggeringEvent);
        }

        public void PopulateEndConditionFromTrigger(
            MatchedEventMap map,
            IDictionary<string, object> triggeringPattern,
            EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            ProcPopulateEndConditionFromTriggerWithPattern?.Invoke(map, triggeringPattern, eventBeanTypedEventFactory);
        }
    }
} // end of namespace