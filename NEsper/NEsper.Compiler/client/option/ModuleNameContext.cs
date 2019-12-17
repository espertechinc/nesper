///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.compiler.client.option
{
    /// <summary>
    ///     Provides the environment to <seealso cref="ModuleNameOption" />.
    /// </summary>
    public class ModuleNameContext
    {
        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="moduleNameProvided">module name or null when none provided</param>
        public ModuleNameContext(string moduleNameProvided)
        {
            ModuleNameProvided = moduleNameProvided;
        }

        /// <summary>
        ///     Returns the module name or null when none provided
        /// </summary>
        /// <returns>module name or null when none provided</returns>
        public string ModuleNameProvided { get; }
    }
} // end of namespace