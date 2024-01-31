///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.configuration.common
{
    /// <summary>
    ///     Provides variable configuration.
    /// </summary>
    public class ConfigurationCommonVariable
    {
        /// <summary>
        ///     Returns the variable type as a fully-qualified class name, primitive type or event type name.
        /// </summary>
        /// <value>type name</value>
        public string VariableType { get; set; }

        /// <summary>
        ///     Returns the initialization value, or null if none was supplied.
        ///     <para />
        ///     String-type initialization values for numeric or boolean types are allowed and are parsed.
        /// </summary>
        /// <value>default value</value>
        public object InitializationValue { get; set; }

        /// <summary>
        ///     Returns true if the variable is a constant, or false for regular variable.
        /// </summary>
        /// <value>true for constant, false for variable</value>
        public bool IsConstant { get; set; }
    }
} // end of namespace