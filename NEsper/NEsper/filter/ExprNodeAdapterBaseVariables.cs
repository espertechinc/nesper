///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.variable;

namespace com.espertech.esper.filter
{
    public class ExprNodeAdapterBaseVariables : ExprNodeAdapterBase
    {
        private readonly VariableService _variableService;

        /// <summary>
        /// Gets the variable service.
        /// </summary>
        /// <value>The variable service.</value>
        protected VariableService VariableService
        {
            get { return _variableService; }
        }

        public ExprNodeAdapterBaseVariables(int filterSpecId, int filterSpecParamPathNum, ExprNode exprNode, ExprEvaluatorContext evaluatorContext, VariableService variableService)
            : base(filterSpecId, filterSpecParamPathNum, exprNode, evaluatorContext)
        {
            _variableService = variableService;
        }
    
        public override bool Evaluate(EventBean theEvent)
        {
            _variableService.SetLocalVersion();
            return EvaluatePerStream(new [] {theEvent});
        }
    }
}
