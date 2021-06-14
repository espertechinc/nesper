///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.settings;

namespace com.espertech.esper.common.@internal.epl.script.core
{
    public class ExpressionNodeScriptCompiler
    {
        public const string DEFAULT_DIALECT = "js";
        
        public static ExpressionScriptCompiled CompileScript(
            string dialect,
            string scriptName,
            string expression,
            string[] parameterNames,
            Type[] evaluationTypes,
            ExpressionScriptCompiled optionalPrecompiled,
            ImportService importService,
            ScriptCompiler scriptingCompiler)
        {
#if NOT_USED
            ExpressionScriptCompiled compiled;
#endif
            if (optionalPrecompiled != null) {
                return optionalPrecompiled;
            }

            dialect ??= DEFAULT_DIALECT;
            return new ExpressionScriptCompiledImpl(
                scriptingCompiler.Compile(
                    dialect ?? DEFAULT_DIALECT,
                    new ExpressionScriptProvided(scriptName, expression, parameterNames, null, false, null, dialect)));
        }
    }
} // end of namespace