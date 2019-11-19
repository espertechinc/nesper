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
    public class TimeoutAttribute : Attribute
    {
        private int _value;

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public int Value {
            get => _value;
            set => _value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeoutAttribute"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public TimeoutAttribute(int value)
        {
            _value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeoutAttribute"/> class.
        /// </summary>
        public TimeoutAttribute()
        {
        }

        public override string ToString()
        {
            return string.Format("@SQLQueryTimeout(\"{0}\")", Value);
        }
    }
}