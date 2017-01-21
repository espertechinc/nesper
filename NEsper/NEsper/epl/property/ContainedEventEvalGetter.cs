///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.property
{
    public class ContainedEventEvalGetter : ContainedEventEval
    {

        private readonly EventPropertyGetter _getter;

        public ContainedEventEvalGetter(EventPropertyGetter getter)
        {
            _getter = getter;
        }

        public Object GetFragment(EventBean eventBean, EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
            return _getter.GetFragment(eventBean);
        }
    }
}
