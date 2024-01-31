///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.historical.execstrategy;
using com.espertech.esper.common.@internal.epl.historical.method.poll;

namespace com.espertech.esper.common.@internal.epl.historical.method.core
{
    public class PollExecStrategyMethod : PollExecStrategy
    {
        private readonly MethodConversionStrategy _methodConversionStrategy;
        private readonly MethodTargetStrategy _methodTargetStrategy;

        public PollExecStrategyMethod(
            MethodTargetStrategy methodTargetStrategy,
            MethodConversionStrategy methodConversionStrategy)
        {
            _methodTargetStrategy = methodTargetStrategy;
            _methodConversionStrategy = methodConversionStrategy;
        }

        public void Start()
        {
            // no action
        }

        public IList<EventBean> Poll(
            object lookupValues,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var result = _methodTargetStrategy.Invoke(lookupValues, exprEvaluatorContext);
            if (result != null) {
                return _methodConversionStrategy.Convert(result, _methodTargetStrategy, exprEvaluatorContext);
            }

            return null;
        }

        public void Done()
        {
            // no action
        }

        public void Dispose()
        {
            // no action
        }
    }
} // end of namespace