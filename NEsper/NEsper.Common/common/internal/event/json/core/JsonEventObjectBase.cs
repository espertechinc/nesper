///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

using com.espertech.esper.common.client.json.util;
using com.espertech.esper.common.@internal.@event.json.serde;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.@event.json.serializers.JsonSerializerUtil;

namespace com.espertech.esper.common.@internal.@event.json.core
{
	public abstract class JsonEventObjectBase : JsonEventObject
	{
		/// <summary>
		/// Add a dynamic property value that the json parser encounters.
		/// Dynamic property values are not predefined and are catch-all in nature.
		/// </summary>
		/// <param name="name">name</param>
		/// <param name="value">value</param>
		public virtual void AddJsonValue(
			string name,
			object value)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Returns the dynamic property values (the non-predefined values)
		/// </summary>
		/// <value>map</value>
		public virtual IDictionary<string, object> JsonValues => EmptyDictionary<string, object>.Instance;

		/// <summary>
		/// Returns the set of property names that are visible to the caller.
		/// </summary>
		public ICollection<string> PropertyNames => Enumerable.Concat(JsonValues.Select(_ => _.Key),NativeKeys).ToList();
		
		#region Native

		/// <summary>
		/// Returns the total number of pre-declared properties available including properties of the parent event type if any
		/// </summary>
		/// <value>size</value>
		public virtual int NativeCount => 0;

		/// <summary>
		/// Attempts to find the native value of the same name including property names of the parent event type if any.  If found,
		/// the method places the value into the out var and returns true.  Otherwise, false.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public virtual bool TryGetNativeEntry(
			string name,
			out KeyValuePair<string, object> value)
		{
			value = default;
			return false;
		}

		/// <summary>
		/// Attempts to find the native value of the same name including property names of the parent event type if any.  Returns
		/// the key-value pair if found, otherwise throws KeyNotFoundException.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		/// <exception cref="KeyNotFoundException"></exception>
		public virtual KeyValuePair<string, object> GetNativeEntry(string name)
		{
			if (!TryGetNativeEntry(name, out var value)) {
				throw new KeyNotFoundException(name);
			}

			return value;
		}

		/// <summary>
		/// Attempts to find the native value of the same name and assigns the value.  If found, the method assigns the
		/// property, otherwise it return false.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public virtual bool TrySetNativeValue(
			string name,
			object value)
		{
			return false;
		}
		
		/// <summary>
		/// Attempts to find the native value of the same name including property names of the parent event type if any.  If found,
		/// the method places the value into the out var and returns true.  Otherwise, false.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public virtual bool TryGetNativeValue(
			string name,
			out object value)
		{
			if (TryGetNativeEntry(name, out var entry)) {
				value = entry.Value;
				return true;
			}

			value = default(object);
			return false;
		}

		/// <summary>
		/// Attempts to find the native value of the same name including property names of the parent event type if any.  Returns
		/// the value if found, otherwise throws KeyNotFoundException.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		/// <exception cref="KeyNotFoundException"></exception>
		public virtual object GetNativeValue(string name)
		{
			if (!TryGetNativeValue(name, out var value)) {
				throw new KeyNotFoundException(name);
			}

			return value;
		}

		/// <summary>
		/// Returns the flag whether the key exists as a pre-declared property of the same name including
		/// property names of the parent event type if any
		/// </summary>
		/// <param name="key">property name</param>
		/// <returns>flag</returns>
		public virtual bool NativeContainsKey(string key)
		{
			return false;
		}

		/// <summary>
		/// Returns the flag whether the value exists as the value element of a pre-declared property.  Not
		/// sure who would use this, but its there, with a default implementation.
		/// </summary>
		/// <param name="value">property value</param>
		/// <returns>flag</returns>
		public virtual bool NativeContainsValue(object value)
		{
			using var enumerator = NativeEnumerable.GetEnumerator();
			while (enumerator.MoveNext()) {
				if (Equals(value, enumerator.Current.Value)) {
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Returns a collection of native keys.
		/// </summary>
		public virtual IEnumerable<string> NativeKeys =>
			NativeEnumerable.Select(_ => _.Key);

		/// <summary>
		/// Returns an enumerable for the native key-value pairs.
		/// </summary>
		/// <value></value>
		public virtual IEnumerable<KeyValuePair<string, object>> NativeEnumerable {
			get => EmptyList<KeyValuePair<string, object>>.Instance;
		}

		/// <summary>
		/// Write the pre-declared properties to the writer
		/// </summary>
		/// <param name="context">serialization context</param>
		/// <throws>IOException for IO exceptions</throws>
		public virtual void NativeWrite(JsonSerializationContext context)
		{
		}

		#endregion

		// ----------------------------------------------------------------------
		public int Count => JsonValues.Count + NativeCount;

		public void Add(
			string key,
			object value)
		{
			throw new NotSupportedException();
		}

		public void Add(KeyValuePair<string, object> item)
		{
			throw new NotSupportedException();
		}

		public bool Remove(KeyValuePair<string, object> item)
		{
			throw new NotSupportedException();
		}

		public bool Remove(string key)
		{
			throw new NotSupportedException();
		}

		public bool ContainsKey(string key)
		{
			return NativeContainsKey(key) || JsonValues.ContainsKey(key);
		}

		public bool ContainsValue(object value)
		{
			return NativeContainsValue(value) || JsonValues.Values.Contains(value);
		}

		public bool Contains(KeyValuePair<string, object> item)
		{
			if (TryGetNativeEntry(item.Key, out var existingKeyValuePair)) {
				return Equals(item.Value, existingKeyValuePair.Value);
			}

			if (JsonValues.TryGetValue(item.Key, out var existingValue)) {
				return Equals(item.Value, existingValue);
			}

			return false;
		}

		public void Clear()
		{
			JsonValues.Clear();
		}

		public ICollection<string> Keys => new JsonEventUnderlyingKeyCollection(this);

		public ICollection<object> Values => new JsonEventUnderlyingValueCollection(this);

		public bool IsReadOnly => true;

		public bool TryGetValue(
			string key,
			out object value)
		{
			return TryGetNativeValue(key, out value) || JsonValues.TryGetValue(key, out value);
		}

		public void CopyTo(
			KeyValuePair<string, object>[] array,
			int arrayIndex)
		{
			var arrayLength = array.Length;

			using (var enumerator = NativeEnumerable.GetEnumerator()) {
				while ((arrayIndex < arrayLength) && enumerator.MoveNext()) {
					array[arrayIndex] = enumerator.Current;
					arrayIndex++;
				}
			}

			using (var enumerator = JsonValues.GetEnumerator()) {
				while ((arrayIndex < arrayLength) && enumerator.MoveNext()) {
					array[arrayIndex] = enumerator.Current;
					arrayIndex++;
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
		{
			using (var enumerator = NativeEnumerable.GetEnumerator()) {
				while (enumerator.MoveNext()) {
					yield return enumerator.Current;
				}
			}

			using (var enumerator = JsonValues.GetEnumerator()) {
				while (enumerator.MoveNext()) {
					yield return enumerator.Current;
				}
			}
		}

		public object this[string key] {
			get {
				if (TryGetNativeValue(key, out var value)) {
					return value;
				}

				return JsonValues[key];
			}
			set {
				if (!TrySetNativeValue(key, value)) {
					JsonValues[key] = value;
				}
			}
		}

		// ----------------------------------------------------------------------

		public void WriteTo(Stream stream)
		{
			var writer = new Utf8JsonWriter(stream);
			var context = new JsonSerializationContext(writer);
			WriteTo(context);
			writer.Flush();
		}

		public void WriteTo(JsonSerializationContext context)
		{
			var writer = context.Writer;
			
			writer.WriteStartObject();
			
			NativeWrite(context);
			
			foreach (var entry in JsonValues) {
				writer.WritePropertyName(entry.Key);
				WriteJsonValue(context, entry.Key, entry.Value);
			}

			writer.WriteEndObject();
		}

		public string ToString(JsonWriterOptions config)
		{
			var stream = new MemoryStream();
			var writer = new Utf8JsonWriter(stream, config);
			var context = new JsonSerializationContext(writer);

			WriteTo(context);

			writer.Flush();
			stream.Flush();

			return Encoding.UTF8.GetString(stream.ToArray());
		}

		public override string ToString()
		{
			return ToString(new JsonWriterOptions());
		}
	}
} // end of namespace
