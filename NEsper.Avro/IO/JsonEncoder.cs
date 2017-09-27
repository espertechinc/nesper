using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using AvroEncoder = Avro.IO.Encoder;

namespace NEsper.Avro.IO
{
    public class JsonEncoder : AvroEncoder
    {
        private readonly JsonWriter _jsonWriter;

        public JsonEncoder(TextWriter textWriter)
        {
            _jsonWriter = new JsonTextWriter(textWriter);
        }

        public void WriteNull()
        {
            _jsonWriter.WriteNull();
        }

        public void WriteBoolean(bool value)
        {
            _jsonWriter.WriteValue(value);
        }

        public void WriteInt(int value)
        {
            _jsonWriter.WriteValue(value);
        }

        public void WriteLong(long value)
        {
            _jsonWriter.WriteValue(value);
        }
        public void WriteFloat(float value)
        {
            _jsonWriter.WriteValue(value);
        }

        public void WriteDouble(double value)
        {
            _jsonWriter.WriteValue(value);
        }

        public void WriteBytes(byte[] value)
        {
            _jsonWriter.WriteValue(value);
        }

        public void WriteString(string value)
        {
            _jsonWriter.WriteValue(value);
        }

        public void WriteEnum(int value)
        {
            _jsonWriter.WriteValue(value);
        }

        public void SetItemCount(long value)
        {
        }

        public void StartItem()
        {
        }

        public void WriteArrayStart()
        {
            _jsonWriter.WriteStartArray();
        }

        public void WriteArrayEnd()
        {
            _jsonWriter.WriteEndArray();
        }

        public void WriteMapStart()
        {
            _jsonWriter.WriteStartObject();
        }

        public void WriteMapEnd()
        {
            _jsonWriter.WriteEndObject();
        }

        public void WriteUnionIndex(int value)
        {
            _jsonWriter.WriteValue(value);
        }

        public void WriteFixed(byte[] data)
        {
            _jsonWriter.WriteValue(data);
        }

        public void WriteFixed(byte[] data, int start, int len)
        {
            byte[] value = new byte[len];
            Array.Copy(data, start, value, 0, len);
            _jsonWriter.WriteValue(value);
        }
    }
}
