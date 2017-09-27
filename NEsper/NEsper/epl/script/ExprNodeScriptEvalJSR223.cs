///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.core;

using javax.script;

namespace com.espertech.esper.epl.script
{
    public class ExprNodeScriptEvalJSR223 : ExprNodeScriptEvalBase, ExprNodeScriptEvaluator {
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        private readonly CompiledScript executable;
    
        public ExprNodeScriptEvalJSR223(string scriptName, string statementName, string[] names, ExprEvaluator[] parameters, Type returnType, EventType eventTypeCollection, CompiledScript executable) {
            Super(scriptName, statementName, names, parameters, returnType, eventTypeCollection);
            this.executable = executable;
        }
    
        public Object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
            Bindings bindings = GetBindings(context);
            for (int i = 0; i < names.Length; i++) {
                bindings.Put(names[i], parameters[i].Evaluate(eventsPerStream, isNewData, context));
            }
            return EvaluateInternal(bindings);
        }
    
        public Object Evaluate(Object[] lookupValues, ExprEvaluatorContext context) {
            Bindings bindings = GetBindings(context);
            for (int i = 0; i < names.Length; i++) {
                bindings.Put(names[i], lookupValues[i]);
            }
            return EvaluateInternal(bindings);
        }
    
        private Bindings GetBindings(ExprEvaluatorContext context) {
            Bindings bindings = executable.Engine.CreateBindings();
            bindings.Put(ExprNodeScript.CONTEXT_BINDING_NAME, context.AllocateAgentInstanceScriptContext);
            return bindings;
        }
    
        private Object EvaluateInternal(Bindings bindings) {
            try {
                Object result = executable.Eval(bindings);
    
                if (coercer != null) {
                    return Coercer.CoerceBoxed((Number) result);
                }
    
                return result;
            } catch (ScriptException e) {
                string message = "Unexpected exception executing script '" + scriptName + "' for statement '" + statementName + "' : " + e.Message;
                Log.Error(message, e);
                throw new EPException(message, e);
            }
        }
    }
} // end of namespace
