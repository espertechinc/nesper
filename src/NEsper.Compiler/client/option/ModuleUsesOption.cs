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
    ///     Implement this interface to provide or override the module-uses at compile-time.
    /// </summary>

    public delegate ISet<string> ModuleUsesOption(ModuleUsesContext env);

#if DEPRECATED_INTERFACE
    public interface ModuleUsesOption
    {
        /// <summary>
        ///     Returns the module-uses to use or null if none is assigned.
        /// </summary>
        /// <param name="env">the module compile context</param>
        /// <returns>module-uses or null if none needs to be assigned</returns>
        ISet<string> GetValue(ModuleUsesContext env);
    }
#endif
} // end of namespace