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
#if DEPRECATED_INTERFACE
    public interface EventBeanUpdateHelperWCopy
    {
        EventBean UpdateWCopy(
            EventBean matchingEvent,
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext);
    }
#else

    public delegate EventBean EventBeanUpdateHelperWCopy(
        EventBean matchingEvent,
        EventBean[] eventsPerStream,
        ExprEvaluatorContext exprEvaluatorContext);

#endif
} // end of namespace