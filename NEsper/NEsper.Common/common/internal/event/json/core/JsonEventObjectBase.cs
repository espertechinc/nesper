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
using System.Text;
using System.Text.Json;

using com.espertech.esper.common.client.json.util;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.@event.json.write.JsonWriteUtil;

namespace com.espertech.esper.common.@internal.@event.json.core
{
	public abstract class JsonEventObjectBase : JsonEventObject {
	    /// <summary>
	    /// Add a dynamic property value that the json parser encounters.
	    /// Dynamic property values are not predefined and are catch-all in nature.
	    /// </summary>
	    /// <param name="name">name</param>
	    /// <param name="value">value</param>
	    public abstract void AddJsonValue(string name, object value);

	    /// <summary>
	    /// Returns the dynamic property values (the non-predefined values)
	    /// </summary>
	    /// <value>map</value>
	    public abstract IDictionary<string, object> JsonValues { get; }

	    #region Native
	    
	    /// <summary>
	    /// Returns the total number of pre-declared properties available including properties of the parent event type if any
	    /// </summary>
	    /// <value>size</value>
	    public abstract int NativeCount { get; }

	    /// <summary>
	    /// Attempts to find the native value of the same name including property names of the parent event type if any.  If found,
	    /// the method places the value into the out var and returns true.  Otherwise, false.
	    /// </summary>
	    /// <param name="name"></param>
	    /// <param name="value"></param>
	    /// <returns></returns>
	    public abstract bool TryGetNativeEntry(
		    string name,
		    out KeyValuePair<string, object> value);

	    /// <summary>
	    /// Attempts to find the native value of the same name including property names of the parent event type if any.  If found,
	    /// the method places the value into the out var and returns true.  Otherwise, false.
	    /// </summary>
	    /// <param name="name"></param>
	    /// <param name="value"></param>
	    /// <returns></returns>
	    public abstract bool TryGetNativeValue(
		    string name,
		    out object value);

	    /// <summary>
	    /// Returns the flag whether the key exists as a pre-declared property of the same name including
	    /// property names of the parent event type if any
	    /// </summary>
	    /// <param name="key">property name</param>
	    /// <returns>flag</returns>
	    public abstract bool NativeContainsKey(string key);

	    /// <summary>
	    /// Returns the flag whether the value exists as the value element of a pre-declared property.  Not
	    /// sure who would use this, but its there, with a default implementation.
	    /// </summary>
	    /// <param name="value">property value</param>
	    /// <returns>flag</returns>
	    public virtual bool NativeContainsValue(object value)
	    {
		    using var enumerator = NativeEnumerable().GetEnumerator();
		    while (enumerator.MoveNext()) {
			    if (Equals(value, enumerator.Current.Value)) {
				    return true;
			    }
		    }

		    return false;
	    }

	    /// <summary>
	    /// Returns an enumerable for the native keys.
	    /// </summary>
	    /// <returns></returns>
	    public abstract IEnumerable<KeyValuePair<string, object>> NativeEnumerable(); 
	    
	    /// <summary>
	    /// Write the pre-declared properties to the writer
	    /// </summary>
	    /// <param name="writer">writer</param>
	    /// <throws>IOException for IO exceptions</throws>
	    protected abstract void NativeWrite(Utf8JsonWriter writer);
	    
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
		    throw new UnsupportedOperationException();
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

		    using(var enumerator = NativeEnumerable().GetEnumerator()) {
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
		    using(var enumerator = NativeEnumerable().GetEnumerator()) {
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
		    set => throw new NotSupportedException();
	    }

	    // ----------------------------------------------------------------------

	    public void WriteTo(Stream stream)
	    {
		    var writer = new Utf8JsonWriter(stream);
		    WriteTo(writer);
		    writer.Flush();
	    }

	    public void WriteTo(Utf8JsonWriter writer) {
		    writer.WriteStartObject();
	        NativeWrite(writer);
	        foreach (var entry in JsonValues) {
		        writer.WritePropertyName(entry.Key);
	            WriteJsonValue(writer, entry.Key, entry.Value);
	        }
	        writer.WriteEndObject();
	    }
	    
	    public string ToString(JsonWriterOptions config)
	    {
		    var stream = new MemoryStream();
		    var writer = new Utf8JsonWriter(stream, config);

		    WriteTo(writer);
		    
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
