///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.util;

namespace com.espertech.esper.client
{
    /// <summary>Configuration for Avro event types.</summary>
    [Serializable]
    public class ConfigurationEventTypeAvro : ConfigurationEventTypeWithSupertype, MetaDefItem {
        private string avroSchemaText;
        private Object avroSchema;
    
        /// <summary>Ctor.</summary>
        public ConfigurationEventTypeAvro() {
        }
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="avroSchema">avro schema</param>
        public ConfigurationEventTypeAvro(Object avroSchema) {
            this.avroSchema = avroSchema;
        }
    
        /// <summary>
        /// Returns the avro schema
        /// </summary>
        /// <returns>avro schema</returns>
        public Object GetAvroSchema() {
            return avroSchema;
        }
    
        /// <summary>
        /// Sets the avro schema
        /// </summary>
        /// <param name="avroSchema">avro schema</param>
        /// <returns>this</returns>
        public ConfigurationEventTypeAvro SetAvroSchema(Object avroSchema) {
            this.avroSchema = avroSchema;
            return this;
        }
    
        /// <summary>
        /// Returns the avro schema text
        /// </summary>
        /// <returns>avro schema text</returns>
        public string GetAvroSchemaText() {
            return avroSchemaText;
        }
    
        /// <summary>
        /// Returns the avro schema text
        /// </summary>
        /// <param name="avroSchemaText">avro schema text</param>
        /// <returns>this</returns>
        public ConfigurationEventTypeAvro SetAvroSchemaText(string avroSchemaText) {
            this.avroSchemaText = avroSchemaText;
            return this;
        }
    }
} // end of namespace
