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
    ///     Provides the environment to <seealso cref="AccessModifierNamedWindowOption" />.
    /// </summary>
    public class AccessModifierNamedWindowContext : StatementOptionContextBase
    {
        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="base">statement info</param>
        /// <param name="namedWindowName">named window name</param>
        public AccessModifierNamedWindowContext(
            StatementBaseInfo @base,
            string namedWindowName)
            : base(@base)
        {
            NamedWindowName = namedWindowName;
        }

        /// <summary>
        ///     Returns the named window name
        /// </summary>
        /// <returns>named window name</returns>
        public string NamedWindowName { get; }
    }
} // end of namespace