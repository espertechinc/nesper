///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.util;

namespace com.espertech.esper.common.client.configuration.compiler
{
    /// <summary>
    ///     Configuration information for plugging in a custom view.
    /// </summary>
    public class ConfigurationCompilerPlugInPatternObject
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
        ///     Returns an object type of the pattern object plug-in.
        /// </summary>
        /// <value>pattern object type</value>
        public PatternObjectType? PatternObjectType { get; set; }
    }
} // end of namespace