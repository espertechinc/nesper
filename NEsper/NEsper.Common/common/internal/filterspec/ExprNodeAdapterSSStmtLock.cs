///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.variable.core;

namespace com.espertech.esper.common.@internal.filterspec
{
    public sealed class ExprNodeAdapterSSStmtLock : ExprNodeAdapterBase
    {
        internal readonly VariableManagementService variableService;

        public ExprNodeAdapterSSStmtLock(
            FilterSpecParamExprNode factory,
            ExprEvaluatorContext evaluatorContext,
            VariableManagementService variableService)
            : base(factory, evaluatorContext)

        {
            this.variableService = variableService;
        }

        public override bool Evaluate(EventBean theEvent)
        {
            evaluatorContext.AgentInstanceLock.AcquireWriteLock();
            try {
                variableService.SetLocalVersion();
                return EvaluatePerStream(new[] {theEvent});
            }
            finally {
                evaluatorContext.AgentInstanceLock.ReleaseWriteLock();
            }
        }
    }
} // end of namespace