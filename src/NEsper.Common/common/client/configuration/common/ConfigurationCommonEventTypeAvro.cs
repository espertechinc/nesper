///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.configuration.common
{
    /// <summary>
    ///     Configuration for Avro event types.
    /// </summary>
    [Serializable]
    public class ConfigurationCommonEventTypeAvro : ConfigurationCommonEventTypeWithSupertype
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        public ConfigurationCommonEventTypeAvro()
        {
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="avroSchema">avro schema</param>
        public ConfigurationCommonEventTypeAvro(object avroSchema)
        {
            AvroSchema = avroSchema;
        }

        /// <summary>
        ///     Returns the avro schema
        /// </summary>
        /// <returns>avro schema</returns>
        public object AvroSchema { get; set; }

        /// <summary>
        ///     Returns the avro schema text
        /// </summary>
        /// <returns>avro schema text</returns>
        public string AvroSchemaText { get; set; }

        /// <summary>
        ///     Sets the avro schema
        /// </summary>
        /// <param name="avroSchema">avro schema</param>
        /// <returns>this</returns>
        public ConfigurationCommonEventTypeAvro SetAvroSchema(object avroSchema)
        {
            AvroSchema = avroSchema;
            return this;
        }

        /// <summary>
        ///     Returns the avro schema text
        /// </summary>
        /// <param name="avroSchemaText">avro schema text</param>
        /// <returns>this</returns>
        public ConfigurationCommonEventTypeAvro SetAvroSchemaText(string avroSchemaText)
        {
            AvroSchemaText = avroSchemaText;
            return this;
        }
    }
} // end of namespace