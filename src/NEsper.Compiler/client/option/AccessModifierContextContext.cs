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
    ///     Provides the environment to <seealso cref="AccessModifierContextOption" />.
    /// </summary>
    public class AccessModifierContextContext : StatementOptionContextBase
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="base">statement info</param>
        /// <param name="contextName">context name</param>
        public AccessModifierContextContext(
            StatementBaseInfo @base,
            string contextName)
            : base(@base)
        {
            ContextName = contextName;
        }

        /// <summary>
        ///     Returns the context name.
        /// </summary>
        /// <returns>context name</returns>
        public string ContextName { get; }
    }
} // end of namespace