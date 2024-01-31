///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.compile.stage3;

namespace com.espertech.esper.compiler.client.option
{
    /// <summary>
    ///     Provides the environment to <seealso cref="AccessModifierScriptOption" />.
    /// </summary>
    public class AccessModifierScriptContext : StatementOptionContextBase
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="base">statement info</param>
        /// <param name="scriptName">script name</param>
        /// <param name="numParameters">script number of parameters</param>
        public AccessModifierScriptContext(
            StatementBaseInfo @base,
            string scriptName,
            int numParameters)
            : base(@base)
        {
            ScriptName = scriptName;
            NumParameters = numParameters;
        }

        /// <summary>
        ///     Returns the script name
        /// </summary>
        /// <returns>script name</returns>
        public string ScriptName { get; }

        /// <summary>
        ///     Returns the script number of parameters
        /// </summary>
        /// <returns>script number of parameters</returns>
        public int NumParameters { get; }
    }
} // end of namespace