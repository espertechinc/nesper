///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.core.context.factory
{
    public interface StatementAgentInstancePreload
    {
        void ExecutePreload(ExprEvaluatorContext exprEvaluatorContext);
    }

    public class ProxyStatementAgentInstancePreload : StatementAgentInstancePreload
    {
        public Action<ExprEvaluatorContext> ProcExecutePreload { get; set; }

        /// <summary>
        /// Executes the preload.
        /// </summary>
        public void ExecutePreload(ExprEvaluatorContext exprEvaluatorContext)
        {
            if (ProcExecutePreload != null)
                ProcExecutePreload(exprEvaluatorContext);
        }
    }
}