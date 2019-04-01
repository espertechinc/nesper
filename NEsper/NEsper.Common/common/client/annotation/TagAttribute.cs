///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.annotation
{
    /// <summary>
    /// Annotation for use in EPL statement to tag a statement with a name-value pair.
    /// </summary>
    public class TagAttribute : Attribute
    {
        /// <summary>
        /// Returns the tag name.
        /// </summary>
        /// <returns>
        /// tag name.
        /// </returns>
        public virtual string Name { get; set; }

        /// <summary>
        /// Returns the tag value.
        /// </summary>
        /// <returns>
        /// tag value.
        /// </returns>
        public virtual string Value { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TagAttribute"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public TagAttribute(string name, string value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TagAttribute"/> class.
        /// </summary>
        public TagAttribute()
        {
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format("@Tag(Name=\"{0}\", Value=\"{1}\")", Name, Value);
        }
    }
} // end of namespace
