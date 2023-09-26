///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.datetime.dtlocal
{
    internal class DTLocalBeanReformatEval : DTLocalEvaluator
    {
        private readonly EventPropertyGetter getter;
        private readonly DTLocalEvaluator inner;

        public DTLocalBeanReformatEval(
            EventPropertyGetter getter,
            DTLocalEvaluator inner)
        {
            this.getter = getter;
            this.inner = inner;
        }

        public object Evaluate(
            object target,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var timestamp = getter.Get((EventBean)target);
            if (timestamp == null) {
                return null;
            }

            return inner.Evaluate(timestamp, eventsPerStream, isNewData, exprEvaluatorContext);
        }
    }
} // end of namespace