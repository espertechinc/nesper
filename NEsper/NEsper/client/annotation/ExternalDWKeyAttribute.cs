///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client.annotation
{
    /// <summary>
    /// Annotation for defining the name of the property or the function name returning the external data window key values.
    /// </summary>
    public class ExternalDWKeyAttribute : Attribute
    {
        public ExternalDWKeyAttribute()
        {
            Property = string.Empty;
            PropertyNames = new string[0];
            Function = string.Empty;
        }

        /// <summary>Property name acting as key. </summary>
        /// <returns>key property name</returns>
        public String Property { get; set; }

        /// <summary>Multiple property names acting as key (check for support in the documentation). </summary>
        /// <returns>property names array</returns>
        public String[] PropertyNames { get; set; }

        /// <summary>Value generator function. </summary>
        /// <returns>function name</returns>
        public String Function { get; set; }
    }
}
