///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.updatehelper
{
    public interface EventBeanUpdateHelperNoCopy
    {
        string[] UpdatedProperties { get; }

        bool IsRequiresStream2InitialValueEvent { get; }

        void UpdateNoCopy(
            EventBean matchingEvent,
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext);
    }

    public class ProxyEventBeanUpdateHelperNoCopy : EventBeanUpdateHelperNoCopy
    {
        public delegate string[] UpdatedPropertiesFunc();

        public delegate bool IsRequiresStream2InitialValueEventFunc();

        public delegate void UpdateNoCopyFunc(
            EventBean matchingEvent,
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext);

        public UpdatedPropertiesFunc ProcUpdatedProperties { get; set; }
        public IsRequiresStream2InitialValueEventFunc ProcIsRequiresStream2InitialValueEvent { get; set; }
        public UpdateNoCopyFunc ProcUpdateNoCopy { get; set; }

        public string[] UpdatedProperties => ProcUpdatedProperties?.Invoke();

        public bool IsRequiresStream2InitialValueEvent => ProcIsRequiresStream2InitialValueEvent.Invoke();

        public void UpdateNoCopy(
            EventBean matchingEvent,
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            ProcUpdateNoCopy?.Invoke(matchingEvent, eventsPerStream, exprEvaluatorContext);
        }
    }
} // end of namespace