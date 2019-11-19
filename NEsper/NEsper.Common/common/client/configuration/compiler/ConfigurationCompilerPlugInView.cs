///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.configuration.compiler
{
    /// <summary>
    ///     Configuration information for plugging in a custom view.
    /// </summary>
    [Serializable]
    public class ConfigurationCompilerPlugInView
    {
        /// <summary>
        ///     Returns the namespace
        /// </summary>
        /// <returns>namespace</returns>
        public string Namespace { get; set; }

        /// <summary>
        ///     Returns the view name.
        /// </summary>
        /// <returns>view name</returns>
        public string Name { get; set; }

        /// <summary>
        ///     Returns the view forge class name.
        /// </summary>
        /// <returns>factory class name</returns>
        public string ForgeClassName { get; set; }
    }
} // end of namespace