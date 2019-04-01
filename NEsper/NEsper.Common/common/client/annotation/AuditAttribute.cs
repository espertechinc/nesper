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
    ///     Annotation for use in EPL statements to add a debug.
    /// </summary>
    public class AuditAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AuditAttribute" /> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public AuditAttribute(string value)
        {
            Value = value;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AuditAttribute" /> class.
        /// </summary>
        public AuditAttribute()
        {
            Value = "*";
        }

        /// <summary>
        ///     Comma-separated list of keywords (not case-sentitive), see <see cref="AuditEnum" /> for a list of keywords.
        /// </summary>
        /// <value>The value.</value>
        /// <returns>comma-separated list of audit keywords</returns>
        public virtual string Value { get; set; }
    }
} // end of namespace