///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.table.core;

namespace com.espertech.esper.common.@internal.filterspec
{
    public sealed class ExprNodeAdapterWTableAccess : ExprNodeAdapterBase
    {
        private readonly ExprNodeAdapterBase evalBase;
        private readonly TableExprEvaluatorContext tableExprEvaluatorContext;

        public ExprNodeAdapterWTableAccess(
            FilterSpecParamExprNode factory,
            ExprEvaluatorContext evaluatorContext,
            ExprNodeAdapterBase evalBase,
            TableExprEvaluatorContext tableExprEvaluatorContext)
            : base(factory, evaluatorContext)

        {
            this.evalBase = evalBase;
            this.tableExprEvaluatorContext = tableExprEvaluatorContext;
        }

        public override bool Evaluate(EventBean theEvent)
        {
            try {
                return evalBase.Evaluate(theEvent);
            }
            finally {
                tableExprEvaluatorContext.ReleaseAcquiredLocks();
            }
        }
    }
} // end of namespace