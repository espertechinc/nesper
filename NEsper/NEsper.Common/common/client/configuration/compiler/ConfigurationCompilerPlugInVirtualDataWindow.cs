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
    public class ConfigurationCompilerPlugInVirtualDataWindow
    {

        /// <summary>
        ///     Returns the namespace
        /// </summary>
        /// <value>namespace</value>
        public string Namespace { get; set; }

        /// <summary>
        ///     Returns the view name.
        /// </summary>
        /// <value>view name</value>
        public string Name { get; set; }

        /// <summary>
        ///     Returns the view factory class name.
        /// </summary>
        /// <value>factory class name</value>
        public string ForgeClassName { get; set; }

        /// <summary>
        ///     Returns any additional configuration passed to the factory as part of the context.
        /// </summary>
        /// <value>optional additional configuration</value>
        public object Config { get; set; }
    }
} // end of namespace