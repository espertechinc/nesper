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
    /// An execution directive for use in an EPL statement, by which processing of an
    /// event by statements start with the statement that has the highest priority,
    /// applicable only if multiple statements must process the same event.
    /// <para/>
    /// Ensure the engine configuration for prioritized execution is set before using
    /// this annotation.
    /// <para/>
    /// The default priority value is zero (0).
    /// </summary>
    public class PriorityAttribute : Attribute
    {
        /// <summary>
        /// Priority value.
        /// </summary>
        /// <returns>
        /// value
        /// </returns>
        public int Value { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriorityAttribute"/> class.
        /// </summary>
        public PriorityAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriorityAttribute"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public PriorityAttribute(int value)
        {
            Value = value;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format("@Priority({0})", Value);
        }
    }
}
