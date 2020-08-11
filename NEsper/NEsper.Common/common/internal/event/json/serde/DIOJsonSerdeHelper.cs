///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.io;


namespace com.espertech.esper.common.@internal.@event.json.serde
{
	public class DIOJsonSerdeHelper
	{
		private static readonly byte NULL_TYPE = 0;
		private static readonly byte INT_TYPE = 1;
		private static readonly byte DOUBLE_TYPE = 2;
		private static readonly byte STRING_TYPE = 3;
		private static readonly byte BOOLEAN_TYPE = 4;
		private static readonly byte OBJECT_TYPE = 5;
		private static readonly byte ARRAY_TYPE = 6;

		public static void Write(
			IDictionary<string, object> @object,
			DataOutput output)
		{
			output.WriteInt(@object.Count);
			foreach (var entry in @object) {
				output.WriteUTF(entry.Key);
				WriteValue(entry.Value, output);
			}
		}

		public static IDictionary<string, object> Read(DataInput input)
		{
			var size = input.ReadInt();
			var map = new LinkedHashMap<string, object>();
			for (var i = 0; i < size; i++) {
				var key = input.ReadUTF();
				var value = ReadValue(input);
				map.Put(key, value);
			}

			return map;
		}

		public static void WriteValue(
			object value,
			DataOutput output)
		{
			if (value == null) {
				output.WriteByte(NULL_TYPE);
			}
			else if (value is int intValue) {
				output.WriteByte(INT_TYPE);
				output.WriteInt(intValue);
			}
			else if (value is double doubleValue) {
				output.WriteByte(DOUBLE_TYPE);
				output.WriteDouble(doubleValue);
			}
			else if (value is string stringValue) {
				output.WriteByte(STRING_TYPE);
				output.WriteUTF(stringValue);
			}
			else if (value is bool boolValue) {
				output.WriteByte(BOOLEAN_TYPE);
				output.WriteBoolean(boolValue);
			}
			else if (value is IDictionary<string, object> dictionary) {
				output.WriteByte(OBJECT_TYPE);
				Write(dictionary, output);
			}
			else if (value is object[] objectArray) {
				output.WriteByte(ARRAY_TYPE);
				WriteArray(objectArray, output);
			}
			else {
				throw new IOException("Unrecognized json object type value of type " + value.GetType() + "'");
			}
		}

		public static object ReadValue(DataInput input)
		{
			int type = input.ReadByte();
			if (type == NULL_TYPE) {
				return null;
			}
			else if (type == INT_TYPE) {
				return input.ReadInt();
			}
			else if (type == DOUBLE_TYPE) {
				return input.ReadDouble();
			}
			else if (type == STRING_TYPE) {
				return input.ReadUTF();
			}
			else if (type == BOOLEAN_TYPE) {
				return input.ReadBoolean();
			}
			else if (type == OBJECT_TYPE) {
				return Read(input);
			}
			else if (type == ARRAY_TYPE) {
				return ReadArray(input);
			}
			else {
				throw new IOException("Unrecognized json object type value of type " + type + "'");
			}
		}

		public static void WriteArray(
			object[] value,
			DataOutput output)
		{
			output.WriteInt(value.Length);
			foreach (var o in value) {
				WriteValue(o, output);
			}
		}

		public static object[] ReadArray(DataInput input)
		{
			var size = input.ReadInt();
			var result = new object[size];
			for (var i = 0; i < result.Length; i++) {
				result[i] = ReadValue(input);
			}

			return result;
		}
	}
} // end of namespace
