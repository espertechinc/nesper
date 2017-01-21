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
    /// Annotation for providing a statement execution hint.
    /// <para/>
    /// Hints are providing instructions that can change latency, throughput or memory
    /// requirements of a statement.
    /// </summary>
    public class HintAttribute : Attribute
    {
        /// <summary>
        /// Hint Keyword(s), comma-separated.
        /// </summary>
        /// <returns>
        /// keywords
        /// </returns>
        public string Value { get; set; }

        public string Model { get; set; }

        public AppliesTo Applies { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HintAttribute"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public HintAttribute(string value = "", string model = "", AppliesTo applies = AppliesTo.UNDEFINED)
        {
            Value = value;
            Model = model;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HintAttribute"/> class.
        /// </summary>
        public HintAttribute()
        {
        }

        public override string ToString()
        {
            return string.Format("@Hint(\"{0}\")", Value);
        }
    }
}
