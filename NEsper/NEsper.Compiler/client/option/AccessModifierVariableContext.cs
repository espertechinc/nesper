///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.compile.stage3;

namespace com.espertech.esper.compiler.client.option
{
    /// <summary>
    ///     Provides the environment to <seealso cref="AccessModifierVariableOption" />.
    /// </summary>
    public class AccessModifierVariableContext : StatementOptionContextBase
    {
        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="base">statement info</param>
        /// <param name="variableName">returns the variable name</param>
        public AccessModifierVariableContext(
            StatementBaseInfo @base,
            string variableName)
            : base(@base)
        {
            VariableName = variableName;
        }

        /// <summary>
        ///     Returns the variable name
        /// </summary>
        /// <returns>the variable name</returns>
        public string VariableName { get; }
    }
} // end of namespace