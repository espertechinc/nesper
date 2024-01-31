///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text.Json.Serialization;

using com.espertech.esper.common.@internal.util.serde;

namespace com.espertech.esper.common.client.soda
{
    public class AnnotationAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnnotationAttribute"/> class.
        /// </summary>
        public AnnotationAttribute()
        {
        }

        /// <summary>
        ///   <para>
        ///  Initializes a new instance of the <see cref="AnnotationAttribute"/> class.
        /// </para>
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public AnnotationAttribute(
            string name,
            object value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; set; }

        [JsonConverter(typeof(JsonConverterAbstract<object>))]
        public object Value { get; set; }
    }
}