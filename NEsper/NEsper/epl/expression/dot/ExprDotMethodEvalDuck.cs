///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.epl.expression.core;

using XLR8.CGLib;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.rettype;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.dot
{
    public class ExprDotMethodEvalDuck : ExprDotEval
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    
        private readonly String _statementName;
        private readonly MethodResolutionService _methodResolutionService;
        private readonly String _methodName;
        private readonly Type[] _parameterTypes;
        private readonly ExprEvaluator[] _parameters;
    
        private readonly IDictionary<Type, FastMethod> _cache;
    
        public ExprDotMethodEvalDuck(String statementName, MethodResolutionService methodResolutionService, String methodName, Type[] parameterTypes, ExprEvaluator[] parameters)
        {
            _statementName = statementName;
            _methodResolutionService = methodResolutionService;
            _methodName = methodName;
            _parameterTypes = parameterTypes;
            _parameters = parameters;
            _cache = new Dictionary<Type, FastMethod>();
        }
    
        public void Visit(ExprDotEvalVisitor visitor) {
            visitor.VisitMethod(_methodName);
        }
    
        public Object Evaluate(Object target, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext) {
            if (target == null) {
                return null;
            }
    
            FastMethod method;
            var targetType = target.GetType();
            if (_cache.ContainsKey(targetType)) {
                method = _cache.Get(targetType);
            }
            else {
                method = GetFastMethod(targetType);
                _cache.Put(targetType, method);
            }
    
            if (method == null) {
                return null;
            }

            var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext);
            var args = new Object[_parameters.Length];
    		for(int i = 0; i < args.Length; i++)
    		{
    		    args[i] = _parameters[i].Evaluate(evaluateParams);
    		}

            try
    		{
                return method.Invoke(target, args);
    		}
    		catch (TargetInvocationException e)
    		{
                String message = TypeHelper.GetMessageInvocationTarget(_statementName, method.Target, targetType.FullName, args, e);
                Log.Error(message, e.InnerException);
    		}
            return null;
        }
    
        private FastMethod GetFastMethod(Type clazz)
        {
            try
            {
                MethodInfo method = _methodResolutionService.ResolveMethod(clazz, _methodName, _parameterTypes, new bool[_parameterTypes.Length], new bool[_parameterTypes.Length]);
                FastClass declaringClass = FastClass.Create(method.DeclaringType);
                return declaringClass.GetMethod(method);
            }
            catch(Exception)
            {
                Log.Debug("Not resolved for class '" + clazz.Name + "' method '" + _methodName + "'");
            }
            return null;
        }

        public EPType TypeInfo
        {
            get { return EPTypeHelper.SingleValue(typeof (Object)); }
        }
    }
}
