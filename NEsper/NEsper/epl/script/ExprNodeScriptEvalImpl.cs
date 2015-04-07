///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.script;

namespace com.espertech.esper.epl.script
{
    using ScriptAction = Func<ScriptArgs,Object>;

    public class ExprNodeScriptEvalImpl : ExprNodeScriptEvalBase
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ScriptAction _scriptAction;

        public ExprNodeScriptEvalImpl(String scriptName, String statementName, String[] names, ExprEvaluator[] parameters, Type returnType, Func<ScriptArgs,Object> scriptAction)
            : base(scriptName, statementName, names, parameters, returnType)
        {
            _scriptAction = scriptAction;
        }

        public override Object Evaluate(EvaluateParams evaluateParams)
        {
            var scriptArgs = new ScriptArgs();
            var bindings = new Dictionary<string, object>();
            bindings.Put(ExprNodeScript.CONTEXT_BINDING_NAME, evaluateParams.ExprEvaluatorContext.AgentInstanceScriptContext);
            for (int i = 0; i < Names.Length; i++)
            {
                bindings.Put(Names[i], Parameters[i].Evaluate(evaluateParams));
            }

            scriptArgs.Bindings = bindings;

            try
            {
                var result = _scriptAction.Invoke(scriptArgs);

                if (Coercer != null)
                {
                    result = Coercer.Invoke(result);
                }

                return result;
            }
            catch (Exception e)
            {
                String message = "Unexpected exception executing script '" + ScriptName + "' for statement '" + StatementName + "' : " + e.Message;
                Log.Error(message, e);
                throw new EPException(message, e);
            }
        }
    }
}
