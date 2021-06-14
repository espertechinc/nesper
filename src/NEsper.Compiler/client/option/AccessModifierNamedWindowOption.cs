///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.util;

namespace com.espertech.esper.compiler.client.option
{
    /// <summary>
    ///     Implement this interface to provide or override the access modifier for a named window.
    /// </summary>
    public delegate NameAccessModifier AccessModifierNamedWindowOption(
        AccessModifierNamedWindowContext env);

#if DEPRECATED_INTERFACE
    public interface AccessModifierNamedWindowOption
    {
        /// <summary>
        ///     Returns the access modifier for the named window
        /// </summary>
        /// <param name="env">information about the statement</param>
        /// <returns>modifier</returns>
        NameAccessModifier GetValue(AccessModifierNamedWindowContext env);
    }
#endif
} // end of namespace