using System;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

namespace NEsper.Avro.Extensions
{
    public class SchemaReducer
    {
        private ISet<string> _schemas;

        public SchemaReducer()
        {
            _schemas = new HashSet<string>();
        }

        public JToken ReduceSimpleType(JToken token)
        {
            return token;
        }

        public JToken ReduceComplexType(JObject schema)
        {
            var typeType = schema.Value<string>("type");
            var typeName = schema.Value<string>("name");
            if (typeName != null)
            {
                if (_schemas.Contains(typeName))
                {
                    // this is a named schema type that has already been added to
                    // the encompasing schema.  more accurately, we've already parsed
                    // this schema once.  we want to reduce this schema "out"

                    return JValue.CreateString(typeName);
                }

                // this is a new schema... at a bare minimum, we need to save the
                // schema for future reductions.
                _schemas.Add(typeName);
            }

            // now we need to descend into the atoms of the nested type
            switch (typeType)
            {
                case "array":
                    ReduceArray(schema);
                    break;
                case "record":
                    ReduceRecord(schema);
                    break;
                case "map":
                    ReduceMap(schema);
                    break;
            }

            return schema;
        }

        public JToken ReduceUnionType(JArray schema)
        {
            return schema;
        }

        public void ReduceType(JProperty typeProperty)
        {
            var typeValue = typeProperty.Value;
            if (typeValue.Type == JTokenType.String)
            {
                typeProperty.Value = ReduceSimpleType(typeValue);
            }
            else if (typeValue.Type == JTokenType.Object)
            {
                typeProperty.Value = ReduceComplexType((JObject) typeValue);
            }
            else if (typeValue.Type == JTokenType.Array)
            {
                typeProperty.Value = ReduceUnionType((JArray) typeValue);
            }
            else
            {
                throw new ArgumentException("unknown schema definition");
            }
        }

        public void ReduceField(JObject field)
        {
            ReduceType(field.Property("type"));
        }

        public void ReduceFields(JArray fields)
        {
            foreach (var field in fields.Children())
            {
                var fieldAsObject = field as JObject;
                if (fieldAsObject != null)
                {
                    ReduceField(fieldAsObject);
                }
            }
        }

        public void ReduceRecord(JObject record)
        {
            var fields = record.Value<JArray>("fields");
            if (fields != null)
            {
                ReduceFields(fields);
            }
        }

        public void ReduceArray(JObject schema)
        {
            ReduceType(schema.Property("items"));
        }

        public void ReduceMap(JObject schema)
        {
            ReduceType(schema.Property("values"));
        }
    }
}
