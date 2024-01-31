///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.ComponentModel;

namespace com.espertech.esper.common.client.annotation
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
        [DefaultValue("")]
        public virtual string Value { get; set; }

        [DefaultValue("")]
        public virtual string Model { get; set; }

        [DefaultValue(AppliesTo.UNDEFINED)]
        public virtual AppliesTo Applies { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HintAttribute" /> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="model">The model.</param>
        /// <param name="applies">What this hint applies to.</param>
        public HintAttribute(
            string value = "",
            string model = "",
            AppliesTo applies = AppliesTo.UNDEFINED)
        {
            Value = value;
            Model = model;
            Applies = applies;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HintAttribute"/> class.
        /// </summary>
        public HintAttribute()
        {
        }

        public override string ToString()
        {
            return $"@Hint(\"{Value}\")";
        }
    }
} // end of namespace