///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.historical.database.core;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.fafquery.processor
{
    public class FireAndForgetProcessorDBExecPrepared : FireAndForgetProcessorDBExecBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        public FireAndForgetProcessorDBExecPrepared(
            PollExecStrategyDBQuery poll,
            ExprEvaluator lookupValuesEval)
            : base(poll, lookupValuesEval)
        {
            poll.Start();
        }

        public ICollection<EventBean> PerformQuery(ExprEvaluatorContext exprEvaluatorContext)
        {
            if (Poll == null) {
                throw new EPException("Prepared fire-and-forget query is already closed");
            }

            return DoPoll(exprEvaluatorContext);
        }

        public void Close()
        {
            if (Poll != null) {
                try {
                    Poll.Done();
                }
                catch (Exception ex) {
                    Log.Error("Failed to return database poll resources: " + ex.Message, ex);
                }

                try {
                    Poll.Dispose();
                }
                catch (Exception ex) {
                    Log.Error("Failed to destroy database poll resources: " + ex.Message, ex);
                }

                Poll = null;
            }
        }
    }
} // end of namespace