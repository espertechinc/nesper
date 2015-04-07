///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Castle.DynamicProxy;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.compat.collections;
using com.espertech.esper.events;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.core
{
    public class ExprEvaluatorProxy : IInterceptor
    {
        private static readonly MethodInfo TargetEvaluate = typeof(ExprEvaluator).GetMethod("Evaluate");
        private static readonly MethodInfo TargetEvaluateCollEvents = typeof(ExprEvaluatorEnumeration).GetMethod("EvaluateGetROCollectionEvents");
        private static readonly MethodInfo TargetEvaluateCollScalar = typeof(ExprEvaluatorEnumeration).GetMethod("EvaluateGetROCollectionScalar");
        private static readonly MethodInfo TargetEvaluateBean = typeof(ExprEvaluatorEnumeration).GetMethod("EvaluateGetEventBean");

        private static readonly Assembly CastleAssembly =
            typeof (ProxyGenerator).Assembly;

        private readonly String _engineURI;
        private readonly String _statementName;
        private readonly String _expressionToString;
        private readonly ExprEvaluator _evaluator;

        public static Object NewInstance(String engineURI, String statementName, String expressionToString, ExprEvaluator evaluator)
        {
            var generator = new ProxyGenerator();
            var interfaces = evaluator.GetType()
                .GetInterfaces()
                .Where(inf => inf.Assembly != CastleAssembly)
                .ToArray();

            return generator.CreateInterfaceProxyWithoutTarget(
                typeof(ExprEvaluator), interfaces,
                new ExprEvaluatorProxy(engineURI, statementName, expressionToString, evaluator));
        }

        public ExprEvaluatorProxy(String engineURI, String statementName, String expressionToString, ExprEvaluator evaluator)
        {
            _engineURI = engineURI;
            _statementName = statementName;
            _expressionToString = expressionToString;
            _evaluator = evaluator;
        }

        /// <summary>
        /// Intercepts the specified invocation.
        /// </summary>
        /// <param name="invocation">The invocation.</param>
        public void Intercept(IInvocation invocation)
        {
            var result = invocation.Method.Invoke(_evaluator, invocation.Arguments);
            invocation.ReturnValue = result;

            if (invocation.Method == TargetEvaluate)
            {
                if (AuditPath.IsAuditEnabled)
                {
                    AuditPath.AuditLog(_engineURI, _statementName, AuditEnum.EXPRESSION,
                                       "expression " + _expressionToString + " result " + result);
                }
            }
            else if (invocation.Method == TargetEvaluateCollEvents)
            {
                if (AuditPath.IsAuditEnabled)
                {
                    var resultBeans = (ICollection<EventBean>)result;
                    var @out = "null";
                    if (resultBeans != null)
                    {
                        if (resultBeans.IsEmpty())
                        {
                            @out = "{}";
                        }
                        else
                        {
                            var buf = new StringWriter();
                            var count = 0;
                            foreach (EventBean theEvent in resultBeans)
                            {
                                buf.Write(" Event ");
                                buf.Write(Convert.ToString(count++));
                                buf.Write(":");
                                EventBeanUtility.AppendEvent(buf, theEvent);
                            }
                            @out = buf.ToString();
                        }
                    }

                    AuditPath.AuditLog(_engineURI, _statementName, AuditEnum.EXPRESSION, "expression " + _expressionToString + " result " + @out);
                }
            }
            else if (invocation.Method == TargetEvaluateCollScalar)
            {
                if (AuditPath.IsAuditEnabled)
                {
                    AuditPath.AuditLog(_engineURI, _statementName, AuditEnum.EXPRESSION, "expression " + _expressionToString + " result " + result);
                }
            }
            else if (invocation.Method == TargetEvaluateBean)
            {
                if (AuditPath.IsAuditEnabled)
                {
                    var @out = "null";
                    if (result != null)
                    {
                        var buf = new StringWriter();
                        EventBeanUtility.AppendEvent(buf, (EventBean)result);
                        @out = buf.ToString();
                    }
                    AuditPath.AuditLog(_engineURI, _statementName, AuditEnum.EXPRESSION, "expression " + _expressionToString + " result " + @out);
                }
            }
        }
    }
}
