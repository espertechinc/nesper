///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

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
	/// A handler for parser events. Instances of this class can be given to a <seealso cref="JsonParser" />. The
	/// parser will then call the methods of the given handler while reading the input.
	/// <para />The default implementations of these methods do nothing. Subclasses may override only those
	/// methods they are interested in. They can use <code>getLocation()</code> to access the current
	/// character position of the parser at any point. The <code>start*</code> methods will be called
	/// while the location points to the first character of the parsed element. The <code>end*</code>methods will be called while the location points to the character position that directly follows
	/// the last character of the parsed element. Example:
	/// ["lorem ipsum"]
	/// ^            ^
	/// startString  endString
	/// <para />Subclasses that build an object representation of the parsed JSON can return arbitrary handler
	/// objects for JSON arrays and JSON objects in {@link #startArray()} and {@link #startObject()}.
	/// These handler objects will then be provided in all subsequent parser events for this particular
	/// array or object. They can be used to keep track the elements of a JSON array or object.
	/// </summary>
	/// <param name="&lt;A&gt;">The type of handlers used for JSON arrays</param>
	/// <param name="&lt;O&gt;">The type of handlers used for JSON objects</param>
	/// <unknown>@see JsonParser</unknown>
	public abstract class JsonHandler<A, O> {

	    JsonParser parser;

	    /// <summary>
	    /// Returns the current parser location.
	    /// </summary>
	    /// <returns>the current parser location</returns>
	    protected Location GetLocation() {
	        return parser.Location;
	    }

	    /// <summary>
	    /// Indicates the beginning of a <code>null</code> literal in the JSON input. This method will be
	    /// called when reading the first character of the literal.
	    /// </summary>
	    public void StartNull() {
	    }

	    /// <summary>
	    /// Indicates the end of a <code>null</code> literal in the JSON input. This method will be called
	    /// after reading the last character of the literal.
	    /// </summary>
	    public void EndNull() {
	    }

	    /// <summary>
	    /// Indicates the beginning of a boolean literal (<code>true</code> or <code>false</code>) in the
	    /// JSON input. This method will be called when reading the first character of the literal.
	    /// </summary>
	    public void StartBoolean() {
	    }

	    /// <summary>
	    /// Indicates the end of a boolean literal (<code>true</code> or <code>false</code>) in the JSON
	    /// input. This method will be called after reading the last character of the literal.
	    /// </summary>
	    /// <param name="value">the parsed boolean value</param>
	    public void EndBoolean(bool value) {
	    }

	    /// <summary>
	    /// Indicates the beginning of a string in the JSON input. This method will be called when reading
	    /// the opening double quote character (<code>'"'</code>).
	    /// </summary>
	    public void StartString() {
	    }

	    /// <summary>
	    /// Indicates the end of a string in the JSON input. This method will be called after reading the
	    /// closing double quote character (<code>'"'</code>).
	    /// </summary>
	    /// <param name="string">the parsed string</param>
	    public void EndString(string string) {
	    }

	    /// <summary>
	    /// Indicates the beginning of a number in the JSON input. This method will be called when reading
	    /// the first character of the number.
	    /// </summary>
	    public void StartNumber() {
	    }

	    /// <summary>
	    /// Indicates the end of a number in the JSON input. This method will be called after reading the
	    /// last character of the number.
	    /// </summary>
	    /// <param name="string">the parsed number string</param>
	    public void EndNumber(string string) {
	    }

	    /// <summary>
	    /// Indicates the beginning of an array in the JSON input. This method will be called when reading
	    /// the opening square bracket character (<code>'['</code>).
	    /// <para />This method may return an object to handle subsequent parser events for this array. This array
	    /// handler will then be provided in all calls to {@link #startArrayValue(Object)
	    /// startArrayValue()}, {@link #endArrayValue(Object) endArrayValue()}, and
	    /// {@link #endArray(Object) endArray()} for this array.
	    /// </summary>
	    /// <returns>a handler for this array, or &lt;code&gt;null&lt;/code&gt; if not needed</returns>
	    public A StartArray() {
	        return null;
	    }

	    /// <summary>
	    /// Indicates the end of an array in the JSON input. This method will be called after reading the
	    /// closing square bracket character (<code>']'</code>).
	    /// </summary>
	    /// <param name="array">the array handler returned from {@link #startArray()}, or &lt;code&gt;null&lt;/code&gt; if notprovided
	    /// </param>
	    public void EndArray(A array) {
	    }

	    /// <summary>
	    /// Indicates the beginning of an array element in the JSON input. This method will be called when
	    /// reading the first character of the element, just before the call to the <code>start</code>method for the specific element type ({@link #startString()}, {@link #startNumber()}, etc.).
	    /// </summary>
	    /// <param name="array">the array handler returned from {@link #startArray()}, or &lt;code&gt;null&lt;/code&gt; if notprovided
	    /// </param>
	    public void StartArrayValue(A array) {
	    }

	    /// <summary>
	    /// Indicates the end of an array element in the JSON input. This method will be called after
	    /// reading the last character of the element value, just after the <code>end</code> method for the
	    /// specific element type (like {@link #endString(String) endString()}, {@link #endNumber(String)
	    /// endNumber()}, etc.).
	    /// </summary>
	    /// <param name="array">the array handler returned from {@link #startArray()}, or &lt;code&gt;null&lt;/code&gt; if notprovided
	    /// </param>
	    public void EndArrayValue(A array) {
	    }

	    /// <summary>
	    /// Indicates the beginning of an object in the JSON input. This method will be called when reading
	    /// the opening curly bracket character (<code>'{'</code>).
	    /// <para />This method may return an object to handle subsequent parser events for this object. This
	    /// object handler will be provided in all calls to {@link #startObjectName(Object)
	    /// startObjectName()}, {@link #endObjectName(Object, String) endObjectName()},
	    /// {@link #startObjectValue(Object, String) startObjectValue()},
	    /// {@link #endObjectValue(Object, String) endObjectValue()}, and {@link #endObject(Object)
	    /// endObject()} for this object.
	    /// </summary>
	    /// <returns>a handler for this object, or &lt;code&gt;null&lt;/code&gt; if not needed</returns>
	    public O StartObject() {
	        return null;
	    }

	    /// <summary>
	    /// Indicates the end of an object in the JSON input. This method will be called after reading the
	    /// closing curly bracket character (<code>'}'</code>).
	    /// </summary>
	    /// <param name="object">the object handler returned from {@link #startObject()}, or null if not provided</param>
	    public void EndObject(O object) {
	    }

	    /// <summary>
	    /// Indicates the beginning of the name of an object member in the JSON input. This method will be
	    /// called when reading the opening quote character ('"') of the member name.
	    /// </summary>
	    /// <param name="object">the object handler returned from {@link #startObject()}, or &lt;code&gt;null&lt;/code&gt; if notprovided
	    /// </param>
	    public void StartObjectName(O object) {
	    }

	    /// <summary>
	    /// Indicates the end of an object member name in the JSON input. This method will be called after
	    /// reading the closing quote character (<code>'"'</code>) of the member name.
	    /// </summary>
	    /// <param name="object">the object handler returned from {@link #startObject()}, or null if not provided</param>
	    /// <param name="name">the parsed member name</param>
	    public void EndObjectName(O object, string name) {
	    }

	    /// <summary>
	    /// Indicates the beginning of the name of an object member in the JSON input. This method will be
	    /// called when reading the opening quote character ('"') of the member name.
	    /// </summary>
	    /// <param name="object">the object handler returned from {@link #startObject()}, or &lt;code&gt;null&lt;/code&gt; if notprovided
	    /// </param>
	    /// <param name="name">the member name</param>
	    public void StartObjectValue(O object, string name) {
	    }

	    /// <summary>
	    /// Indicates the end of an object member value in the JSON input. This method will be called after
	    /// reading the last character of the member value, just after the <code>end</code> method for the
	    /// specific member type (like {@link #endString(String) endString()}, {@link #endNumber(String)
	    /// endNumber()}, etc.).
	    /// </summary>
	    /// <param name="object">the object handler returned from {@link #startObject()}, or null if not provided</param>
	    /// <param name="name">the parsed member name</param>
	    public void EndObjectValue(O object, string name) {
	    }
	}
} // end of namespace
