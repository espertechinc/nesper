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
    /// Attribute for use with Avro to provide a schema for a given event property.
    /// </summary>
    public class AvroSchemaFieldAttribute : Attribute
    {
        /// <summary>
        /// Property name.
        /// </summary>
        /// <returns>name</returns>
        public virtual string Name { get; set; }

        /// <summary>
        /// Schema text.
        /// </summary>
        /// <returns>schema text</returns>
        public virtual string Schema { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AvroSchemaFieldAttribute"/> class.
        /// </summary>
        public AvroSchemaFieldAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AvroSchemaFieldAttribute"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="schema">The schema.</param>
        public AvroSchemaFieldAttribute(
            string name = "",
            string schema = "")
        {
            Name = name;
            Schema = schema;
        }
    }
} // end of namespace