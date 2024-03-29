///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.epl.contained
{
    public class ContainedEventEvalGetter : ContainedEventEval
    {
        private readonly EventPropertyFragmentGetter getter;

        public ContainedEventEvalGetter(EventPropertyFragmentGetter getter)
        {
            this.getter = getter;
        }

        public object GetFragment(
            EventBean eventBean,
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return getter.GetFragment(eventBean);
        }
    }
} // end of namespace