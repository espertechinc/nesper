///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.expression.core;

using XLR8.CGLib;

using com.espertech.esper.client;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.epl.expression.dot
{
    public class ExprDotMethodEvalNoDuckUnderlying : ExprDotMethodEvalNoDuck
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        public ExprDotMethodEvalNoDuckUnderlying(String statementName, FastMethod method, ExprEvaluator[] parameters)
            : base(statementName, method, parameters)
        {
        }
    
        public override Object Evaluate(Object target, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (target == null) {
                return null;
            }
            if (!(target is EventBean)) {
                Log.Warn("Expected EventBean return value but received '" + target.GetType().FullName + "' for statement " + base.StatementName);
                return null;
            }
            var bean = (EventBean) target;
            return base.Evaluate(bean.Underlying, eventsPerStream, isNewData, exprEvaluatorContext);
        }
    }
}
