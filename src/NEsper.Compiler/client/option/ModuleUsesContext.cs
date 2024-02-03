///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.compiler.client.option
{
    /// <summary>
    ///     Provides the environment to <seealso cref="ModuleUsesOption" />.
    /// </summary>
    public class ModuleUsesContext
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="moduleName">module name or null when none provided</param>
        /// <param name="moduleUsesProvided">module uses or null when none provided</param>
        public ModuleUsesContext(
            string moduleName,
            ICollection<string> moduleUsesProvided)
        {
            ModuleName = moduleName;
            ModuleUsesProvided = moduleUsesProvided;
        }

        /// <summary>
        ///     Returns the module name or null when none provided
        /// </summary>
        /// <returns>module name or null when none provided</returns>
        public string ModuleName { get; }

        /// <summary>
        ///     Returns the module uses or null when none provided
        /// </summary>
        /// <returns>module uses</returns>
        public ICollection<string> ModuleUsesProvided { get; }
    }
} // end of namespace