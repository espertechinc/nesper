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
	/// This class serves as the entry point to the minimal-json API.
	/// <para />To <strong>parse</strong> a given JSON input, use the <code>parse()</code> methods like in this
	/// example:
	/// JsonObject object = Json.parse(string).asObject();
	/// <para />To <strong>create</strong> a JSON data structure to be serialized, use the methods
	/// <code>value()</code>, <code>array()</code>, and <code>object()</code>. For example, the following
	/// snippet will produce the JSON string <em>{"foo": 23, "bar": true}</em>:
	/// String string = Json.object().add("foo", 23).add("bar", true).toString();
	/// <para />To create a JSON array from a given Java array, you can use one of the <code>array()</code>methods with varargs parameters:
	/// String[] names = ...
	/// JsonArray array = Json.array(names);
	/// </summary>
	public final class Json {

	    private Json() {
	        // not meant to be instantiated
	    }

	    /// <summary>
	    /// Represents the JSON literal <code>null</code>.
	    /// </summary>
	    public static readonly JsonValue NULL = new JsonLiteral("null");

	    /// <summary>
	    /// Represents the JSON literal <code>true</code>.
	    /// </summary>
	    public static readonly JsonValue TRUE = new JsonLiteral("true");

	    /// <summary>
	    /// Represents the JSON literal <code>false</code>.
	    /// </summary>
	    public static readonly JsonValue FALSE = new JsonLiteral("false");

	    /// <summary>
	    /// Returns a JsonValue instance that represents the given <code>int</code> value.
	    /// </summary>
	    /// <param name="value">the value to get a JSON representation for</param>
	    /// <returns>a JSON value that represents the given value</returns>
	    public static JsonValue Value(int value) {
	        return new JsonNumber(int?.ToString(value, 10));
	    }

	    /// <summary>
	    /// Returns a JsonValue instance that represents the given <code>long</code> value.
	    /// </summary>
	    /// <param name="value">the value to get a JSON representation for</param>
	    /// <returns>a JSON value that represents the given value</returns>
	    public static JsonValue Value(long value) {
	        return new JsonNumber(long?.ToString(value, 10));
	    }

	    /// <summary>
	    /// Returns a JsonValue instance that represents the given <code>float</code> value.
	    /// </summary>
	    /// <param name="value">the value to get a JSON representation for</param>
	    /// <returns>a JSON value that represents the given value</returns>
	    public static JsonValue Value(float value) {
	        if (Float.IsInfinite(value) || Float.IsNaN(value)) {
	            throw new ArgumentException("Infinite and NaN values not permitted in JSON");
	        }
	        return new JsonNumber(CutOffPointZero(Float.ToString(value)));
	    }

	    /// <summary>
	    /// Returns a JsonValue instance that represents the given <code>double</code> value.
	    /// </summary>
	    /// <param name="value">the value to get a JSON representation for</param>
	    /// <returns>a JSON value that represents the given value</returns>
	    public static JsonValue Value(double value) {
	        if (Double.IsInfinite(value) || Double.IsNaN(value)) {
	            throw new ArgumentException("Infinite and NaN values not permitted in JSON");
	        }
	        return new JsonNumber(CutOffPointZero(Double.ToString(value)));
	    }

	    /// <summary>
	    /// Returns a JsonValue instance that represents the given string.
	    /// </summary>
	    /// <param name="string">the string to get a JSON representation for</param>
	    /// <returns>a JSON value that represents the given string</returns>
	    public static JsonValue Value(string string) {
	        return string == null ? NULL : new JsonString(string);
	    }

	    /// <summary>
	    /// Returns a JsonValue instance that represents the given <code>boolean</code> value.
	    /// </summary>
	    /// <param name="value">the value to get a JSON representation for</param>
	    /// <returns>a JSON value that represents the given value</returns>
	    public static JsonValue Value(bool value) {
	        return value ? TRUE : FALSE;
	    }

	    /// <summary>
	    /// Creates a new empty JsonArray. This is equivalent to creating a new JsonArray using the
	    /// constructor.
	    /// </summary>
	    /// <returns>a new empty JSON array</returns>
	    public static JsonArray Array() {
	        return new JsonArray();
	    }

	    /// <summary>
	    /// Creates a new JsonArray that contains the JSON representations of the given <code>int</code>values.
	    /// </summary>
	    /// <param name="values">the values to be included in the new JSON array</param>
	    /// <returns>a new JSON array that contains the given values</returns>
	    public static JsonArray Array(int... values) {
	        if (values == null) {
	            throw new NullPointerException("values is null");
	        }
	        JsonArray array = new JsonArray();
	        foreach (int value in values) {
	            array.Add(value);
	        }
	        return array;
	    }

	    /// <summary>
	    /// Creates a new JsonArray that contains the JSON representations of the given <code>long</code>values.
	    /// </summary>
	    /// <param name="values">the values to be included in the new JSON array</param>
	    /// <returns>a new JSON array that contains the given values</returns>
	    public static JsonArray Array(long... values) {
	        if (values == null) {
	            throw new NullPointerException("values is null");
	        }
	        JsonArray array = new JsonArray();
	        foreach (long value in values) {
	            array.Add(value);
	        }
	        return array;
	    }

	    /// <summary>
	    /// Creates a new JsonArray that contains the JSON representations of the given <code>float</code>values.
	    /// </summary>
	    /// <param name="values">the values to be included in the new JSON array</param>
	    /// <returns>a new JSON array that contains the given values</returns>
	    public static JsonArray Array(float... values) {
	        if (values == null) {
	            throw new NullPointerException("values is null");
	        }
	        JsonArray array = new JsonArray();
	        foreach (float value in values) {
	            array.Add(value);
	        }
	        return array;
	    }

	    /// <summary>
	    /// Creates a new JsonArray that contains the JSON representations of the given <code>double</code>values.
	    /// </summary>
	    /// <param name="values">the values to be included in the new JSON array</param>
	    /// <returns>a new JSON array that contains the given values</returns>
	    public static JsonArray Array(double... values) {
	        if (values == null) {
	            throw new NullPointerException("values is null");
	        }
	        JsonArray array = new JsonArray();
	        foreach (double value in values) {
	            array.Add(value);
	        }
	        return array;
	    }

	    /// <summary>
	    /// Creates a new JsonArray that contains the JSON representations of the given
	    /// <code>boolean</code> values.
	    /// </summary>
	    /// <param name="values">the values to be included in the new JSON array</param>
	    /// <returns>a new JSON array that contains the given values</returns>
	    public static JsonArray Array(bool... values) {
	        if (values == null) {
	            throw new NullPointerException("values is null");
	        }
	        JsonArray array = new JsonArray();
	        foreach (bool value in values) {
	            array.Add(value);
	        }
	        return array;
	    }

	    /// <summary>
	    /// Creates a new JsonArray that contains the JSON representations of the given strings.
	    /// </summary>
	    /// <param name="strings">the strings to be included in the new JSON array</param>
	    /// <returns>a new JSON array that contains the given strings</returns>
	    public static JsonArray Array(string... strings) {
	        if (strings == null) {
	            throw new NullPointerException("values is null");
	        }
	        JsonArray array = new JsonArray();
	        foreach (string value in strings) {
	            array.Add(value);
	        }
	        return array;
	    }

	    /// <summary>
	    /// Creates a new empty JsonObject. This is equivalent to creating a new JsonObject using the
	    /// constructor.
	    /// </summary>
	    /// <returns>a new empty JSON object</returns>
	    public static JsonObject Object() {
	        return new JsonObject();
	    }

	    /// <summary>
	    /// Parses the given input string as JSON. The input must contain a valid JSON value, optionally
	    /// padded with whitespace.
	    /// </summary>
	    /// <param name="string">the input string, must be valid JSON</param>
	    /// <returns>a value that represents the parsed JSON</returns>
	    /// <throws>ParseException if the input is not valid JSON</throws>
	    public static JsonValue Parse(string string) {
	        if (string == null) {
	            throw new NullPointerException("string is null");
	        }
	        DefaultHandler handler = new DefaultHandler();
	        new JsonParser(handler).Parse(string);
	        return handler.Value;
	    }

	    /// <summary>
	    /// Reads the entire input from the given reader and parses it as JSON. The input must contain a
	    /// valid JSON value, optionally padded with whitespace.
	    /// <para />Characters are read in chunks into an input buffer. Hence, wrapping a reader in an additional
	    /// <code>BufferedReader</code> likely won't improve reading performance.
	    /// </summary>
	    /// <param name="reader">the reader to read the JSON value from</param>
	    /// <returns>a value that represents the parsed JSON</returns>
	    /// <throws>IOException    if an I/O error occurs in the reader</throws>
	    /// <throws>ParseException if the input is not valid JSON</throws>
	    public static JsonValue Parse(Reader reader) {
	        if (reader == null) {
	            throw new NullPointerException("reader is null");
	        }
	        DefaultHandler handler = new DefaultHandler();
	        new JsonParser(handler).Parse(reader);
	        return handler.Value;
	    }

	    private static string CutOffPointZero(string string) {
	        if (string.EndsWith(".0")) {
	            return string.Substring(0, string.Length() - 2);
	        }
	        return string;
	    }

	    static class DefaultHandler : JsonHandler<JsonArray, JsonObject> {

	        protected JsonValue value;

	        @Override
	        public JsonArray StartArray() {
	            return new JsonArray();
	        }

	        @Override
	        public JsonObject StartObject() {
	            return new JsonObject();
	        }

	        @Override
	        public void EndNull() {
	            value = NULL;
	        }

	        @Override
	        public void EndBoolean(bool bool) {
	            value = bool ? TRUE : FALSE;
	        }

	        @Override
	        public void EndString(string string) {
	            value = new JsonString(string);
	        }

	        @Override
	        public void EndNumber(string string) {
	            value = new JsonNumber(string);
	        }

	        @Override
	        public void EndArray(JsonArray array) {
	            value = array;
	        }

	        @Override
	        public void EndObject(JsonObject object) {
	            value = object;
	        }

	        @Override
	        public void EndArrayValue(JsonArray array) {
	            array.Add(value);
	        }

	        @Override
	        public void EndObjectValue(JsonObject object, string name) {
	            object.Add(name, value);
	        }

	        JsonValue GetValue() {
	            return value;
	        }

	        public object GetBean() {
	            throw new UnsupportedOperationException("No result bean available");
	        }
	    }
	}
} // end of namespace
