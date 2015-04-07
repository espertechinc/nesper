///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.table.mgmt;

namespace com.espertech.esper.filter
{
    public class ExprNodeAdapterBaseWTableAccess : ExprNodeAdapterBase
    {
        private readonly ExprNodeAdapterBase _evalBase;
        private readonly TableService _tableService;
    
        public ExprNodeAdapterBaseWTableAccess(string statementName, ExprNode exprNode, ExprEvaluatorContext evaluatorContext, ExprNodeAdapterBase evalBase, TableService tableService)
            : base(statementName, exprNode, evaluatorContext)
        {
            this._evalBase = evalBase;
            this._tableService = tableService;
        }
    
        public override bool Evaluate(EventBean theEvent)
        {
            try {
                return _evalBase.Evaluate(theEvent);
            }
            finally {
                 _tableService.TableExprEvaluatorContext.ReleaseAcquiredLocks();
            }
        }
    }
}
