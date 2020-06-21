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

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

// ATTRIBUTION NOTICE
// ==================
// MinimalJson is a fast and minimal JSON parser and writer for Java. It's not an object mapper, but a bare-bones library that aims at being:
// - fast: high performance comparable with other state-of-the-art parsers (see below)
// - lightweight: object representation with minimal memory footprint (e.g. no HashMaps)
// - simple: reading, writing and modifying JSON with minimal code (short names, fluent style)
// Minimal JSON can be found at https://github.com/ralfstx/minimal-json.
// Minimal JSON is licensed under the MIT License, see https://github.com/ralfstx/minimal-json/blob/master/LICENSE

namespace com.espertech.esper.common.client.json.minimaljson
{
	/// <summary>
	/// Represents a JSON array, an ordered collection of JSON values.
	/// <para />Elements can be added using the <code>add(...)</code> methods which accept instances of
	/// <seealso cref="JsonValue" />, strings, primitive numbers, and boolean values. To replace an element of an
	/// array, use the <code>set(int, ...)</code> methods.
	/// <para />Elements can be accessed by their index using {@link #get(int)}. This class also supports
	/// iterating over the elements in document order using an {@link #iterator()} or an enhanced for
	/// loop:
	/// for (JsonValue value : jsonArray) {
	/// ...
	/// }
	/// <para />An equivalent <seealso cref="List" /> can be obtained from the method {@link #values()}.
	/// <para />Note that this class is <strong>not thread-safe</strong>. If multiple threads access a
	/// <code>JsonArray</code> instance concurrently, while at least one of these threads modifies the
	/// contents of this array, access to the instance must be synchronized externally. Failure to do so
	/// may lead to an inconsistent state.
	/// <para />This class is <strong>not supposed to be extended</strong> by clients.
	/// </summary>
	// use default serial UID
	public class JsonArray : JsonValue , Iterable<JsonValue> {

	    private readonly IList<JsonValue> values;

	    /// <summary>
	    /// Creates a new empty JsonArray.
	    /// </summary>
	    public JsonArray() {
	        values = new List<JsonValue>();
	    }

	    /// <summary>
	    /// Creates a new JsonArray with the contents of the specified JSON array.
	    /// </summary>
	    /// <param name="array">the JsonArray to get the initial contents from, must not be &lt;code&gt;null&lt;/code&gt;</param>
	    public JsonArray(JsonArray array) {
	        This(array, false);
	    }

	    private JsonArray(JsonArray array, bool unmodifiable) {
	        if (array == null) {
	            throw new NullPointerException("array is null");
	        }
	        if (unmodifiable) {
	            values = Collections.UnmodifiableList(array.values);
	        } else {
	            values = new List<JsonValue>(array.values);
	        }
	    }

	    /// <summary>
	    /// Reads a JSON array from the given reader.
	    /// <para />Characters are read in chunks and buffered internally, therefore wrapping an existing reader in
	    /// an additional <code>BufferedReader</code> does <strong>not</strong> improve reading
	    /// performance.
	    /// </summary>
	    /// <param name="reader">the reader to read the JSON array from</param>
	    /// <returns>the JSON array that has been read</returns>
	    /// <throws>IOException                   if an I/O error occurs in the reader</throws>
	    /// <throws>ParseException                if the input is not valid JSON</throws>
	    /// <throws>UnsupportedOperationException if the input does not contain a JSON array</throws>
	    /// <unknown>@deprecated Use {@link Json#parse(Reader)}{@link JsonValue#asArray() .asArray()} instead</unknown>
	    @Deprecated
	    public static JsonArray ReadFrom(Reader reader) {
	        return JsonValue.ReadFrom(reader).AsArray();
	    }

	    /// <summary>
	    /// Reads a JSON array from the given string.
	    /// </summary>
	    /// <param name="string">the string that contains the JSON array</param>
	    /// <returns>the JSON array that has been read</returns>
	    /// <throws>ParseException                if the input is not valid JSON</throws>
	    /// <throws>UnsupportedOperationException if the input does not contain a JSON array</throws>
	    /// <unknown>@deprecated Use {@link Json#parse(String)}{@link JsonValue#asArray() .asArray()} instead</unknown>
	    @Deprecated
	    public static JsonArray ReadFrom(string string) {
	        return JsonValue.ReadFrom(string).AsArray();
	    }

	    /// <summary>
	    /// Returns an unmodifiable wrapper for the specified JsonArray. This method allows to provide
	    /// read-only access to a JsonArray.
	    /// <para />The returned JsonArray is backed by the given array and reflects subsequent changes. Attempts
	    /// to modify the returned JsonArray result in an <code>UnsupportedOperationException</code>.
	    /// </summary>
	    /// <param name="array">the JsonArray for which an unmodifiable JsonArray is to be returned</param>
	    /// <returns>an unmodifiable view of the specified JsonArray</returns>
	    public static JsonArray UnmodifiableArray(JsonArray array) {
	        return new JsonArray(array, true);
	    }

	    /// <summary>
	    /// Appends the JSON representation of the specified <code>int</code> value to the end of this
	    /// array.
	    /// </summary>
	    /// <param name="value">the value to add to the array</param>
	    /// <returns>the array itself, to enable method chaining</returns>
	    public JsonArray Add(int value) {
	        values.Add(Json.Value(value));
	        return this;
	    }

	    /// <summary>
	    /// Appends the JSON representation of the specified <code>long</code> value to the end of this
	    /// array.
	    /// </summary>
	    /// <param name="value">the value to add to the array</param>
	    /// <returns>the array itself, to enable method chaining</returns>
	    public JsonArray Add(long value) {
	        values.Add(Json.Value(value));
	        return this;
	    }

	    /// <summary>
	    /// Appends the JSON representation of the specified <code>float</code> value to the end of this
	    /// array.
	    /// </summary>
	    /// <param name="value">the value to add to the array</param>
	    /// <returns>the array itself, to enable method chaining</returns>
	    public JsonArray Add(float value) {
	        values.Add(Json.Value(value));
	        return this;
	    }

	    /// <summary>
	    /// Appends the JSON representation of the specified <code>double</code> value to the end of this
	    /// array.
	    /// </summary>
	    /// <param name="value">the value to add to the array</param>
	    /// <returns>the array itself, to enable method chaining</returns>
	    public JsonArray Add(double value) {
	        values.Add(Json.Value(value));
	        return this;
	    }

	    /// <summary>
	    /// Appends the JSON representation of the specified <code>boolean</code> value to the end of this
	    /// array.
	    /// </summary>
	    /// <param name="value">the value to add to the array</param>
	    /// <returns>the array itself, to enable method chaining</returns>
	    public JsonArray Add(bool value) {
	        values.Add(Json.Value(value));
	        return this;
	    }

	    /// <summary>
	    /// Appends the JSON representation of the specified string to the end of this array.
	    /// </summary>
	    /// <param name="value">the string to add to the array</param>
	    /// <returns>the array itself, to enable method chaining</returns>
	    public JsonArray Add(string value) {
	        values.Add(Json.Value(value));
	        return this;
	    }

	    /// <summary>
	    /// Appends the specified JSON value to the end of this array.
	    /// </summary>
	    /// <param name="value">the JsonValue to add to the array, must not be &lt;code&gt;null&lt;/code&gt;</param>
	    /// <returns>the array itself, to enable method chaining</returns>
	    public JsonArray Add(JsonValue value) {
	        if (value == null) {
	            throw new NullPointerException("value is null");
	        }
	        values.Add(value);
	        return this;
	    }

	    /// <summary>
	    /// Replaces the element at the specified position in this array with the JSON representation of
	    /// the specified <code>int</code> value.
	    /// </summary>
	    /// <param name="index">the index of the array element to replace</param>
	    /// <param name="value">the value to be stored at the specified array position</param>
	    /// <returns>the array itself, to enable method chaining</returns>
	    /// <throws>IndexOutOfBoundsException if the index is out of range, i.e. &lt;code&gt;index &amp;lt; 0&lt;/code&gt; or<code>index &gt;= size</code></throws>
	    public JsonArray Set(int index, int value) {
	        values.Set(index, Json.Value(value));
	        return this;
	    }

	    /// <summary>
	    /// Replaces the element at the specified position in this array with the JSON representation of
	    /// the specified <code>long</code> value.
	    /// </summary>
	    /// <param name="index">the index of the array element to replace</param>
	    /// <param name="value">the value to be stored at the specified array position</param>
	    /// <returns>the array itself, to enable method chaining</returns>
	    /// <throws>IndexOutOfBoundsException if the index is out of range, i.e. &lt;code&gt;index &amp;lt; 0&lt;/code&gt; or<code>index &gt;= size</code></throws>
	    public JsonArray Set(int index, long value) {
	        values.Set(index, Json.Value(value));
	        return this;
	    }

	    /// <summary>
	    /// Replaces the element at the specified position in this array with the JSON representation of
	    /// the specified <code>float</code> value.
	    /// </summary>
	    /// <param name="index">the index of the array element to replace</param>
	    /// <param name="value">the value to be stored at the specified array position</param>
	    /// <returns>the array itself, to enable method chaining</returns>
	    /// <throws>IndexOutOfBoundsException if the index is out of range, i.e. &lt;code&gt;index &amp;lt; 0&lt;/code&gt; or<code>index &gt;= size</code></throws>
	    public JsonArray Set(int index, float value) {
	        values.Set(index, Json.Value(value));
	        return this;
	    }

	    /// <summary>
	    /// Replaces the element at the specified position in this array with the JSON representation of
	    /// the specified <code>double</code> value.
	    /// </summary>
	    /// <param name="index">the index of the array element to replace</param>
	    /// <param name="value">the value to be stored at the specified array position</param>
	    /// <returns>the array itself, to enable method chaining</returns>
	    /// <throws>IndexOutOfBoundsException if the index is out of range, i.e. &lt;code&gt;index &amp;lt; 0&lt;/code&gt; or<code>index &gt;= size</code></throws>
	    public JsonArray Set(int index, double value) {
	        values.Set(index, Json.Value(value));
	        return this;
	    }

	    /// <summary>
	    /// Replaces the element at the specified position in this array with the JSON representation of
	    /// the specified <code>boolean</code> value.
	    /// </summary>
	    /// <param name="index">the index of the array element to replace</param>
	    /// <param name="value">the value to be stored at the specified array position</param>
	    /// <returns>the array itself, to enable method chaining</returns>
	    /// <throws>IndexOutOfBoundsException if the index is out of range, i.e. &lt;code&gt;index &amp;lt; 0&lt;/code&gt; or<code>index &gt;= size</code></throws>
	    public JsonArray Set(int index, bool value) {
	        values.Set(index, Json.Value(value));
	        return this;
	    }

	    /// <summary>
	    /// Replaces the element at the specified position in this array with the JSON representation of
	    /// the specified string.
	    /// </summary>
	    /// <param name="index">the index of the array element to replace</param>
	    /// <param name="value">the string to be stored at the specified array position</param>
	    /// <returns>the array itself, to enable method chaining</returns>
	    /// <throws>IndexOutOfBoundsException if the index is out of range, i.e. &lt;code&gt;index &amp;lt; 0&lt;/code&gt; or<code>index &gt;= size</code></throws>
	    public JsonArray Set(int index, string value) {
	        values.Set(index, Json.Value(value));
	        return this;
	    }

	    /// <summary>
	    /// Replaces the element at the specified position in this array with the specified JSON value.
	    /// </summary>
	    /// <param name="index">the index of the array element to replace</param>
	    /// <param name="value">the value to be stored at the specified array position, must not be &lt;code&gt;null&lt;/code&gt;</param>
	    /// <returns>the array itself, to enable method chaining</returns>
	    /// <throws>IndexOutOfBoundsException if the index is out of range, i.e. &lt;code&gt;index &amp;lt; 0&lt;/code&gt; or<code>index &gt;= size</code></throws>
	    public JsonArray Set(int index, JsonValue value) {
	        if (value == null) {
	            throw new NullPointerException("value is null");
	        }
	        values.Set(index, value);
	        return this;
	    }

	    /// <summary>
	    /// Removes the element at the specified index from this array.
	    /// </summary>
	    /// <param name="index">the index of the element to remove</param>
	    /// <returns>the array itself, to enable method chaining</returns>
	    /// <throws>IndexOutOfBoundsException if the index is out of range, i.e. &lt;code&gt;index &amp;lt; 0&lt;/code&gt; or<code>index &gt;= size</code></throws>
	    public JsonArray Remove(int index) {
	        values.Remove(index);
	        return this;
	    }

	    /// <summary>
	    /// Returns the number of elements in this array.
	    /// </summary>
	    /// <returns>the number of elements in this array</returns>
	    public int Size() {
	        return values.Count;
	    }

	    /// <summary>
	    /// Returns <code>true</code> if this array contains no elements.
	    /// </summary>
	    /// <returns>&lt;code&gt;true&lt;/code&gt; if this array contains no elements</returns>
	    public bool IsEmpty() {
	        return values.IsEmpty();
	    }

	    /// <summary>
	    /// Returns the value of the element at the specified position in this array.
	    /// </summary>
	    /// <param name="index">the index of the array element to return</param>
	    /// <returns>the value of the element at the specified position</returns>
	    /// <throws>IndexOutOfBoundsException if the index is out of range, i.e. &lt;code&gt;index &amp;lt; 0&lt;/code&gt; or<code>index &gt;= size</code></throws>
	    public JsonValue Get(int index) {
	        return values.Get(index);
	    }

	    /// <summary>
	    /// Returns a list of the values in this array in document order. The returned list is backed by
	    /// this array and will reflect subsequent changes. It cannot be used to modify this array.
	    /// Attempts to modify the returned list will result in an exception.
	    /// </summary>
	    /// <returns>a list of the values in this array</returns>
	    public IList<JsonValue> Values() {
	        return Collections.UnmodifiableList(values);
	    }

	    /// <summary>
	    /// Returns an iterator over the values of this array in document order. The returned iterator
	    /// cannot be used to modify this array.
	    /// </summary>
	    /// <returns>an iterator over the values of this array</returns>
	    public IEnumerator<JsonValue> Iterator() {
	        IEnumerator<JsonValue> iterator = values.Iterator();
	        return new IEnProxyIterator<JsonValue>() {

	            ProcHasNext = () =>  {
	                return iterator.HasNext;
	            },

	            ProcNext = () =>  {
	                return iterator.Next();
	            },

	            ProcRemove = () =>  {
	                throw new UnsupportedOperationException();
	            },
	        };
	    }

	    @Override
	    void Write(JsonWriter writer) {
	        writer.WriteArrayOpen();
	        IEnumerator<JsonValue> iterator = Iterator();
	        if (iterator.HasNext) {
	            iterator.Next().Write(writer);
	            while (iterator.HasNext) {
	                writer.WriteArraySeparator();
	                iterator.Next().Write(writer);
	            }
	        }
	        writer.WriteArrayClose();
	    }

	    @Override
	    public bool IsArray() {
	        return true;
	    }

	    @Override
	    public JsonArray AsArray() {
	        return this;
	    }

	    @Override
	    public int HashCode() {
	        return values.HasHCode;
	    }

	    /// <summary>
	    /// Indicates whether a given object is "equal to" this JsonArray. An object is considered equal
	    /// if it is also a <code>JsonArray</code> and both arrays contain the same list of values.
	    /// <para />If two JsonArrays are equal, they will also produce the same JSON output.
	    /// </summary>
	    /// <param name="object">the object to be compared with this JsonArray</param>
	    /// <returns>&lt;tt&gt;true&lt;/tt&gt; if the specified object is equal to this JsonArray, &lt;code&gt;false&lt;/code&gt;otherwise
	    /// </returns>
	    @Override
	    public bool Equals(object object) {
	        if (this == object) {
	            return true;
	        }
	        if (object == null) {
	            return false;
	        }
	        if (GetType() != object.GetType()) {
	            return false;
	        }
	        JsonArray other = (JsonArray) object;
	        return values.Equals(other.values);
	    }

	}
} // end of namespace
