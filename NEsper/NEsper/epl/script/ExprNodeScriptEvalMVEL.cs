///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.script.mvel;

namespace com.espertech.esper.epl.script
{
    public class ExprNodeScriptEvalMVEL : ExprNodeScriptEvalBase, ExprNodeScriptEvaluator {
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        private readonly Object executable;
    
        public ExprNodeScriptEvalMVEL(string scriptName, string statementName, string[] names, ExprEvaluator[] parameters, Type returnType, EventType eventTypeCollection, Object executable) {
            Super(scriptName, statementName, names, parameters, returnType, eventTypeCollection);
            this.executable = executable;
        }
    
        public Object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
            IDictionary<string, Object> paramsList = GetParamsList(context);
            for (int i = 0; i < names.Length; i++) {
                paramsList.Put(names[i], parameters[i].Evaluate(eventsPerStream, isNewData, context));
            }
            return EvaluateInternal(paramsList);
        }
    
        public Object Evaluate(Object[] lookupValues, ExprEvaluatorContext context) {
            IDictionary<string, Object> paramsList = GetParamsList(context);
            for (int i = 0; i < names.Length; i++) {
                paramsList.Put(names[i], lookupValues[i]);
            }
            return EvaluateInternal(paramsList);
        }
    
        private IDictionary<string, Object> GetParamsList(ExprEvaluatorContext context) {
            var paramsList = new Dictionary<string, Object>();
            paramsList.Put(ExprNodeScript.CONTEXT_BINDING_NAME, context.AllocateAgentInstanceScriptContext);
            return paramsList;
        }
    
        private Object EvaluateInternal(IDictionary<string, Object> paramsList) {
            try {
                Object result = MVELInvoker.ExecuteExpression(executable, paramsList);
    
                if (coercer != null) {
                    return Coercer.CoerceBoxed((Number) result);
                }
    
                return result;
            } catch (InvocationTargetException ex) {
                Throwable mvelException = ex.Cause;
                string message = "Unexpected exception executing script '" + scriptName + "' for statement '" + statementName + "' : " + mvelException.Message;
                Log.Error(message, mvelException);
                throw new EPException(message, ex);
            }
        }
    }
} // end of namespace
