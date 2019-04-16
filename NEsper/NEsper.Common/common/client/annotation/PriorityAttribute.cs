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
    /// An execution directive for use in an EPL statement, by which processing of an event by statements
    /// start with the statement that has the highest priority, applicable only if multiple statements must process the same event.
    /// <para />Ensure the runtime configuration for prioritized execution is set before using this annotation.
    /// <para />The default priority value is zero (0).
    /// </summary>
    public class PriorityAttribute : Attribute
    {
        /// <summary>
        /// Priority value.
        /// </summary>
        /// <returns>value</returns>
        public virtual int Value { get; set; }

        public PriorityAttribute(int value)
        {
            Value = value;
        }

        public PriorityAttribute()
        {
        }
    }
} // end of namespace