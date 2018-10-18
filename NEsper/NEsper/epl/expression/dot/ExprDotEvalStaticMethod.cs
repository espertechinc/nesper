///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using XLR8.CGLib;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.enummethod.dot;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.rettype;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.dot
{
    public class ExprDotEvalStaticMethod : ExprEvaluator, EventPropertyGetter
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static MethodInfo UnwrapCollection;
        private static MethodInfo UnwrapList;

        static ExprDotEvalStaticMethod()
        {
            UnwrapCollection = typeof(CompatExtensions).GetMethod("Unwrap", new Type[] { typeof(object), typeof(bool) });
            UnwrapList = typeof(CompatExtensions).GetMethod("UnwrapIntoList", new Type[] { typeof(object), typeof(bool) });
        }

        private readonly String _statementName;
        private readonly String _classOrPropertyName;
        private readonly FastMethod _staticMethod;
        private readonly ExprEvaluator[] _childEvals;
        private readonly bool _isConstantParameters;
        private readonly ExprDotEval[] _chainEval;
        private readonly ExprDotStaticMethodWrap _resultWrapLambda;
        private readonly bool _rethrowExceptions;
        private readonly Object _targetObject;

        private bool _isCachedResult;
        private Object _cachedResult;
        private Type[] _methodParameterTypes;

        public ExprDotEvalStaticMethod(String statementName,
                                       String classOrPropertyName,
                                       FastMethod staticMethod,
                                       ExprEvaluator[] childEvals,
                                       bool constantParameters,
                                       ExprDotStaticMethodWrap resultWrapLambda,
                                       ExprDotEval[] chainEval,
                                       bool rethrowExceptions,
                                       Object targetObject)
        {
            _statementName = statementName;
            _classOrPropertyName = classOrPropertyName;
            _staticMethod = staticMethod;
            _childEvals = childEvals;
            _targetObject = targetObject;
            _isConstantParameters = chainEval.Length <= 0 && constantParameters;
            _resultWrapLambda = resultWrapLambda;
            _chainEval = chainEval;
            _rethrowExceptions = rethrowExceptions;
            _methodParameterTypes = _staticMethod.Target.GetParameters()
                .Select(param => param.ParameterType)
                .ToArray();
        }

        public Type ReturnType
        {
            get
            {
                if (_chainEval.Length == 0)
                {
                    return _staticMethod.ReturnType;
                }
                else
                {
                    return EPTypeHelper.GetNormalizedClass(_chainEval[_chainEval.Length - 1].TypeInfo);
                }
            }
        }

        private object RewriteArgument(object value, Type parameterType)
        {
            if (value == null)
                return null;
            if (value.GetType() == parameterType)
                return value;
            if (TypeHelper.IsSubclassOrImplementsInterface(value.GetType(), parameterType))
                return value;

            // ICollection<object> gets passed around a lot internally rather than
            // the actual type required for final targeting.  If this is a collection
            // to collection target, then we can rewrite the values.
            if (value is ICollection<object>)
            {
                if (parameterType.IsGenericCollection())
                {
                    var genericType = parameterType.GetGenericArguments()[0];
                    return UnwrapCollection
                        .MakeGenericMethod(new Type[] { genericType })
                        .Invoke(null, new object[] { value, true });
                }
                else if (parameterType.IsGenericList())
                {
                    var genericType = parameterType.GetGenericArguments()[0];
                    return UnwrapList
                        .MakeGenericMethod(new Type[] { genericType })
                        .Invoke(null, new object[] { value, true });
                }
            }

            return value;
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QExprPlugInSingleRow(_staticMethod.Target); }
            if ((_isConstantParameters) && (_isCachedResult))
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprPlugInSingleRow(_cachedResult); }
                return _cachedResult;
            }

            //var methodParameters = _staticMethod.Target.GetParameters();

            var argsLength = _childEvals.Length;
            var args = new Object[argsLength];
            for (var i = argsLength - 1; i >= 0; i--)
            {
                var arg = _childEvals[i].Evaluate(evaluateParams);
                args[i] = RewriteArgument(arg, _methodParameterTypes[i]);
            }

            // The method is static so the object it is invoked on
            // can be null
            try
            {
                var result = _staticMethod.Invoke(_targetObject, args);

                result = ExprDotNodeUtility.EvaluateChainWithWrap(
                    _resultWrapLambda, result, null, _staticMethod.ReturnType, _chainEval, evaluateParams);

                if (_isConstantParameters)
                {
                    _cachedResult = result;
                    _isCachedResult = true;
                }

                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprPlugInSingleRow(result); }
                return result;
            }
            catch (EPException)
            {
                throw;
            }
            catch (TargetInvocationException e)
            {
                var message = TypeHelper.GetMessageInvocationTarget(
                    _statementName, _staticMethod.Target, _classOrPropertyName, args, e);
                Log.Error(message, e.InnerException);
                if (_rethrowExceptions)
                {
                    throw new EPException(message, e.InnerException);
                }
            }
            catch (Exception e)
            {
                var message = TypeHelper.GetMessageInvocationTarget(
                    _statementName, _staticMethod.Target, _classOrPropertyName, args, e);
                Log.Error(message, e);
                if (_rethrowExceptions)
                {
                    throw new EPException(message, e);
                }
            }

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprPlugInSingleRow(null); }
            return null;
        }

        public Object Get(EventBean eventBean)
        {
            var args = new Object[_childEvals.Length];
            for (var i = 0; i < args.Length; i++)
            {
                args[i] = _childEvals[i].Evaluate(new EvaluateParams(new EventBean[] { eventBean }, false, null));
            }

            // The method is static so the object it is invoked on
            // can be null
            try
            {
                return _staticMethod.Invoke(_targetObject, args);
            }
            catch (TargetInvocationException e)
            {
                var message = TypeHelper.GetMessageInvocationTarget(_statementName, _staticMethod.Target, _classOrPropertyName, args, e);
                Log.Error(message, e.InnerException);
                if (_rethrowExceptions)
                {
                    throw new EPException(message, e.InnerException);
                }
            }
            return null;
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return false;
        }

        public Object GetFragment(EventBean eventBean)
        {
            return null;
        }
    }
}
