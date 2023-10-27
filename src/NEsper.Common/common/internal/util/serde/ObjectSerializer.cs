using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.util.serde
{
    public partial class ObjectSerializer : Serializer
    {
        private JsonSerializerOptions _options;
        private TypeResolver _typeResolver;

        public JsonSerializerOptions Options {
            get => _options;
            set => _options = value;
        }

        public TypeResolver TypeResolver {
            get => _typeResolver;
            set => _typeResolver = value;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="typeResolver"></param>
        public ObjectSerializer(TypeResolver typeResolver)
        {
            _typeResolver = typeResolver;
            _options = new JsonSerializerOptions() {
                
                TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
                UnknownTypeHandling = JsonUnknownTypeHandling.JsonElement,
                Converters = {
                    new JsonConverterImport(),
                    new JsonConverterImportBuiltin(),
                    new JsonConverterDictionaryWithTypeInfo(),
                    new JsonConverterRuntimeType(_typeResolver),
                    new JsonConverterTimeZoneInfo()
                }
            };
        }

        /// <summary>
        /// Returns true if the serializer accepts this type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool Accepts(Type type)
        {
            return true;
        }

        /// <summary>
        /// Serialize an object
        /// </summary>
        /// <param name="obj"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public byte[] SerializeAny(object obj)
        {
            var stream = new MemoryStream();
            var writer = new Utf8JsonWriter(stream);

            if (obj == null) {
                writer.WriteNullValue();
                return stream.ToArray();
            }

            var typeName = obj.GetType().AssemblyQualifiedName;
            Console.WriteLine(typeName);
            writer.WriteStartObject();
            writer.WriteString("__type", typeName);
            writer.WritePropertyName("__data");
            JsonSerializer.Serialize(writer, obj, _options);
            writer.WriteEndObject();
            writer.Flush();

            return stream.ToArray();
        }

        /// <summary>
        /// Deserialize an object from the source stream.
        /// </summary>
        public object DeserializeAny(byte[] buffer)
        {
            var reader = new Utf8JsonReader(buffer, isFinalBlock: true, state: default);
            if (!reader.Read()) {
                throw new SerializationException("invalid data representation");
            }

            switch (reader.TokenType) {
                case JsonTokenType.None:
                case JsonTokenType.Null:
                    return null;
                case JsonTokenType.StartObject:
                    var document = JsonDocument.ParseValue(ref reader);
                    var rootElement = document.RootElement;
                    if (rootElement.TryGetProperty("__type", out JsonElement typeElement) &&
                        rootElement.TryGetProperty("__data", out JsonElement dataElement)) {

                        // Validate that the typeElement is a string
                        if (typeElement.ValueKind == JsonValueKind.String) {
                            var typeName = typeElement.GetString();
                            var type = _typeResolver.ResolveType(typeName);
                            var data = dataElement.GetRawText();
                            return JsonSerializer.Deserialize(data, type, _options);
                        }

                        throw new SerializationException("invalid type representation");
                    }
                    
                    throw new SerializationException("malformed data representation");
                default:
                    throw new SerializationException("invalid data representation");
            }
        }
    }
}