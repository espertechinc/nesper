///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.datetime.eval.reformat
{
    internal class DTLocalEvaluatorBeanIntervalWithEnd : DTLocalEvaluator
    {
        private readonly EventPropertyGetter _getterStartTimestamp;
        private readonly EventPropertyGetter _getterEndTimestamp;
        private readonly DTLocalEvaluatorIntervalComp _inner;

        internal DTLocalEvaluatorBeanIntervalWithEnd(
            EventPropertyGetter getterStartTimestamp,
            EventPropertyGetter getterEndTimestamp,
            DTLocalEvaluatorIntervalComp inner)
        {
            _getterStartTimestamp = getterStartTimestamp;
            _getterEndTimestamp = getterEndTimestamp;
            _inner = inner;
        }

        public object Evaluate(object target, EvaluateParams evaluateParams)
        {
            var startTimestamp = _getterStartTimestamp.Get((EventBean)target);
            if (startTimestamp == null)
            {
                return null;
            }
            var endTimestamp = _getterEndTimestamp.Get((EventBean)target);
            if (endTimestamp == null)
            {
                return null;
            }

            return _inner.Evaluate(startTimestamp, endTimestamp, evaluateParams);
        }
    }
}
