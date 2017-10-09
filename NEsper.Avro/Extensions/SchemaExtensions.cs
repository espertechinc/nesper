///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using Avro;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using com.espertech.esper.compat.collections;
using NEsper.Avro.IO;

namespace NEsper.Avro.Extensions
{
    public static class SchemaExtensions
    {
        /// <summary>
        /// Coverts the JSON structure to an avro schema.
        /// </summary>
        /// <param name="jsonObject">The json object.</param>
        /// <returns></returns>
        public static Schema ToAvro(this JToken jsonObject)
        {
            return Schema.Parse(jsonObject.ToString(Formatting.None));
        }

        /// <summary>
        /// Coverts the JSON structure to an avro record schema.
        /// </summary>
        /// <param name="jsonObject">The json object.</param>
        /// <returns></returns>
        public static RecordSchema ToAvroRecord(this JObject jsonObject)
        {
            return ToAvro(jsonObject).AsRecordSchema();
        }

        /// <summary>
        /// Decompiles the schema into a json object.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <returns></returns>
        public static JToken ToJsonObject(this Schema schema)
        {
            var jsonRepr = SchemaToJsonEncoder.Encode(schema);
            return jsonRepr;
        }

        /// <summary>
        /// Converts the schema to a union schema or throws an exception.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">schema of incorrect type;schema</exception>
        public static UnionSchema AsUnionSchema(this Schema schema)
        {
            return AsSchema<UnionSchema>(schema);
        }

        /// <summary>
        /// Converts the schema to a map schema or throws an exception.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">schema of incorrect type;schema</exception>
        public static MapSchema AsMapSchema(this Schema schema)
        {
            return AsSchema<MapSchema>(schema);
        }

        /// <summary>
        /// Converts the schema to an array schema or throws an exception.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">schema of incorrect type;schema</exception>
        public static ArraySchema AsArraySchema(this Schema schema)
        {
            return AsSchema<ArraySchema>(schema);
        }

        /// <summary>
        /// Converts the schema to a record schema or throws an exception.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">schema of incorrect type;schema</exception>
        public static RecordSchema AsRecordSchema(this Schema schema)
        {
            return AsSchema<RecordSchema>(schema);
        }

        /// <summary>
        /// Converts the schema to a typed schema or throws an exception.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">schema of incorrect type;schema</exception>
        public static TSchema AsSchema<TSchema>(this Schema schema) where TSchema : Schema
        {
            var castSchema = schema as TSchema;
            if (castSchema == null)
            {
                throw new ArgumentException("schema of incorrect type", nameof(schema));
            }

            return castSchema;
        }

        /// <summary>
        /// Gets element type for array schemas.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <returns></returns>
        public static Schema GetElementType(this Schema schema)
        {
            return schema.AsArraySchema().ItemSchema;
        }

        /// <summary>
        /// Gets value type for map schemas.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <returns></returns>
        public static Schema GetValueType(this Schema schema)
        {
            return schema.AsMapSchema().ValueSchema;
        }

        /// <summary>
        /// Returns the fields associated with the specified schema.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <returns></returns>
        public static ICollection<Field> GetFields(this Schema schema)
        {
            return schema.AsRecordSchema().Fields;
        }

        /// <summary>
        /// Returns the field associated with the specified name.
        /// </summary>
        /// <param name="recordSchema">The record schema.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns></returns>
        public static Field GetField(this Schema recordSchema, string fieldName)
        {
            return recordSchema.AsRecordSchema()[fieldName];
        }

        /// <summary>
        /// Gets a property from the schema.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <returns></returns>
        public static PropertyMap GetPropertyMap(this Schema schema)
        {
            var property = typeof(Schema).GetProperty("Props", BindingFlags.Instance | BindingFlags.NonPublic);
            if (property == null)
            {
                return null;
            }

            var getMethod = property.GetGetMethod(true);
            if (getMethod == null)
            {
                return null;
            }

            var propertyMap = (PropertyMap)getMethod.Invoke(schema, NO_ARGS);
            if (propertyMap == null)
            {
                return null;
            }

            return propertyMap;
        }

        /// <summary>
        /// Gets a property from the schema.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static string GetProp(this Schema schema, String name)
        {
            var property = typeof(Schema).GetProperty("Props", BindingFlags.Instance | BindingFlags.NonPublic);
            if (property == null)
            {
                return null;
            }

            var getMethod = property.GetGetMethod(true);
            if (getMethod == null)
            {
                return null;
            }

            var propertyMap = (PropertyMap) getMethod.Invoke(schema, NO_ARGS);
            if (propertyMap == null)
            {
                return null;
            }

            return propertyMap.Get(name);
        }

        private static readonly object[] NO_ARGS = new object[0];
    }
}
