using Avro;

using Newtonsoft.Json.Linq;

namespace NEsper.Avro.Extensions
{
    public static class SchemaBuilder
    {
        public static ArraySchema Array(JToken itemType)
        {
            return SchemaExtensions
                .ToAvro(TypeBuilder.Array(itemType))
                .AsArraySchema();
        }

        public static UnionSchema Union(params JToken[] types)
        {
            return SchemaExtensions
                .ToAvro(TypeBuilder.Union(types))
                .AsUnionSchema();
        }

        public static RecordSchema Record(string name, params JObject[] fields)
        {
            return SchemaExtensions
                .ToAvro(TypeBuilder.Record(name, fields))
                .AsRecordSchema();
        }

        public static RecordSchema Record(string name, JArray fields)
        {
            return SchemaExtensions
                .ToAvro(TypeBuilder.Record(name, fields))
                .AsRecordSchema();
        }
    }
}
