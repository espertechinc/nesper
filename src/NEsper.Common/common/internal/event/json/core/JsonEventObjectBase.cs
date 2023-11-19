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
        /// <param name="name"></param>
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
        public ICollection<string> PropertyNames => Enumerable
            .Concat(JsonValues.Select(_ => _.Key), NativeKeys.Select(GetNativeKeyName)) 
            .ToList();
            

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
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual bool TryGetNativeEntry(
            int index,
            out KeyValuePair<string, object> value)
        {
            value = default;
            return false;
        }

        /// <summary>
        /// Attempts to find the native value of the same name including property names of the parent event type if any.  Returns
        /// the key-value pair if found, otherwise throws KeyNotFoundException.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public virtual KeyValuePair<string, object> GetNativeEntry(int index)
        {
            if (!TryGetNativeEntry(index, out var value)) {
                throw new KeyNotFoundException();
            }

            return value;
        }

        /// <summary>
        /// Attempts to find the native value of the same name and assigns the value.  If found, the method assigns the
        /// property, otherwise it return false.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual bool TrySetNativeValue(
            int index,
            object value)
        {
            return false;
        }

        /// <summary>
        /// Returns the pre-declared property name including properties names of the parent event type if any
        /// </summary>
        /// <param name="index">the index of the property</param>
        /// <param name="propertyName">the property name (output only)</param>
        /// <returns></returns>

        public virtual bool TryGetNativeKeyName(
            int index,
            out string propertyName)
        {
            propertyName = default;
            return false;
        }

        /// <summary>
        /// Returns the pre-declared property name including properties names of the parent event type if any
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public virtual string GetNativeKeyName(int index)
        {
            if (TryGetNativeKeyName(index, out var propertyName)) {
                return propertyName;
            }
            
            throw new NoSuchElementException();
        }

        /// <summary>
        /// Returns the index for a pre-declared property name including properties names of the parent event type if any
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public virtual bool TryGetNativeKey(string propertyName, out int index)
        {
            index = default;
            return false;
        }

        /// <summary>
        /// Returns the pre-declared property name including properties names of the parent event type if any.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        /// <exception cref="NoSuchElementException"></exception>
        
        public virtual int GetNativeKey(string propertyName)
        {
            if (TryGetNativeKey(propertyName, out var index)) {
                return index;
            }

            throw new NoSuchElementException();
        }
        
        /// <summary>
        /// Attempts to find the native value of the same name including property names of the parent event type if any.  If found,
        /// the method places the value into the out var and returns true.  Otherwise, false.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual bool TryGetNativeValue(
            int index,
            out object value)
        {
            if (TryGetNativeEntry(index, out var entry)) {
                value = entry.Value;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Attempts to find the native value of the same name including property names of the parent event type if any.  Returns
        /// the value if found, otherwise throws KeyNotFoundException.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public virtual object GetNativeValue(int index)
        {
            if (!TryGetNativeValue(index, out var value)) {
                throw new KeyNotFoundException();
            }

            return value;
        }

        /// <summary>
        /// Returns the flag whether the key exists as a pre-declared property of the same index including
        /// indices of the parent event type if any
        /// </summary>
        /// <param name="index">property index</param>
        /// <returns>flag</returns>
        public virtual bool NativeContainsKey(int index)
        {
            // see if there is a native name mapped to this index
            return TryGetNativeKeyName(index, out _);
        }

        /// <summary>
        /// Returns the flag whether the key exists as a pre-declared property of the same name including
        /// property names of the parent event type if any
        /// </summary>
        /// <param name="name">property name</param>
        /// <returns>flag</returns>
        public virtual bool NativeContainsKey(string name)
        {
            // see if there is a native index mapped to this key
            return TryGetNativeKey(name, out _);
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
        public virtual IEnumerable<int> NativeKeys => NativeEnumerable.Select(_ => _.Key);

        /// <summary>
        /// Returns an enumerable for the native key-value pairs.
        /// </summary>
        /// <value></value>
        public virtual IEnumerable<KeyValuePair<int, object>> NativeEnumerable =>
            EmptyList<KeyValuePair<int, object>>.Instance;

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

        public bool ContainsKey(int index)
        {
            return NativeContainsKey(index);
        }
        
        public bool ContainsKey(string key)
        {
            if (TryGetNativeKey(key, out var index)) {
                if (NativeContainsKey(key)) {
                    return true;
                }
            }

            return JsonValues.ContainsKey(key);
        }

        public bool ContainsValue(object value)
        {
            return NativeContainsValue(value) || JsonValues.Values.Contains(value);
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            // if (TryGetNativeEntry(item.Key, out var existingKeyValuePair)) {
            //     return Equals(item.Value, existingKeyValuePair.Value);
            // }

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
            return JsonValues.TryGetValue(key, out value);
            //return TryGetNativeValue(key, out value) || JsonValues.TryGetValue(key, out value);
        }

        public void CopyTo(
            KeyValuePair<string, object>[] array,
            int arrayIndex)
        {
            var arrayLength = array.Length;

            using (var enumerator = NativeEnumerable.GetEnumerator()) {
                while (arrayIndex < arrayLength && enumerator.MoveNext()) {
                    var nkv = enumerator.Current;
                    array[arrayIndex] = new KeyValuePair<string, object>(GetNativeKeyName(nkv.Key), nkv.Value);
                    arrayIndex++;
                }
            }

            using (var enumerator = JsonValues.GetEnumerator()) {
                while (arrayIndex < arrayLength && enumerator.MoveNext()) {
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
                    var nkv = enumerator.Current;
                    yield return new KeyValuePair<string, object>(GetNativeKeyName(nkv.Key), nkv.Value);
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
                if (TryGetNativeKey(key, out var index)) {
                    if (TryGetNativeValue(index, out var value)) {
                        return value;
                    }
                }

                return JsonValues[key];
            }
            set {
                if (TryGetNativeKey(key, out var index)) {
                    if (!TrySetNativeValue(index, value)) {
                        // not a native value
                        JsonValues[key] = value;
                    }
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