///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.script.core
{
    public class ScriptEvaluatorCompilerRuntime
    {
        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="descriptor">descriptor</param>
        /// <returns>evaluator</returns>
        public static ScriptEvaluator CompileScriptEval(ScriptDescriptorRuntime descriptor)
        {
            var dialect = descriptor.OptionalDialect ?? descriptor.DefaultDialect;
            try {
                var compiled = ExpressionNodeScriptCompiler.CompileScript(
                    dialect,
                    descriptor.ScriptName,
                    descriptor.Expression,
                    descriptor.ParameterNames,
                    descriptor.EvaluationTypes,
                    null,
                    descriptor.ImportService,
                    descriptor.ScriptCompiler);

                return new ScriptEvaluatorLambda(
                    descriptor.ScriptName,
                    descriptor.ParameterNames,
                    descriptor.Parameters,
                    descriptor.Coercer,
                    compiled);
            }
            catch (ExprValidationException ex) {
                throw new EPException("Failed to compile script '" + descriptor.ScriptName + "': " + ex.Message);
            }
        }
    }
} // end of namespace