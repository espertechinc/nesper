///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.variable.core;

namespace com.espertech.esper.common.@internal.filterspec
{
    public sealed class ExprNodeAdapterMSStmtLock : ExprNodeAdapterMSBase
    {
        public const long LOCK_BACKOFF_MSEC = 10;
        private readonly VariableManagementService _variableService;

        internal ExprNodeAdapterMSStmtLock(
            FilterSpecParamExprNode factory,
            ExprEvaluatorContext evaluatorContext,
            EventBean[] prototype,
            VariableManagementService variableService)
            : base(factory, evaluatorContext, prototype)

        {
            _variableService = variableService;
        }

        public override bool Evaluate(EventBean theEvent)
        {
            var eventsPerStream = new EventBean[prototypeArray.Length];
            Array.Copy(prototypeArray, 0, eventsPerStream, 0, prototypeArray.Length);
            eventsPerStream[0] = theEvent;

            if (_variableService != null) {
                _variableService.SetLocalVersion();
            }

            var obtained = evaluatorContext.AgentInstanceLock.AcquireWriteLock(LOCK_BACKOFF_MSEC);
            if (!obtained) {
                throw new FilterLockBackoffException();
            }

            try {
                return EvaluatePerStream(eventsPerStream);
            }
            finally {
                evaluatorContext.AgentInstanceLock.ReleaseWriteLock();
            }
        }
    }
} // end of namespace