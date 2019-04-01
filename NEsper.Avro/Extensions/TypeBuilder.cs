using System.Collections.Generic;

using Avro;

using Newtonsoft.Json.Linq;

namespace NEsper.Avro.Extensions
{
    public class TypeBuilder
    {
        public static JObject Record(string name, JArray fields)
        {
            var schemaReducer = new SchemaReducer();
            var jsonDefinition = new JObject(
                new JProperty("type", "record"),
                new JProperty("name", name),
                new JProperty("fields", fields));

            schemaReducer.ReduceRecord(jsonDefinition);

            return jsonDefinition;
        }

        public static JObject Record(string name, params JObject[] fields)
        {
            var jsonDefinition = new JObject(
                new JProperty("type", "record"),
                new JProperty("name", name));

            var jsonFields = new JArray();
            foreach (var field in fields)
            {
                jsonFields.Add(field);
            }

            jsonDefinition.Add("fields", jsonFields);

            return jsonDefinition;
        }

        public static JObject Array(Schema type)
        {
            return new JObject(
                new JProperty("type", "array"),
                new JProperty("items", type.ToJsonObject()));
        }

        public static JObject Array(JToken itemType)
        {
            // "type" : "array",
            // "items" : {
            //     "type" : "string",
            //     "property" : "property-value"
            // }

            return new JObject(
                new JProperty("type", "array"),
                new JProperty("items", itemType));
        }

        public static JObject Map(JToken valueType)
        {
            return new JObject(
                new JProperty("type", "map"),
                new JProperty("values", valueType));
        }

        public static JObject Field(string name, string type)
        {
            return new JObject(
                new JProperty("name", name),
                new JProperty("type", type));
        }

        public static JObject Field(string name, JToken type)
        {
            return new JObject(
                new JProperty("name", name),
                new JProperty("type", type));
        }

        public static JObject Field(string name, Schema type)
        {
            var typeJson = type.ToJsonObject();
            return new JObject(
                new JProperty("name", name),
                new JProperty("type", typeJson));
        }

        public static JArray Union(params JToken[] types)
        {
            var unionArray = new JArray(types);
            return unionArray;
        }

        public static JArray Union(JToken token, Schema schema)
        {
            var unionArray = new JArray(token, schema.ToJsonObject());
            return unionArray;
        }

        public static JToken Primitive(string typeName, params JProperty[] properties)
        {
            if (properties.Length == 0)
            {
                return new JValue(typeName);
            }

            var typeInstance = new JObject(
                new JProperty("type", typeName));
            foreach (var property in properties)
            {
                typeInstance.Add(property);
            }

            return typeInstance;
        }

        public static JToken NullType()
        {
            return new JValue("null");
        }

        public static JToken BytesType(params JProperty[] properties)
        {
            return Primitive("bytes", properties);
        }

        public static JToken IntType(params JProperty[] properties)
        {
            return Primitive("int", properties);
        }

        public static JToken LongType(params JProperty[] properties)
        {
            return Primitive("long", properties);
        }

        public static JToken FloatType(params JProperty[] properties)
        {
            return Primitive("float", properties);
        }

        public static JToken DoubleType(params JProperty[] properties)
        {
            return Primitive("double", properties);
        }

        public static JToken StringType(params JProperty[] properties)
        {
            return Primitive("string", properties);
        }

        public static JToken BooleanType(params JProperty[] properties)
        {
            return Primitive("boolean", properties);
        }

        public static JProperty Property(string propertyName, string propertyValue)
        {
            return new JProperty(propertyName, propertyValue);
        }

        public static JObject Required(string name, string type)
        {
            return new JObject(
                new JProperty("name", name),
                new JProperty("type", type));
        }

        public static JObject RequiredBytes(string name)
        {
            return Required(name, "bytes");
        }

        public static JObject RequiredInt(string name)
        {
            return Required(name, "int");
        }

        public static JObject RequiredLong(string name)
        {
            return Required(name, "long");
        }

        public static JObject RequiredFloat(string name)
        {
            return Required(name, "float");
        }

        public static JObject RequiredDouble(string name)
        {
            return Required(name, "double");
        }

        public static JObject RequiredString(string name)
        {
            return Required(name, "string");
        }

        public static readonly JValue NULL_VALUE = new JValue("null");

        public static JObject Optional(string name, string type, JValue defaultValue)
        {
            return new JObject(
                new JProperty("name", name),
                new JProperty("type", new JArray(new JValue("null"), type)),
                new JProperty("default", defaultValue ?? NULL_VALUE));
        }

        public static JObject OptionalInt(string name, JValue defaultValue = null)
        {
            return Optional(name, "int", defaultValue);
        }

        public static JObject OptionalLong(string name, JValue defaultValue = null)
        {
            return Optional(name, "long", defaultValue);
        }

        public static JObject OptionalFloat(string name, JValue defaultValue = null)
        {
            return Optional(name, "float", defaultValue);
        }

        public static JObject OptionalDouble(string name, JValue defaultValue = null)
        {
            return Optional(name, "double", defaultValue);
        }

        public static JObject OptionalString(string name, JValue defaultValue = null)
        {
            return Optional(name, "string", defaultValue);
        }

        public static JObject TypeWithProperty(string typeName, string propertyKey, string propertyValue)
        {
            return new JObject(
                new JProperty("type", typeName),
                new JProperty(propertyKey, propertyValue));
        }
    }
}
