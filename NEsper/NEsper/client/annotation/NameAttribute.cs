///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client.annotation
{
    /// <summary>
    /// Annotation for use in EPL statement to define a statement name.
    /// </summary>
    public class NameAttribute : Attribute
    {
        /// <summary>
        /// Returns the statement name.
        /// </summary>
        /// <returns>
        /// statement name
        /// </returns>
        public string Value { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NameAttribute"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public NameAttribute(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NameAttribute"/> class.
        /// </summary>
        public NameAttribute()
        {
        }

        public override string ToString()
        {
            return string.Format("@Name(\"{0}\")", Value);
        }
    }
}
