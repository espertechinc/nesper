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
    internal class DTLocalEvaluatorBeanCalOps : DTLocalEvaluator
    {
        private readonly EventPropertyGetter _getter;
        private readonly DTLocalEvaluator _inner;

        internal DTLocalEvaluatorBeanCalOps(EventPropertyGetter getter, DTLocalEvaluator inner)
        {
            _getter = getter;
            _inner = inner;
        }

        public object Evaluate(object target, EvaluateParams evaluateParams)
        {
            var timestamp = _getter.Get((EventBean)target);
            if (timestamp == null)
            {
                return null;
            }
            return _inner.Evaluate(timestamp, evaluateParams);
        }
    }
}
