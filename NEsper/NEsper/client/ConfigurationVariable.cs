///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client
{
    /// <summary>
    /// Provides variable configuration.
    /// </summary>
	[Serializable]
    public class ConfigurationVariable
	{
        /// <summary>
        /// Gets or sets the variable type.
        /// </summary>
        /// <returns>variable type</returns>
        public string VariableType { get; set; }

        /// <summary>
        /// Gets or sets the initialization value, or null if none was supplied.
        /// String-type initialization values for numeric or bool types are allowed and are parsed.
        /// Variables are scalar values and primitive or boxed builtin types are accepted.
        /// </summary>
        /// <returns>default value</returns>
        public object InitializationValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ConfigurationVariable"/> is constant.
        /// </summary>
        /// <value><c>true</c> if constant; otherwise, <c>false</c>.</value>
        public bool IsConstant { get; set; }
	}
} // End of namespace
