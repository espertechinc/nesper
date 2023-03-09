using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.script.core
{
    public class ScriptEvaluatorLambda : ScriptEvaluatorBase
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        private readonly Func<ScriptArgs, object> _scriptAction;

        public ScriptEvaluatorLambda(
            string scriptName,
            string[] parameterNames,
            ExprEvaluator[] parameters,
            Coercer coercer,
            ExpressionScriptCompiled compiled) : base(scriptName, parameterNames, parameters, coercer)
        {
            _scriptAction = compiled.ScriptAction;
        }

        private IDictionary<string, object> GetBindings(ExprEvaluatorContext exprEvaluatorContext)
        {
            var bindings = new Dictionary<string, object>();
            bindings.Put(ExprNodeScript.CONTEXT_BINDING_NAME, exprEvaluatorContext.AllocateAgentInstanceScriptContext);
            return bindings;
        }

        public override object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var bindings = GetBindings(context);
            for (var i = 0; i < ParameterNames.Length; i++) {
                var parameterName = ParameterNames[i];
                var parameterValue = Parameters[i].Evaluate(eventsPerStream, isNewData, context);
                bindings.Put(parameterName, parameterValue);
            }

            return EvaluateInternal(new ScriptArgs { Bindings = bindings, Context = context });
        }

        public override object Evaluate(
            object lookupValues,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var lookupValuesArray = lookupValues.UnwrapIntoArray<object>();
            var bindings = GetBindings(exprEvaluatorContext);
            for (var i = 0; i < ParameterNames.Length; i++) {
                var parameterName = ParameterNames[i];
                var parameterValue = lookupValuesArray[i];
                bindings.Put(parameterName, parameterValue);
            }

            return EvaluateInternal(new ScriptArgs { Bindings = bindings });
        }

        public object EvaluateInternal(ScriptArgs scriptArgs)
        {
            try {
                var result = _scriptAction.Invoke(scriptArgs);
                if (Coercer != null) {
                    result = Coercer.CoerceBoxed(result);
                }

                return result;
            }
            catch (Exception e) {
                string message = "Unexpected exception executing script '" + ScriptName + "': " + e.Message;
                Log.Error(message, e);
                throw new EPException(message, e);
            }
        }
    }
}