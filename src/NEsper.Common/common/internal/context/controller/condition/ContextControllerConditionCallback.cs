///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;

namespace com.espertech.esper.common.@internal.context.controller.condition
{
    public interface ContextControllerConditionCallback
    {
        void RangeNotification(
            IntSeqKey conditionPath,
            ContextControllerConditionNonHA originEndpoint,
            EventBean optionalTriggeringEvent,
            IDictionary<string, object> optionalTriggeringPattern,
            EventBean optionalTriggeringEventPattern,
            IDictionary<string, object> optionalPatternForInclusiveEval,
            IDictionary<string, object> terminationProperties);
    }

    public delegate void ContextControllerConditionCallbackDelegate(
        IntSeqKey conditionPath,
        ContextControllerConditionNonHA originEndpoint,
        EventBean optionalTriggeringEvent,
        IDictionary<string, object> optionalTriggeringPattern,
        EventBean optionalTriggeringEventPattern,
        IDictionary<string, object> optionalPatternForInclusiveEval,
        IDictionary<string, object> terminationProperties);

    public class ProxyContextControllerConditionCallback : ContextControllerConditionCallback
    {
        public ContextControllerConditionCallbackDelegate ProcRangeNotification { get; set; }

        public ProxyContextControllerConditionCallback(ContextControllerConditionCallbackDelegate procRangeNotification)
        {
            ProcRangeNotification = procRangeNotification;
        }

        public ProxyContextControllerConditionCallback()
        {
        }

        public void RangeNotification(
            IntSeqKey conditionPath,
            ContextControllerConditionNonHA originEndpoint,
            EventBean optionalTriggeringEvent,
            IDictionary<string, object> optionalTriggeringPattern,
            EventBean optionalTriggeringEventPattern,
            IDictionary<string, object> optionalPatternForInclusiveEval,
            IDictionary<string, object> terminationProperties)
        {
            ProcRangeNotification(
                conditionPath,
                originEndpoint,
                optionalTriggeringEvent,
                optionalTriggeringPattern,
                optionalTriggeringEventPattern,
                optionalPatternForInclusiveEval,
                terminationProperties);
        }
    }
} // end of namespace