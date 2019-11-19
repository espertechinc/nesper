///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.module
{
    /// <summary>
    /// Provides well-known module properties.
    /// </summary>
    public enum ModuleProperty
    {
        /// <summary>
        /// The module URI
        /// </summary>
        URI,

        /// <summary>
        /// The module archive name
        /// </summary>
        ARCHIVENAME,

        /// <summary>
        /// The module text
        /// </summary>
        MODULETEXT,

        /// <summary>
        /// The module user object
        /// </summary>
        USEROBJECT,

        /// <summary>
        /// The module uses
        /// </summary>
        USES
    }
} // end of namespace