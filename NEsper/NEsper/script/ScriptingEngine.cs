///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.spec;

namespace com.espertech.esper.script
{
    /// <summary>
    /// Scripting implementations must implement the ScriptingEngine interface.
    /// </summary>

    public interface ScriptingEngine
    {
        /// <summary>
        /// Gets the language associated with this engine.  e.g. Javascript
        /// </summary>
        /// <value>The language.</value>
        string Language { get; }

        /// <summary>
        /// Gets the language prefix to use with this engine.  e.g. js
        /// </summary>
        /// <value>The language prefix.</value>
        string LanguagePrefix { get; }

        /// <summary>
        /// Compiles the code.
        /// </summary>
        /// <param name="expressionScript">The expression script.</param>
        /// <returns></returns>
        Func<ScriptArgs, Object> Compile(ExpressionScriptProvided expressionScript);

        /// <summary>
        /// Verifies the specified script.
        /// </summary>
        /// <param name="script">The script.</param>
        void Verify(ExpressionScriptProvided script);
    }
}
