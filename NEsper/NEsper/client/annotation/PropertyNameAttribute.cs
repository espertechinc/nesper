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
    public class PropertyNameAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyNameAttribute"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public PropertyNameAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyNameAttribute"/> class.
        /// </summary>
        public PropertyNameAttribute()
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
            return string.Format("@PropertyName(\"{0}\")", Name);
        }
    }
}
