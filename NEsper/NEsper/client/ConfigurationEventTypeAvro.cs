///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.util;

namespace com.espertech.esper.client
{
    /// <summary>Configuration for Avro event types.</summary>
    [Serializable]
    public class ConfigurationEventTypeAvro
        : ConfigurationEventTypeWithSupertype,
          MetaDefItem
    {
        private string _avroSchemaText;
        private Object _avroSchema;

        /// <summary>Ctor.</summary>
        public ConfigurationEventTypeAvro()
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="avroSchema">avro schema</param>
        public ConfigurationEventTypeAvro(Object avroSchema)
        {
            _avroSchema = avroSchema;
        }

        /// <summary>
        /// Returns the avro schema
        /// </summary>
        /// <value>avro schema</value>
        public object AvroSchema
        {
            get { return _avroSchema; }
            set { _avroSchema = value; }
        }

        /// <summary>
        /// Sets the avro schema
        /// </summary>
        /// <param name="avroSchema">avro schema</param>
        /// <returns>this</returns>
        public ConfigurationEventTypeAvro SetAvroSchema(Object avroSchema)
        {
            _avroSchema = avroSchema;
            return this;
        }

        /// <summary>
        /// Returns the avro schema text
        /// </summary>
        /// <value>avro schema text</value>
        public string AvroSchemaText
        {
            get { return _avroSchemaText; }
            set { _avroSchemaText = value; }
        }

        /// <summary>
        /// Returns the avro schema text
        /// </summary>
        /// <param name="avroSchemaText">avro schema text</param>
        /// <returns>this</returns>
        public ConfigurationEventTypeAvro SetAvroSchemaText(string avroSchemaText)
        {
            _avroSchemaText = avroSchemaText;
            return this;
        }
    }
} // end of namespace
