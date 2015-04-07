///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.epl.expression.core;

using XLR8.CGLib;

using com.espertech.esper.client;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.rettype;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.dot
{
    [Serializable]
    public class ExprDotMethodEvalNoDuck : ExprDotEval
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    
        protected readonly String StatementName;
        protected readonly FastMethod Method;
        private readonly ExprEvaluator[] _parameters;
    
        public ExprDotMethodEvalNoDuck(String statementName, FastMethod method, ExprEvaluator[] parameters)
        {
            StatementName = statementName;
            Method = method;
            _parameters = parameters;
        }
    
        public void Visit(ExprDotEvalVisitor visitor)
        {
            visitor.VisitMethod(Method.Name);
        }
    
        public virtual Object Evaluate(Object target, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (target == null) {
                return null;
            }
    
    		var args = new Object[_parameters.Length];
            var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext);
            for (int i = 0; i < args.Length; i++)
    		{
    		    args[i] = _parameters[i].Evaluate(evaluateParams);
    		}

            try
    		{
                return Method.Invoke(target, args);
    		}
            catch (TargetInvocationException e)
    		{
                String message = TypeHelper.GetMessageInvocationTarget(StatementName, Method.Target, target.GetType().FullName, args, e);
                Log.Error(message, e.InnerException);
    		}
            return null;
        }

        public virtual EPType TypeInfo
        {
            get { return EPTypeHelper.FromMethod(Method.Target); }
        }
    }
}
