///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat.threading;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.variable;

namespace com.espertech.esper.filter
{
    public class ExprNodeAdapterMultiStreamNoTLStmtLock : ExprNodeAdapterMultiStreamNoTL
    {
        public ExprNodeAdapterMultiStreamNoTLStmtLock(
            int filterSpecId,
            int filterSpecParamPathNum, 
            ExprNode exprNode, 
            ExprEvaluatorContext evaluatorContext, 
            VariableService variableService, 
            EventBean[] prototype,
            IThreadLocalManager threadLocalManager)
            : base(
                filterSpecId, 
                filterSpecParamPathNum, 
                exprNode, 
                evaluatorContext, 
                variableService, 
                prototype,
                threadLocalManager)
        {
        }
    
        protected override bool EvaluatePerStream(EventBean[] eventsPerStream)
        {
            try
            {
                using (EvaluatorContext.AgentInstanceLock.WriteLock.Acquire(ExprNodeAdapterMultiStreamStmtLock.LOCK_BACKOFF_MSEC))
                {
                    return base.EvaluatePerStream(eventsPerStream);
                }
            }
            catch (TimeoutException)
            {
                throw new FilterLockBackoffException();
            }
        }
    }
}
