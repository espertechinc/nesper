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

using com.espertech.esper.common.client.json.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.@event.json.write.JsonWriteUtil; //writeJsonValue;

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

	    /// <summary>
	    /// Returns the total number of pre-declared properties available including properties of the parent event type if any
	    /// </summary>
	    /// <value>size</value>
	    public abstract int NativeSize { get; }

	    /// <summary>
	    /// Returns the name-value pre-declared property including properties of the parent event type if any
	    /// </summary>
	    /// <param name="num">index number of the property</param>
	    /// <returns>entry</returns>
	    /// <throws>java.util.NoSuchElementException for invalid index</throws>
	    public abstract KeyValuePair<string, object> GetNativeEntry(int num);

	    /// <summary>
	    /// Returns the pre-declared property name including properties names of the parent event type if any
	    /// </summary>
	    /// <param name="num">index number of the property</param>
	    /// <returns>name</returns>
	    /// <throws>java.util.NoSuchElementException for invalid index</throws>
	    public abstract string GetNativeKey(int num);

	    /// <summary>
	    /// Returns the value of a pre-declared property including property values of the parent event type if any
	    /// </summary>
	    /// <param name="num">index number of the property</param>
	    /// <returns>value</returns>
	    /// <throws>java.util.NoSuchElementException for invalid index</throws>
	    public abstract object GetNativeValue(int num);

	    /// <summary>
	    /// Returns the index number of a a pre-declared property of the same name including property names of the parent event type if any
	    /// </summary>
	    /// <param name="name">property name</param>
	    /// <returns>index starting at zero, ending at native-size minus 1; Returns -1 for non-existing property name</returns>
	    public abstract int GetNativeNum(string name);

	    /// <summary>
	    /// Returns the flag whether the key exists as a pre-declared property of the same name including property names of the parent event type if any
	    /// </summary>
	    /// <param name="key">property name</param>
	    /// <returns>flag</returns>
	    public abstract bool NativeContainsKey(object key);

	    /// <summary>
	    /// Write the pre-declared properties to the writer
	    /// </summary>
	    /// <param name="writer">writer</param>
	    /// <throws>IOException for IO exceptions</throws>
	    public abstract void NativeWrite(JsonWriter writer) ;

	    public void WriteTo(Writer writer, WriterConfig config) {
	        WritingBuffer buffer = new WritingBuffer(writer, 128);
	        Write(config.CreateWriter(buffer));
	        buffer.Flush();
	    }

	    public void Write(JsonWriter writer) {
	        writer.WriteObjectOpen();
	        NativeWrite(writer);
	        bool first = NativeSize == 0;
	        foreach (var entry in JsonValues) {
	            if (!first) {
	                writer.WriteObjectSeparator();
	            }
	            first = false;
	            writer.WriteMemberName(entry.Key);
	            writer.WriteMemberSeparator();
	            WriteJsonValue(writer, entry.Key, entry.Value);
	        }
	        writer.WriteObjectClose();
	    }

	    public int Size() {
	        return JsonValues.Count + NativeSize;
	    }

	    public ISet<Entry<string, object>> EntrySet() {
	        return new JsonEventUnderlyingEntrySet(this);
	    }

	    public bool IsEmpty() {
	        return NativeSize == 0 && JsonValues.IsEmpty();
	    }

	    public ISet<string> KeySet() {
	        return new JsonEventUnderlyingKeySet(this);
	    }

	    public bool ContainsKey(object key) {
	        return NativeContainsKey(key) || JsonValues.ContainsKey(key);
	    }

	    public object Get(object key) {
	        if (key == null || !(key is string)) {
	            return JsonValues.Get(key);
	        }
	        int num = GetNativeNum((string) key);
	        if (num == -1) {
	            return JsonValues.Get(key);
	        }
	        return GetNativeValue(num);
	    }

	    public bool ContainsValue(object value) {
	        if (value == null) {
	            for (int i = 0; i < NativeSize; i++) {
	                if (GetNativeValue(i) == null) {
	                    return true;
	                }
	            }
	        } else {
	            for (int i = 0; i < NativeSize; i++) {
	                if (value.Equals(GetNativeValue(i))) {
	                    return true;
	                }
	            }
	        }
	        return JsonValues.ContainsValue(value);
	    }

	    public ICollection<object> Values() {
	        return new JsonEventUnderlyingValueCollection(this, JsonValues.Values());
	    }

	    public object Put(string key, object value) {
	        throw new UnsupportedOperationException();
	    }

	    public object Remove(object key) {
	        throw new UnsupportedOperationException();
	    }

	    public void PutAll<T>(IDictionary<? extends string, ?> m) {
	        throw new UnsupportedOperationException();
	    }

	    public void Clear() {
	        throw new UnsupportedOperationException();
	    }

	    public bool Remove(object key, object value) {
	        throw new UnsupportedOperationException();
	    }

	    public void ReplaceAll(Action<string, object> function)
	    {
	        throw new UnsupportedOperationException();
	    }

	    public object PutIfAbsent(string key, object value) {
	        throw new UnsupportedOperationException();
	    }

	    public bool Replace(string key, object oldValue, object newValue) {
	        throw new UnsupportedOperationException();
	    }

	    public object Replace(string key, object value) {
	        throw new UnsupportedOperationException();
	    }

	    public string ToString(WriterConfig config) {
	        StringWriter writer = new StringWriter();
	        try {
	            WriteTo(writer, config);
	        } catch (IOException exception) {
	            // StringWriter does not throw IOExceptions
	            throw new RuntimeException(exception);
	        }
	        return writer.ToString();
	    }

	    public override string ToString() {
	        return ToString(WriterConfig.MINIMAL);
	    }
	}

} // end of namespace
