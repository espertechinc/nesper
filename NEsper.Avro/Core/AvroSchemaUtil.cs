///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Avro;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.client.hook;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.events;
using com.espertech.esper.events.map;
using com.espertech.esper.util;

using Newtonsoft.Json.Linq;

using NEsper.Avro.Extensions;

namespace NEsper.Avro.Core
{
    public class AvroSchemaUtil
    {
        private static readonly Action<JArray, string> REQ_BOOLEAN = (t, name) => Required(t, name, "boolean");
        private static readonly Action<JArray, string> OPT_BOOLEAN = (t, name) => Optional(t, name, "boolean");
        private static readonly Action<JArray, string> REQ_INT = (t, name) => Required(t, name, "int");
        private static readonly Action<JArray, string> OPT_INT = (t, name) => Optional(t, name, "int");
        private static readonly Action<JArray, string> REQ_DOUBLE = (t, name) => Required(t, name, "double");
        private static readonly Action<JArray, string> OPT_DOUBLE = (t, name) => Optional(t, name, "double");
        private static readonly Action<JArray, string> REQ_FLOAT = (t, name) => Required(t, name, "float");
        private static readonly Action<JArray, string> OPT_FLOAT = (t, name) => Optional(t, name, "float");
        private static readonly Action<JArray, string> REQ_LONG = (t, name) => Required(t, name, "long");
        private static readonly Action<JArray, string> OPT_LONG = (t, name) => Optional(t, name, "long");
        private static readonly Action<JArray, string> REQ_STRING = (t, name) => Required(t, name, "string");
        private static readonly Action<JArray, string> OPT_STRING = (t, name) => Optional(t, name, "string");

        private static readonly JObject ARRAY_OF_REQ_BOOLEAN = TypeBuilder.Array("boolean");
        private static readonly JObject ARRAY_OF_OPT_BOOLEAN = TypeBuilder.Array(new JArray("null", "boolean"));

        private static readonly JObject ARRAY_OF_REQ_INT = TypeBuilder.Array("int");
        private static readonly JObject ARRAY_OF_OPT_INT = TypeBuilder.Array(new JArray("null", "int"));

        private static readonly JObject ARRAY_OF_REQ_LONG = TypeBuilder.Array("long");
        private static readonly JObject ARRAY_OF_OPT_LONG = TypeBuilder.Array(new JArray("null", "long"));

        private static readonly JObject ARRAY_OF_REQ_DOUBLE = TypeBuilder.Array("double");
        private static readonly JObject ARRAY_OF_OPT_DOUBLE = TypeBuilder.Array(new JArray("null", "double"));

        private static readonly JObject ARRAY_OF_REQ_FLOAT = TypeBuilder.Array("float");
        private static readonly JObject ARRAY_OF_OPT_FLOAT = TypeBuilder.Array(new JArray("null", "float"));

        internal static JArray Required(JArray array, String name, String type)
        {
            array.Add(TypeBuilder.Field(name, type));
            return array;
        }

        internal static JArray Optional(JArray array, String name, String type)
        {
            array.Add(TypeBuilder.Optional(name, type, null));
            return array;
        }

        internal static string ToSchemaStringSafe(Schema schema)
        {
            try
            {
                return schema.ToString();
            }
            catch (Exception t)
            {
                return "[Invalid schema: " + t.GetType().FullName + ": " + t.Message + "]";
            }
        }

        public static Schema FindUnionRecordSchemaSingle(Schema schema)
        {
            if (schema.Tag != Schema.Type.Union)
            {
                return null;
            }

            Schema found = null;

            UnionSchema unionSchema = (UnionSchema) schema;
            foreach (Schema member in unionSchema.Schemas)
            {
                if (member.Tag == Schema.Type.Record)
                {
                    if (found == null)
                    {
                        found = member;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            return found;
        }

        internal static void AssembleField(
            string propertyName,
            Object propertyType,
            JArray assembler,
            Attribute[] annotations,
            ConfigurationEngineDefaults.AvroSettings avroSettings,
            EventAdapterService eventAdapterService,
            string statementName,
            string engineURI,
            TypeRepresentationMapper optionalMapper)
        {
            if (propertyName.Contains("."))
            {
                throw new EPException(
                    "Invalid property name as Avro does not allow dot '.' in field names (property '" + propertyName +
                    "')");
            }

            Schema schema = GetAnnotationSchema(propertyName, annotations);
            if (schema != null)
            {
                assembler.Add(TypeBuilder.Field(propertyName, schema));
                // assembler.Name(propertyName).Type(schema).NoDefault();
                return;
            }

            if (optionalMapper != null && propertyType is Type)
            {
                var result = (Schema) optionalMapper.Map(
                    new TypeRepresentationMapperContext(
                        (Type) propertyType, propertyName, statementName, engineURI));
                if (result != null)
                {
                    assembler.Add(TypeBuilder.Field(propertyName, result));
                    // assembler.Name(propertyName).Type(result).NoDefault();
                    return;
                }
            }

            if (propertyType == null)
            {
                assembler.Add(TypeBuilder.Field(propertyName, TypeBuilder.NullType()));
                // assembler.Name(propertyName).Type("null");
            }
            else if (propertyType is string)
            {
                string propertyTypeName = propertyType.ToString();
                bool isArray = EventTypeUtility.IsPropertyArray(propertyTypeName);
                if (isArray)
                {
                    propertyTypeName = EventTypeUtility.GetPropertyRemoveArray(propertyTypeName);
                }

                // Add EventType itself as a property
                EventType eventType = eventAdapterService.GetEventTypeByName(propertyTypeName);
                if (!(eventType is AvroEventType))
                {
                    throw new EPException(
                        "Type definition encountered an unexpected property type name '"
                        + propertyType + "' for property '" + propertyName +
                        "', expected the name of a previously-declared Avro type");
                }
                schema = ((AvroEventType) eventType).SchemaAvro;

                if (!isArray)
                {
                    assembler.Add(TypeBuilder.Field(propertyName, schema));
                }
                else
                {
                    assembler.Add(TypeBuilder.Field(propertyName, TypeBuilder.Array(schema)));
                }
            }
            else if (propertyType is EventType)
            {
                var eventType = (EventType) propertyType;
                CheckCompatibleType(eventType);
                if (eventType is AvroEventType)
                {
                    schema = ((AvroEventType) eventType).SchemaAvro;
                    assembler.Add(TypeBuilder.Field(propertyName, schema));
                }
                else if (eventType is MapEventType)
                {
                    var mapEventType = (MapEventType) eventType;
                    var nestedSchema = AssembleNestedSchema(
                        mapEventType, avroSettings, annotations, eventAdapterService, statementName, engineURI,
                        optionalMapper);
                    assembler.Add(TypeBuilder.Field(propertyName, nestedSchema));
                }
                else
                {
                    throw new IllegalStateException("Unrecognized event type " + eventType);
                }
            }
            else if (propertyType is EventType[])
            {
                EventType eventType = ((EventType[]) propertyType)[0];
                CheckCompatibleType(eventType);
                if (eventType is AvroEventType)
                {
                    schema = ((AvroEventType) eventType).SchemaAvro;
                    assembler.Add(TypeBuilder.Field(propertyName, TypeBuilder.Array(schema)));
                }
                else if (eventType is MapEventType)
                {
                    var mapEventType = (MapEventType) eventType;
                    var nestedSchema = AssembleNestedSchema(
                        mapEventType, avroSettings, annotations, eventAdapterService, statementName, engineURI,
                        optionalMapper);

                    assembler.Add(TypeBuilder.Field(propertyName, TypeBuilder.Array(nestedSchema)));
                }
                else
                {
                    throw new IllegalStateException("Unrecognized event type " + eventType);
                }
            }
            else if (propertyType is Type)
            {
                var propertyClass = (Type) propertyType;
                var propertyClassBoxed = propertyClass.GetBoxedType();
                bool nullable = propertyClass == propertyClassBoxed;
                bool preferNonNull = avroSettings.IsEnableSchemaDefaultNonNull;
                if (propertyClassBoxed == typeof (bool?))
                {
                    AssemblePrimitive(nullable, REQ_BOOLEAN, OPT_BOOLEAN, assembler, propertyName, preferNonNull);
                }
                else if (propertyClassBoxed == typeof (int?) || propertyClassBoxed == typeof (byte?))
                {
                    AssemblePrimitive(nullable, REQ_INT, OPT_INT, assembler, propertyName, preferNonNull);
                }
                else if (propertyClassBoxed == typeof (long?))
                {
                    AssemblePrimitive(nullable, REQ_LONG, OPT_LONG, assembler, propertyName, preferNonNull);
                }
                else if (propertyClassBoxed == typeof (float?))
                {
                    AssemblePrimitive(nullable, REQ_FLOAT, OPT_FLOAT, assembler, propertyName, preferNonNull);
                }
                else if (propertyClassBoxed == typeof (double?))
                {
                    AssemblePrimitive(nullable, REQ_DOUBLE, OPT_DOUBLE, assembler, propertyName, preferNonNull);
                }
                else if (propertyClass == typeof (string))
                {
                    if (avroSettings.IsEnableNativeString)
                    {
                        if (preferNonNull)
                        {
                            assembler.Add(
                                TypeBuilder.Field(propertyName, 
                                    TypeBuilder.Primitive("string",
                                        TypeBuilder.Property(AvroConstant.PROP_STRING_KEY, AvroConstant.PROP_STRING_VALUE))));
                        }
                        else
                        {
                            assembler.Add(
                                TypeBuilder.Field(
                                    propertyName,
                                    TypeBuilder.Union(
                                        TypeBuilder.NullType(),
                                        TypeBuilder.StringType(
                                            TypeBuilder.Property(
                                                AvroConstant.PROP_STRING_KEY, AvroConstant.PROP_STRING_VALUE)))));
                        }
                    }
                    else
                    {
                        AssemblePrimitive(nullable, REQ_STRING, OPT_STRING, assembler, propertyName, preferNonNull);
                    }
                }
                else if (propertyClass == typeof (byte[]))
                {
                    if (preferNonNull)
                    {
                        assembler.Add(TypeBuilder.RequiredBytes(propertyName));
                    }
                    else
                    {
                        assembler.Add(TypeBuilder.Field(propertyName, TypeBuilder.Union(
                            TypeBuilder.NullType(), TypeBuilder.BytesType())));
                    }
                }
                else if (propertyClass.IsArray)
                {
                    var componentType = propertyClass.GetElementType();
                    var  componentTypeBoxed = componentType.GetBoxedType();
                    var nullableElements = componentType == componentTypeBoxed;

                    if (componentTypeBoxed == typeof (bool?))
                    {
                        AssembleArray(
                            nullableElements, ARRAY_OF_REQ_BOOLEAN, ARRAY_OF_OPT_BOOLEAN, assembler, propertyName, preferNonNull);
                    }
                    else if (componentTypeBoxed == typeof (int?))
                    {
                        AssembleArray(
                            nullableElements, ARRAY_OF_REQ_INT, ARRAY_OF_OPT_INT, assembler, propertyName, preferNonNull);
                    }
                    else if (componentTypeBoxed == typeof (long?))
                    {
                        AssembleArray(
                            nullableElements, ARRAY_OF_REQ_LONG, ARRAY_OF_OPT_LONG, assembler, propertyName, preferNonNull);
                    }
                    else if (componentTypeBoxed == typeof (float?))
                    {
                        AssembleArray(
                            nullableElements, ARRAY_OF_REQ_FLOAT, ARRAY_OF_OPT_FLOAT, assembler, propertyName, preferNonNull);
                    }
                    else if (componentTypeBoxed == typeof(double?))
                    {
                        AssembleArray(
                            nullableElements, ARRAY_OF_REQ_DOUBLE, ARRAY_OF_OPT_DOUBLE, assembler, propertyName, preferNonNull);
                    }
                    else if (componentTypeBoxed == typeof (byte?))
                    {
                        AssembleArray(
                            nullableElements, ARRAY_OF_REQ_INT, ARRAY_OF_OPT_INT, assembler, propertyName, preferNonNull);
                    }
                    else if (propertyClass == typeof (string[]))
                    {
                        JObject array;
                        if (avroSettings.IsEnableNativeString)
                        {
                            array = TypeBuilder.Array(TypeBuilder.StringType(TypeBuilder.Property(
                                AvroConstant.PROP_STRING_KEY, AvroConstant.PROP_STRING_VALUE)));
                        }
                        else
                        {
                            array = TypeBuilder.Array(TypeBuilder.StringType());
                        }

                        if (preferNonNull)
                        {
                            assembler.Add(TypeBuilder.Field(propertyName, array));
                        }
                        else
                        {
                            assembler.Add(TypeBuilder.Field(propertyName, TypeBuilder.Union(
                                TypeBuilder.NullType(), array)));
                        }
                    }
                    else if (propertyClass.CanUnwrap<object>())
                    {

                    }
                    else
                    {
                        throw MakeEPException(propertyName, propertyType);
                    }
                }
                else if (propertyClass.IsGenericDictionary())
                {
                    JToken value;
                    if (avroSettings.IsEnableNativeString)
                    {
                        value = TypeBuilder.StringType(
                            TypeBuilder.Property(AvroConstant.PROP_STRING_KEY, AvroConstant.PROP_STRING_VALUE));
                    }
                    else
                    {
                        value = TypeBuilder.StringType();
                    }

                    if (preferNonNull)
                    {
                        assembler.Add(TypeBuilder.Field(propertyName, TypeBuilder.Map(value)));
                    }
                    else
                    {
                        assembler.Add(TypeBuilder.Field(propertyName, TypeBuilder.Union(
                            TypeBuilder.NullType(), TypeBuilder.Map(value))));
                    }
                }
                else if (propertyClass.IsGenericCollection())
                {
                    AssembleFieldForCollection(propertyName, propertyType, assembler, avroSettings, propertyClass, preferNonNull);
                }
                else
                {
                    throw MakeEPException(propertyName, propertyType);
                }
            }
            else
            {
                throw MakeEPException(propertyName, propertyType);
            }
        }

        private static void AssembleFieldForCollection(
            string propertyName, 
            object propertyType, 
            JArray assembler,
            ConfigurationEngineDefaults.AvroSettings avroSettings, 
            Type propertyClass, 
            bool preferNonNull)
        {
            var componentType = propertyClass.GetIndexType();
            var componentTypeBoxed = componentType.GetBoxedType();
            var nullableElements = componentType == componentTypeBoxed;

            if (componentTypeBoxed == typeof(bool?))
            {
                AssembleArray(
                    nullableElements, ARRAY_OF_REQ_BOOLEAN, ARRAY_OF_OPT_BOOLEAN, assembler, propertyName, preferNonNull);
            }
            else if (componentTypeBoxed == typeof(int?))
            {
                AssembleArray(
                    nullableElements, ARRAY_OF_REQ_INT, ARRAY_OF_OPT_INT, assembler, propertyName, preferNonNull);
            }
            else if (componentTypeBoxed == typeof(long?))
            {
                AssembleArray(
                    nullableElements, ARRAY_OF_REQ_LONG, ARRAY_OF_OPT_LONG, assembler, propertyName, preferNonNull);
            }
            else if (componentTypeBoxed == typeof(float?))
            {
                AssembleArray(
                    nullableElements, ARRAY_OF_REQ_FLOAT, ARRAY_OF_OPT_FLOAT, assembler, propertyName, preferNonNull);
            }
            else if (componentTypeBoxed == typeof(double?))
            {
                AssembleArray(
                    nullableElements, ARRAY_OF_REQ_DOUBLE, ARRAY_OF_OPT_DOUBLE, assembler, propertyName, preferNonNull);
            }
            else if (componentTypeBoxed == typeof(byte?))
            {
                AssembleArray(
                    nullableElements, ARRAY_OF_REQ_INT, ARRAY_OF_OPT_INT, assembler, propertyName, preferNonNull);
            }
            else if (propertyClass == typeof(string[]))
            {
                JObject array;
                if (avroSettings.IsEnableNativeString)
                {
                    array = TypeBuilder.Array(TypeBuilder.StringType(TypeBuilder.Property(
                        AvroConstant.PROP_STRING_KEY, AvroConstant.PROP_STRING_VALUE)));
                }
                else
                {
                    array = TypeBuilder.Array(TypeBuilder.StringType());
                }

                if (preferNonNull)
                {
                    assembler.Add(TypeBuilder.Field(propertyName, array));
                }
                else
                {
                    assembler.Add(TypeBuilder.Field(propertyName, TypeBuilder.Union(
                        TypeBuilder.NullType(), array)));
                }
            }
            else
            {
                throw MakeEPException(propertyName, propertyType);
            }
        }

        private static Schema AssembleNestedSchema(
            MapEventType mapEventType,
            ConfigurationEngineDefaults.AvroSettings avroSettings,
            Attribute[] annotations,
            EventAdapterService eventAdapterService,
            string statementName,
            string engineURI,
            TypeRepresentationMapper optionalMapper)
        {
            var fields = new JArray();

            foreach (var prop in mapEventType.Types)
            {
                AssembleField(
                    prop.Key, prop.Value, fields, annotations, avroSettings, eventAdapterService, statementName,
                    engineURI, optionalMapper);
            }

            return SchemaBuilder.Record(mapEventType.Name, fields);
        }

        private static Schema GetAnnotationSchema(string propertyName, Attribute[] annotations)
        {
            if (annotations == null)
            {
                return null;
            }
            foreach (Attribute annotation in annotations)
            {
                if (annotation is AvroSchemaFieldAttribute)
                {
                    var avroSchemaField = (AvroSchemaFieldAttribute) annotation;
                    if (avroSchemaField.Name == propertyName)
                    {
                        string schema = avroSchemaField.Schema;
                        try
                        {
                            return Schema.Parse(schema);
                        }
                        catch (Exception ex)
                        {
                            throw new EPException(
                                "Failed to parse Avro schema for property '" + propertyName + "': " + ex.Message, ex);
                        }
                    }
                }
            }
            return null;
        }

        private static void CheckCompatibleType(EventType eventType)
        {
            if (!(eventType is AvroEventType) && !(eventType is MapEventType))
            {
                throw new EPException(
                    "Property type cannot be an event type with an underlying of type '" + eventType.UnderlyingType.Name +
                    "'");
            }
        }

        private static void AssemblePrimitive(
            bool nullable,
            Action<JArray, string> reqAssemble,
            Action<JArray, string> optAssemble,
            JArray assembler,
            string propertyName,
            bool preferNonNull)
        {
            if (preferNonNull)
            {
                reqAssemble.Invoke(assembler, propertyName);
            }
            else
            {
                if (nullable)
                {
                    optAssemble.Invoke(assembler, propertyName);
                }
                else
                {
                    reqAssemble.Invoke(assembler, propertyName);
                }
            }
        }

        private static void AssembleArray(
            bool nullableElements,
            JObject arrayOfReq,
            JObject arrayOfOpt,
            JArray fields,
            string propertyName,
            bool preferNonNull)
        {
            if (preferNonNull)
            {
                if (!nullableElements)
                {
                    fields.Add(TypeBuilder.Field(propertyName, arrayOfReq));
                }
                else
                {
                    fields.Add(TypeBuilder.Field(propertyName, arrayOfOpt));
                }
            }
            else
            {
                if (!nullableElements)
                {
                    // Schema union = SchemaBuilder.Union().NullType().And().Type(arrayOfReq).EndUnion();
                    // assembler.Name(propertyName).Type(union).NoDefault();
                    fields.Add(TypeBuilder.Union(TypeBuilder.NullType(), arrayOfReq));
                }
                else
                {
                    // Schema union = SchemaBuilder.Union().NullType().And().Type(arrayOfOpt).EndUnion();
                    // assembler.Name(propertyName).Type(union).NoDefault();
                    fields.Add(TypeBuilder.Union(TypeBuilder.NullType(), arrayOfOpt));
                }
            }
        }

        private static EPException MakeEPException(string propertyName, Object propertyType)
        {
            return
                new EPException(
                    "Property '" + propertyName + "' type '" + propertyType +
                    "' does not have a mapping to an Avro type");
        }
    }
} // end of namespace