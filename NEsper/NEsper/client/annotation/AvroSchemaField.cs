///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.client.annotation
{
    /// <summary>
    /// Attribute for use with Avro to provide a schema for a given event property.
    /// </summary>
    public class AvroSchemaField : Attribute 
    {
        /// <summary>
        /// Property name.
        /// </summary>
        /// <returns>name</returns>
        public string Name { get; set; }
    
        /// <summary>
        /// Schema text.
        /// </summary>
        /// <returns>schema text</returns>
        public string Schema { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AvroSchemaField"/> class.
        /// </summary>
        public AvroSchemaField()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AvroSchemaField"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="schema">The schema.</param>
        public AvroSchemaField(string name = "", string schema = "")
        {
            Name = name;
            Schema = schema;
        }
    }
} // end of namespace
