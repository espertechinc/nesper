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
    ///     Accessor style defines the methods of a class that are automatically exposed via event property.
    /// </summary>
    public enum AccessorStyle
    { // ensure the names match the configuration schema type restriction defs
        /// <summary>
        ///     Expose native objects properties and getter methods only, plus explicitly configured properties.
        /// </summary>
        NATIVE,

        /// <summary>
        ///     Expose only the explicitly configured methods and public members as event properties.
        /// </summary>
        EXPLICIT,

        /// <summary>
        ///     Expose all public methods and public members as event properties, plus explicitly configured properties.
        /// </summary>
        PUBLIC
    }
} // end of namespace