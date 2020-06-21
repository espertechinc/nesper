///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
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
	/// Represents a JSON value. This can be a JSON <strong>object</strong>, an <strong> array</strong>,
	/// a <strong>number</strong>, a <strong>string</strong>, or one of the literals
	/// <strong>true</strong>, <strong>false</strong>, and <strong>null</strong>.
	/// <para />The literals <strong>true</strong>, <strong>false</strong>, and <strong>null</strong> are
	/// represented by the constants <seealso cref="Json#TRUE" />, <seealso cref="Json#FALSE" />, and <seealso cref="Json#NULL" />.
	/// <para />JSON <strong>objects</strong> and <strong>arrays</strong> are represented by the subtypes
	/// <seealso cref="JsonObject" /> and <seealso cref="JsonArray" />. Instances of these types can be created using the
	/// public constructors of these classes.
	/// <para />Instances that represent JSON <strong>numbers</strong>, <strong>strings</strong> and
	/// <strong>boolean</strong> values can be created using the static factory methods
	/// {@link Json#value(String)}, {@link Json#value(long)}, {@link Json#value(double)}, etc.
	/// <para />In order to find out whether an instance of this class is of a certain type, the methods
	/// {@link #isObject()}, {@link #isArray()}, {@link #isString()}, {@link #isNumber()} etc. can be
	/// used.
	/// <para />If the type of a JSON value is known, the methods {@link #asObject()}, {@link #asArray()},
	/// {@link #asString()}, {@link #asInt()}, etc. can be used to get this value directly in the
	/// appropriate target type.
	/// <para />This class is <strong>not supposed to be extended</strong> by clients.
	/// </summary>
	// use default serial UID
	public abstract class JsonValue : Serializable {

	    /// <summary>
	    /// Represents the JSON literal <code>true</code>.
	    /// </summary>
	    /// <unknown>@deprecated Use &lt;code&gt;Json.TRUE&lt;/code&gt; instead</unknown>
	    @Deprecated
	    public static readonly JsonValue TRUE = new JsonLiteral("true");

	    /// <summary>
	    /// Represents the JSON literal <code>false</code>.
	    /// </summary>
	    /// <unknown>@deprecated Use &lt;code&gt;Json.FALSE&lt;/code&gt; instead</unknown>
	    @Deprecated
	    public static readonly JsonValue FALSE = new JsonLiteral("false");

	    /// <summary>
	    /// Represents the JSON literal <code>null</code>.
	    /// </summary>
	    /// <unknown>@deprecated Use &lt;code&gt;Json.NULL&lt;/code&gt; instead</unknown>
	    @Deprecated
	    public static readonly JsonValue NULL = new JsonLiteral("null");

	    JsonValue() {
	        // prevent subclasses outside of this package
	    }

	    /// <summary>
	    /// Reads a JSON value from the given reader.
	    /// <para />Characters are read in chunks and buffered internally, therefore wrapping an existing reader in
	    /// an additional <code>BufferedReader</code> does <strong>not</strong> improve reading
	    /// performance.
	    /// </summary>
	    /// <param name="reader">the reader to read the JSON value from</param>
	    /// <returns>the JSON value that has been read</returns>
	    /// <throws>IOException    if an I/O error occurs in the reader</throws>
	    /// <throws>ParseException if the input is not valid JSON</throws>
	    /// <unknown>@deprecated Use {@link Json#parse(Reader)} instead</unknown>
	    @Deprecated
	    public static JsonValue ReadFrom(Reader reader) {
	        return Json.Parse(reader);
	    }

	    /// <summary>
	    /// Reads a JSON value from the given string.
	    /// </summary>
	    /// <param name="text">the string that contains the JSON value</param>
	    /// <returns>the JSON value that has been read</returns>
	    /// <throws>ParseException if the input is not valid JSON</throws>
	    /// <unknown>@deprecated Use {@link Json#parse(String)} instead</unknown>
	    @Deprecated
	    public static JsonValue ReadFrom(string text) {
	        return Json.Parse(text);
	    }

	    /// <summary>
	    /// Returns a JsonValue instance that represents the given <code>int</code> value.
	    /// </summary>
	    /// <param name="value">the value to get a JSON representation for</param>
	    /// <returns>a JSON value that represents the given value</returns>
	    /// <unknown>@deprecated Use &lt;code&gt;Json.value()&lt;/code&gt; instead</unknown>
	    @Deprecated
	    public static JsonValue ValueOf(int value) {
	        return Json.Value(value);
	    }

	    /// <summary>
	    /// Returns a JsonValue instance that represents the given <code>long</code> value.
	    /// </summary>
	    /// <param name="value">the value to get a JSON representation for</param>
	    /// <returns>a JSON value that represents the given value</returns>
	    /// <unknown>@deprecated Use &lt;code&gt;Json.value()&lt;/code&gt; instead</unknown>
	    @Deprecated
	    public static JsonValue ValueOf(long value) {
	        return Json.Value(value);
	    }

	    /// <summary>
	    /// Returns a JsonValue instance that represents the given <code>float</code> value.
	    /// </summary>
	    /// <param name="value">the value to get a JSON representation for</param>
	    /// <returns>a JSON value that represents the given value</returns>
	    /// <unknown>@deprecated Use &lt;code&gt;Json.value()&lt;/code&gt; instead</unknown>
	    @Deprecated
	    public static JsonValue ValueOf(float value) {
	        return Json.Value(value);
	    }

	    /// <summary>
	    /// Returns a JsonValue instance that represents the given <code>double</code> value.
	    /// </summary>
	    /// <param name="value">the value to get a JSON representation for</param>
	    /// <returns>a JSON value that represents the given value</returns>
	    /// <unknown>@deprecated Use &lt;code&gt;Json.value()&lt;/code&gt; instead</unknown>
	    @Deprecated
	    public static JsonValue ValueOf(double value) {
	        return Json.Value(value);
	    }

	    /// <summary>
	    /// Returns a JsonValue instance that represents the given string.
	    /// </summary>
	    /// <param name="string">the string to get a JSON representation for</param>
	    /// <returns>a JSON value that represents the given string</returns>
	    /// <unknown>@deprecated Use &lt;code&gt;Json.value()&lt;/code&gt; instead</unknown>
	    @Deprecated
	    public static JsonValue ValueOf(string string) {
	        return Json.Value(string);
	    }

	    /// <summary>
	    /// Returns a JsonValue instance that represents the given <code>boolean</code> value.
	    /// </summary>
	    /// <param name="value">the value to get a JSON representation for</param>
	    /// <returns>a JSON value that represents the given value</returns>
	    /// <unknown>@deprecated Use &lt;code&gt;Json.value()&lt;/code&gt; instead</unknown>
	    @Deprecated
	    public static JsonValue ValueOf(bool value) {
	        return Json.Value(value);
	    }

	    /// <summary>
	    /// Detects whether this value represents a JSON object. If this is the case, this value is an
	    /// instance of <seealso cref="JsonObject" />.
	    /// </summary>
	    /// <returns>&lt;code&gt;true&lt;/code&gt; if this value is an instance of JsonObject</returns>
	    public bool IsObject() {
	        return false;
	    }

	    /// <summary>
	    /// Detects whether this value represents a JSON array. If this is the case, this value is an
	    /// instance of <seealso cref="JsonArray" />.
	    /// </summary>
	    /// <returns>&lt;code&gt;true&lt;/code&gt; if this value is an instance of JsonArray</returns>
	    public bool IsArray() {
	        return false;
	    }

	    /// <summary>
	    /// Detects whether this value represents a JSON number.
	    /// </summary>
	    /// <returns>&lt;code&gt;true&lt;/code&gt; if this value represents a JSON number</returns>
	    public bool IsNumber() {
	        return false;
	    }

	    /// <summary>
	    /// Detects whether this value represents a JSON string.
	    /// </summary>
	    /// <returns>&lt;code&gt;true&lt;/code&gt; if this value represents a JSON string</returns>
	    public bool IsString() {
	        return false;
	    }

	    /// <summary>
	    /// Detects whether this value represents a boolean value.
	    /// </summary>
	    /// <returns>&lt;code&gt;true&lt;/code&gt; if this value represents either the JSON literal &lt;code&gt;true&lt;/code&gt; or<code>false</code></returns>
	    public bool IsBoolean() {
	        return false;
	    }

	    /// <summary>
	    /// Detects whether this value represents the JSON literal <code>true</code>.
	    /// </summary>
	    /// <returns>&lt;code&gt;true&lt;/code&gt; if this value represents the JSON literal &lt;code&gt;true&lt;/code&gt;</returns>
	    public bool IsTrue() {
	        return false;
	    }

	    /// <summary>
	    /// Detects whether this value represents the JSON literal <code>false</code>.
	    /// </summary>
	    /// <returns>&lt;code&gt;true&lt;/code&gt; if this value represents the JSON literal &lt;code&gt;false&lt;/code&gt;</returns>
	    public bool IsFalse() {
	        return false;
	    }

	    /// <summary>
	    /// Detects whether this value represents the JSON literal <code>null</code>.
	    /// </summary>
	    /// <returns>&lt;code&gt;true&lt;/code&gt; if this value represents the JSON literal &lt;code&gt;null&lt;/code&gt;</returns>
	    public bool IsNull() {
	        return false;
	    }

	    /// <summary>
	    /// Returns this JSON value as <seealso cref="JsonObject" />, assuming that this value represents a JSON
	    /// object. If this is not the case, an exception is thrown.
	    /// </summary>
	    /// <returns>a JSONObject for this value</returns>
	    /// <throws>UnsupportedOperationException if this value is not a JSON object</throws>
	    public JsonObject AsObject() {
	        throw new UnsupportedOperationException("Not an object: " + ToString());
	    }

	    /// <summary>
	    /// Returns this JSON value as <seealso cref="JsonArray" />, assuming that this value represents a JSON array.
	    /// If this is not the case, an exception is thrown.
	    /// </summary>
	    /// <returns>a JSONArray for this value</returns>
	    /// <throws>UnsupportedOperationException if this value is not a JSON array</throws>
	    public JsonArray AsArray() {
	        throw new UnsupportedOperationException("Not an array: " + ToString());
	    }

	    /// <summary>
	    /// Returns this JSON value as an <code>int</code> value, assuming that this value represents a
	    /// JSON number that can be interpreted as Java <code>int</code>. If this is not the case, an
	    /// exception is thrown.
	    /// <para />To be interpreted as Java <code>int</code>, the JSON number must neither contain an exponent
	    /// nor a fraction part. Moreover, the number must be in the <code>Integer</code> range.
	    /// </summary>
	    /// <returns>this value as &lt;code&gt;int&lt;/code&gt;</returns>
	    /// <throws>UnsupportedOperationException if this value is not a JSON number</throws>
	    /// <throws>NumberFormatException         if this JSON number can not be interpreted as &lt;code&gt;int&lt;/code&gt; value</throws>
	    public int AsInt() {
	        throw new UnsupportedOperationException("Not a number: " + ToString());
	    }

	    /// <summary>
	    /// Returns this JSON value as a <code>long</code> value, assuming that this value represents a
	    /// JSON number that can be interpreted as Java <code>long</code>. If this is not the case, an
	    /// exception is thrown.
	    /// <para />To be interpreted as Java <code>long</code>, the JSON number must neither contain an exponent
	    /// nor a fraction part. Moreover, the number must be in the <code>Long</code> range.
	    /// </summary>
	    /// <returns>this value as &lt;code&gt;long&lt;/code&gt;</returns>
	    /// <throws>UnsupportedOperationException if this value is not a JSON number</throws>
	    /// <throws>NumberFormatException         if this JSON number can not be interpreted as &lt;code&gt;long&lt;/code&gt; value</throws>
	    public long AsLong() {
	        throw new UnsupportedOperationException("Not a number: " + ToString());
	    }

	    /// <summary>
	    /// Returns this JSON value as a <code>float</code> value, assuming that this value represents a
	    /// JSON number. If this is not the case, an exception is thrown.
	    /// <para />If the JSON number is out of the <code>Float</code> range, <seealso cref="Float#POSITIVE_INFINITY" /> or
	    /// <seealso cref="Float#NEGATIVE_INFINITY" /> is returned.
	    /// </summary>
	    /// <returns>this value as &lt;code&gt;float&lt;/code&gt;</returns>
	    /// <throws>UnsupportedOperationException if this value is not a JSON number</throws>
	    public float AsFloat() {
	        throw new UnsupportedOperationException("Not a number: " + ToString());
	    }

	    /// <summary>
	    /// Returns this JSON value as a <code>double</code> value, assuming that this value represents a
	    /// JSON number. If this is not the case, an exception is thrown.
	    /// <para />If the JSON number is out of the <code>Double</code> range, <seealso cref="Double#POSITIVE_INFINITY" /> or
	    /// <seealso cref="Double#NEGATIVE_INFINITY" /> is returned.
	    /// </summary>
	    /// <returns>this value as &lt;code&gt;double&lt;/code&gt;</returns>
	    /// <throws>UnsupportedOperationException if this value is not a JSON number</throws>
	    public double AsDouble() {
	        throw new UnsupportedOperationException("Not a number: " + ToString());
	    }

	    /// <summary>
	    /// Returns this JSON value as String, assuming that this value represents a JSON string. If this
	    /// is not the case, an exception is thrown.
	    /// </summary>
	    /// <returns>the string represented by this value</returns>
	    /// <throws>UnsupportedOperationException if this value is not a JSON string</throws>
	    public string AsString() {
	        throw new UnsupportedOperationException("Not a string: " + ToString());
	    }

	    /// <summary>
	    /// Returns this JSON value as a <code>boolean</code> value, assuming that this value is either
	    /// <code>true</code> or <code>false</code>. If this is not the case, an exception is thrown.
	    /// </summary>
	    /// <returns>this value as &lt;code&gt;boolean&lt;/code&gt;</returns>
	    /// <throws>UnsupportedOperationException if this value is neither &lt;code&gt;true&lt;/code&gt; or &lt;code&gt;false&lt;/code&gt;</throws>
	    public bool AsBoolean() {
	        throw new UnsupportedOperationException("Not a boolean: " + ToString());
	    }

	    /// <summary>
	    /// Writes the JSON representation of this value to the given writer in its minimal form, without
	    /// any additional whitespace.
	    /// <para />Writing performance can be improved by using a {@link java.io.BufferedWriter BufferedWriter}.
	    /// </summary>
	    /// <param name="writer">the writer to write this value to</param>
	    /// <throws>IOException if an I/O error occurs in the writer</throws>
	    public void WriteTo(Writer writer) {
	        WriteTo(writer, WriterConfig.MINIMAL);
	    }

	    /// <summary>
	    /// Writes the JSON representation of this value to the given writer using the given formatting.
	    /// <para />Writing performance can be improved by using a {@link java.io.BufferedWriter BufferedWriter}.
	    /// </summary>
	    /// <param name="writer">the writer to write this value to</param>
	    /// <param name="config">a configuration that controls the formatting or &lt;code&gt;null&lt;/code&gt; for the minimal form</param>
	    /// <throws>IOException if an I/O error occurs in the writer</throws>
	    public void WriteTo(Writer writer, WriterConfig config) {
	        if (writer == null) {
	            throw new NullPointerException("writer is null");
	        }
	        if (config == null) {
	            throw new NullPointerException("config is null");
	        }
	        WritingBuffer buffer = new WritingBuffer(writer, 128);
	        Write(config.CreateWriter(buffer));
	        buffer.Flush();
	    }

	    /// <summary>
	    /// Returns the JSON string for this value in its minimal form, without any additional whitespace.
	    /// The result is guaranteed to be a valid input for the method {@link Json#parse(String)} and to
	    /// create a value that is <em>equal</em> to this object.
	    /// </summary>
	    /// <returns>a JSON string that represents this value</returns>
	    @Override
	    public string ToString() {
	        return ToString(WriterConfig.MINIMAL);
	    }

	    /// <summary>
	    /// Returns the JSON string for this value using the given formatting.
	    /// </summary>
	    /// <param name="config">a configuration that controls the formatting or &lt;code&gt;null&lt;/code&gt; for the minimal form</param>
	    /// <returns>a JSON string that represents this value</returns>
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

	    /// <summary>
	    /// Indicates whether some other object is "equal to" this one according to the contract specified
	    /// in {@link Object#equals(Object)}.
	    /// <para />Two JsonValues are considered equal if and only if they represent the same JSON text. As a
	    /// consequence, two given JsonObjects may be different even though they contain the same set of
	    /// names with the same values, but in a different order.
	    /// </summary>
	    /// <param name="object">the reference object with which to compare</param>
	    /// <returns>true if this object is the same as the object argument; false otherwise</returns>
	    @Override
	    public bool Equals(object object) {
	        return super.Equals(object);
	    }

	    @Override
	    public int HashCode() {
	        return super.HasHCode;
	    }

	    abstract void Write(JsonWriter writer) ;

	}
} // end of namespace
