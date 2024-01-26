using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.util.serde
{
    public class JsonConverterRuntimeType : JsonConverter<System.Type>
    {
        private readonly TypeResolver _typeResolver;
        private readonly IDictionary<Type, bool> _typeResultCache;

        internal JsonConverterRuntimeType(TypeResolver typeResolver)
        {
            _typeResolver = typeResolver;
            _typeResultCache = new Dictionary<Type, bool>();
        }

        public override bool CanConvert(Type typeToConvert)
        {
            if (_typeResultCache.TryGetValue(typeToConvert, out var result)) {
                return result;
            }
            
            result = typeToConvert.GetBaseTypeTree().Any(_ => _ == typeof(Type));
            _typeResultCache[typeToConvert] = result;
            return result;
        }

        public override Type Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            switch (reader.TokenType) {
                case JsonTokenType.None:
                case JsonTokenType.Null:
                    return null;

                case JsonTokenType.String:
                    var typeName = reader.GetString();
                    return _typeResolver.ResolveType(typeName, true);

                default:
                    throw new SerializationException("invalid type representation");
            }
        }

        public override void Write(
            Utf8JsonWriter writer,
            Type value,
            JsonSerializerOptions options)
        {
            if (value == null) {
                writer.WriteNullValue();
            }
            else {
                writer.WriteStringValue(value.AssemblyQualifiedName);
            }
        }
    }
}