///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
    /// <summary>
    /// ScriptingService is a wrapper around the scripting engine and it's abstractions.
    /// </summary>
    public interface ScriptCompiler : IDisposable
    {
        /// <summary>
        /// Compiles the specified script given the specified dialect.
        /// </summary>
        /// <param name="dialect">The dialect.</param>
        /// <param name="script">The script.</param>
        /// <param name="importService"></param>
        /// <returns></returns>
        Func<ScriptArgs, object> Compile(
            string dialect,
            ExpressionScriptProvided script,
            ImportService importService);

        /// <summary>
        /// Verifies the script given the specified dialect.
        /// </summary>
        /// <param name="dialect">The dialect.</param>
        /// <param name="script">The script.</param>
        void VerifyScript(
            string dialect,
            ExpressionScriptProvided script);
    }
}