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
    ///     Implement this interface to provide or override the module name at compile-time.
    /// </summary>

    public delegate string ModuleNameOption(ModuleNameContext env);

#if DEPRECATED_INTERFACE
    public interface ModuleNameOption
    {
        /// <summary>
        ///     Returns the module name to use or null if none is assigned.
        /// </summary>
        /// <param name="env">the module compile context</param>
        /// <returns>module name or null if none needs to be assigned</returns>
        string GetValue(ModuleNameContext env);
    }
#endif
} // end of namespace