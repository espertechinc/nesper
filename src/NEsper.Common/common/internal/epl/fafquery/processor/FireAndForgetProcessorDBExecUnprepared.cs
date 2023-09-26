///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.historical.database.core;

namespace com.espertech.esper.common.@internal.epl.fafquery.processor
{
    public class FireAndForgetProcessorDBExecUnprepared : FireAndForgetProcessorDBExecBase
    {
        public FireAndForgetProcessorDBExecUnprepared(
            PollExecStrategyDBQuery poll,
            ExprEvaluator lookupValuesEval)
            : base(poll, lookupValuesEval)
        {
        }

        public ICollection<EventBean> PerformQuery(ExprEvaluatorContext exprEvaluatorContext)
        {
            Poll.Start();
            try {
                return DoPoll(exprEvaluatorContext);
            }
            finally {
                Poll.Done();
                Poll.Dispose();
            }
        }
    }
} // end of namespace