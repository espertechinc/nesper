///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.client.hook.expr;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.epl.script.core.ExprNodeScript;

namespace com.espertech.esper.common.@internal.epl.script.core
{
    public class ExpressionNodeScriptCompiler
    {
        public static ExpressionScriptCompiled CompileScript(
            string dialect,
            string scriptName,
            string expression,
            string[] parameterNames,
            Type[] evaluationTypes,
            ExpressionScriptCompiled optionalPrecompiled,
            ImportService importService)
        {
            ExpressionScriptCompiled compiled;
            if (optionalPrecompiled != null) {
                return optionalPrecompiled;
            }

            return JSR223Helper.VerifyCompileScript(scriptName, expression, dialect);
        }
    }
} // end of namespace