///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.util
{
    /// <summary>
    /// Visibility modifiers for EPL objects.
    /// </summary>
    public enum NameAccessModifier
    {
        /// <summary>
        /// Transient is used for non-visible objects that are only visible for the purpose of statement-internal processing.
        /// </summary>
        TRANSIENT,

        /// <summary>
        /// Private is used for objects that may be used with the same module.
        /// </summary>
        PRIVATE,

        /// <summary>
        /// Protected is used for objects that may be used with the modules of the same module name.
        /// </summary>
        PROTECTED,

        /// <summary>
        /// Public is used for objects that may be used by other modules irrespective of module names.
        /// </summary>
        PUBLIC,

        /// <summary>
        /// Preconfigured is used for objects that are preconfigured by configuration.
        /// </summary>
        PRECONFIGURED
    }
} // end of namespace