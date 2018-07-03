///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.client.annotation;

namespace com.espertech.esper.supportregression.client
{
    public class MyAnnotationValueAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        [Required] public string Value { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MyAnnotationValueAttribute"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public MyAnnotationValueAttribute(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MyAnnotationValueAttribute"/> class.
        /// </summary>
        public MyAnnotationValueAttribute()
        {
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return base.ToString();
        }
    }
}
