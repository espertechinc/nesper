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
	/// Represents a JSON object, a set of name/value pairs, where the names are strings and the values
	/// are JSON values.
	/// <para />Members can be added using the <code>add(String, ...)</code> methods which accept instances of
	/// <seealso cref="JsonValue" />, strings, primitive numbers, and boolean values. To modify certain values of an
	/// object, use the <code>set(String, ...)</code> methods. Please note that the <code>add</code>methods are faster than <code>set</code> as they do not search for existing members. On the other
	/// hand, the <code>add</code> methods do not prevent adding multiple members with the same name.
	/// Duplicate names are discouraged but not prohibited by JSON.
	/// <para />Members can be accessed by their name using {@link #get(String)}. A list of all names can be
	/// obtained from the method {@link #names()}. This class also supports iterating over the members in
	/// document order using an {@link #iterator()} or an enhanced for loop:
	/// for (Member member : jsonObject) {
	/// String name = member.getName();
	/// JsonValue value = member.getValue();
	/// ...
	/// }
	/// <para />Even though JSON objects are unordered by definition, instances of this class preserve the order
	/// of members to allow processing in document order and to guarantee a predictable output.
	/// <para />Note that this class is <strong>not thread-safe</strong>. If multiple threads access a
	/// <code>JsonObject</code> instance concurrently, while at least one of these threads modifies the
	/// contents of this object, access to the instance must be synchronized externally. Failure to do so
	/// may lead to an inconsistent state.
	/// <para />This class is <strong>not supposed to be extended</strong> by clients.
	/// </summary>
	// use default serial UID
	public class JsonObject : JsonValue , Iterable<Member> {

	    private readonly IList<string> names;
	    private readonly IList<JsonValue> values;
	    private transient HashIndexTable table;

	    /// <summary>
	    /// Creates a new empty JsonObject.
	    /// </summary>
	    public JsonObject() {
	        names = new List<string>();
	        values = new List<JsonValue>();
	        table = new HashIndexTable();
	    }

	    /// <summary>
	    /// Creates a new JsonObject, initialized with the contents of the specified JSON object.
	    /// </summary>
	    /// <param name="object">the JSON object to get the initial contents from, must not be &lt;code&gt;null&lt;/code&gt;</param>
	    public JsonObject(JsonObject object) {
	        This(object, false);
	    }

	    private JsonObject(JsonObject object, bool unmodifiable) {
	        if (object == null) {
	            throw new NullPointerException("object is null");
	        }
	        if (unmodifiable) {
	            names = Collections.UnmodifiableList(object.names);
	            values = Collections.UnmodifiableList(object.values);
	        } else {
	            names = new List<string>(object.names);
	            values = new List<JsonValue>(object.values);
	        }
	        table = new HashIndexTable();
	        UpdateHashIndex();
	    }

	    /// <summary>
	    /// Reads a JSON object from the given reader.
	    /// <para />Characters are read in chunks and buffered internally, therefore wrapping an existing reader in
	    /// an additional <code>BufferedReader</code> does <strong>not</strong> improve reading
	    /// performance.
	    /// </summary>
	    /// <param name="reader">the reader to read the JSON object from</param>
	    /// <returns>the JSON object that has been read</returns>
	    /// <throws>IOException                   if an I/O error occurs in the reader</throws>
	    /// <throws>ParseException                if the input is not valid JSON</throws>
	    /// <throws>UnsupportedOperationException if the input does not contain a JSON object</throws>
	    /// <unknown>@deprecated Use {@link Json#parse(Reader)}{@link JsonValue#asObject() .asObject()} instead</unknown>
	    @Deprecated
	    public static JsonObject ReadFrom(Reader reader) {
	        return JsonValue.ReadFrom(reader).AsObject();
	    }

	    /// <summary>
	    /// Reads a JSON object from the given string.
	    /// </summary>
	    /// <param name="string">the string that contains the JSON object</param>
	    /// <returns>the JSON object that has been read</returns>
	    /// <throws>ParseException                if the input is not valid JSON</throws>
	    /// <throws>UnsupportedOperationException if the input does not contain a JSON object</throws>
	    /// <unknown>@deprecated Use {@link Json#parse(String)}{@link JsonValue#asObject() .asObject()} instead</unknown>
	    @Deprecated
	    public static JsonObject ReadFrom(string string) {
	        return JsonValue.ReadFrom(string).AsObject();
	    }

	    /// <summary>
	    /// Returns an unmodifiable JsonObject for the specified one. This method allows to provide
	    /// read-only access to a JsonObject.
	    /// <para />The returned JsonObject is backed by the given object and reflect changes that happen to it.
	    /// Attempts to modify the returned JsonObject result in an
	    /// <code>UnsupportedOperationException</code>.
	    /// </summary>
	    /// <param name="object">the JsonObject for which an unmodifiable JsonObject is to be returned</param>
	    /// <returns>an unmodifiable view of the specified JsonObject</returns>
	    public static JsonObject UnmodifiableObject(JsonObject object) {
	        return new JsonObject(object, true);
	    }

	    /// <summary>
	    /// Appends a new member to the end of this object, with the specified name and the JSON
	    /// representation of the specified <code>int</code> value.
	    /// <para />This method <strong>does not prevent duplicate names</strong>. Calling this method with a name
	    /// that already exists in the object will append another member with the same name. In order to
	    /// replace existing members, use the method <code>set(name, value)</code> instead. However,
	    /// <strong><em>add</em> is much faster than <em>set</em></strong> (because it does not need to
	    /// search for existing members). Therefore <em>add</em> should be preferred when constructing new
	    /// objects.
	    /// </summary>
	    /// <param name="name">the name of the member to add</param>
	    /// <param name="value">the value of the member to add</param>
	    /// <returns>the object itself, to enable method chaining</returns>
	    public JsonObject Add(string name, int value) {
	        Add(name, Json.Value(value));
	        return this;
	    }

	    /// <summary>
	    /// Appends a new member to the end of this object, with the specified name and the JSON
	    /// representation of the specified <code>long</code> value.
	    /// <para />This method <strong>does not prevent duplicate names</strong>. Calling this method with a name
	    /// that already exists in the object will append another member with the same name. In order to
	    /// replace existing members, use the method <code>set(name, value)</code> instead. However,
	    /// <strong><em>add</em> is much faster than <em>set</em></strong> (because it does not need to
	    /// search for existing members). Therefore <em>add</em> should be preferred when constructing new
	    /// objects.
	    /// </summary>
	    /// <param name="name">the name of the member to add</param>
	    /// <param name="value">the value of the member to add</param>
	    /// <returns>the object itself, to enable method chaining</returns>
	    public JsonObject Add(string name, long value) {
	        Add(name, Json.Value(value));
	        return this;
	    }

	    /// <summary>
	    /// Appends a new member to the end of this object, with the specified name and the JSON
	    /// representation of the specified <code>float</code> value.
	    /// <para />This method <strong>does not prevent duplicate names</strong>. Calling this method with a name
	    /// that already exists in the object will append another member with the same name. In order to
	    /// replace existing members, use the method <code>set(name, value)</code> instead. However,
	    /// <strong><em>add</em> is much faster than <em>set</em></strong> (because it does not need to
	    /// search for existing members). Therefore <em>add</em> should be preferred when constructing new
	    /// objects.
	    /// </summary>
	    /// <param name="name">the name of the member to add</param>
	    /// <param name="value">the value of the member to add</param>
	    /// <returns>the object itself, to enable method chaining</returns>
	    public JsonObject Add(string name, float value) {
	        Add(name, Json.Value(value));
	        return this;
	    }

	    /// <summary>
	    /// Appends a new member to the end of this object, with the specified name and the JSON
	    /// representation of the specified <code>double</code> value.
	    /// <para />This method <strong>does not prevent duplicate names</strong>. Calling this method with a name
	    /// that already exists in the object will append another member with the same name. In order to
	    /// replace existing members, use the method <code>set(name, value)</code> instead. However,
	    /// <strong><em>add</em> is much faster than <em>set</em></strong> (because it does not need to
	    /// search for existing members). Therefore <em>add</em> should be preferred when constructing new
	    /// objects.
	    /// </summary>
	    /// <param name="name">the name of the member to add</param>
	    /// <param name="value">the value of the member to add</param>
	    /// <returns>the object itself, to enable method chaining</returns>
	    public JsonObject Add(string name, double value) {
	        Add(name, Json.Value(value));
	        return this;
	    }

	    /// <summary>
	    /// Appends a new member to the end of this object, with the specified name and the JSON
	    /// representation of the specified <code>boolean</code> value.
	    /// <para />This method <strong>does not prevent duplicate names</strong>. Calling this method with a name
	    /// that already exists in the object will append another member with the same name. In order to
	    /// replace existing members, use the method <code>set(name, value)</code> instead. However,
	    /// <strong><em>add</em> is much faster than <em>set</em></strong> (because it does not need to
	    /// search for existing members). Therefore <em>add</em> should be preferred when constructing new
	    /// objects.
	    /// </summary>
	    /// <param name="name">the name of the member to add</param>
	    /// <param name="value">the value of the member to add</param>
	    /// <returns>the object itself, to enable method chaining</returns>
	    public JsonObject Add(string name, bool value) {
	        Add(name, Json.Value(value));
	        return this;
	    }

	    /// <summary>
	    /// Appends a new member to the end of this object, with the specified name and the JSON
	    /// representation of the specified string.
	    /// <para />This method <strong>does not prevent duplicate names</strong>. Calling this method with a name
	    /// that already exists in the object will append another member with the same name. In order to
	    /// replace existing members, use the method <code>set(name, value)</code> instead. However,
	    /// <strong><em>add</em> is much faster than <em>set</em></strong> (because it does not need to
	    /// search for existing members). Therefore <em>add</em> should be preferred when constructing new
	    /// objects.
	    /// </summary>
	    /// <param name="name">the name of the member to add</param>
	    /// <param name="value">the value of the member to add</param>
	    /// <returns>the object itself, to enable method chaining</returns>
	    public JsonObject Add(string name, string value) {
	        Add(name, Json.Value(value));
	        return this;
	    }

	    /// <summary>
	    /// Appends a new member to the end of this object, with the specified name and the specified JSON
	    /// value.
	    /// <para />This method <strong>does not prevent duplicate names</strong>. Calling this method with a name
	    /// that already exists in the object will append another member with the same name. In order to
	    /// replace existing members, use the method <code>set(name, value)</code> instead. However,
	    /// <strong><em>add</em> is much faster than <em>set</em></strong> (because it does not need to
	    /// search for existing members). Therefore <em>add</em> should be preferred when constructing new
	    /// objects.
	    /// </summary>
	    /// <param name="name">the name of the member to add</param>
	    /// <param name="value">the value of the member to add, must not be &lt;code&gt;null&lt;/code&gt;</param>
	    /// <returns>the object itself, to enable method chaining</returns>
	    public JsonObject Add(string name, JsonValue value) {
	        if (name == null) {
	            throw new NullPointerException("name is null");
	        }
	        if (value == null) {
	            throw new NullPointerException("value is null");
	        }
	        table.Add(name, names.Count);
	        names.Add(name);
	        values.Add(value);
	        return this;
	    }

	    /// <summary>
	    /// Sets the value of the member with the specified name to the JSON representation of the
	    /// specified <code>int</code> value. If this object does not contain a member with this name, a
	    /// new member is added at the end of the object. If this object contains multiple members with
	    /// this name, only the last one is changed.
	    /// <para />This method should <strong>only be used to modify existing objects</strong>. To fill a new
	    /// object with members, the method <code>add(name, value)</code> should be preferred which is much
	    /// faster (as it does not need to search for existing members).
	    /// </summary>
	    /// <param name="name">the name of the member to replace</param>
	    /// <param name="value">the value to set to the member</param>
	    /// <returns>the object itself, to enable method chaining</returns>
	    public JsonObject Set(string name, int value) {
	        Set(name, Json.Value(value));
	        return this;
	    }

	    /// <summary>
	    /// Sets the value of the member with the specified name to the JSON representation of the
	    /// specified <code>long</code> value. If this object does not contain a member with this name, a
	    /// new member is added at the end of the object. If this object contains multiple members with
	    /// this name, only the last one is changed.
	    /// <para />This method should <strong>only be used to modify existing objects</strong>. To fill a new
	    /// object with members, the method <code>add(name, value)</code> should be preferred which is much
	    /// faster (as it does not need to search for existing members).
	    /// </summary>
	    /// <param name="name">the name of the member to replace</param>
	    /// <param name="value">the value to set to the member</param>
	    /// <returns>the object itself, to enable method chaining</returns>
	    public JsonObject Set(string name, long value) {
	        Set(name, Json.Value(value));
	        return this;
	    }

	    /// <summary>
	    /// Sets the value of the member with the specified name to the JSON representation of the
	    /// specified <code>float</code> value. If this object does not contain a member with this name, a
	    /// new member is added at the end of the object. If this object contains multiple members with
	    /// this name, only the last one is changed.
	    /// <para />This method should <strong>only be used to modify existing objects</strong>. To fill a new
	    /// object with members, the method <code>add(name, value)</code> should be preferred which is much
	    /// faster (as it does not need to search for existing members).
	    /// </summary>
	    /// <param name="name">the name of the member to add</param>
	    /// <param name="value">the value of the member to add</param>
	    /// <returns>the object itself, to enable method chaining</returns>
	    public JsonObject Set(string name, float value) {
	        Set(name, Json.Value(value));
	        return this;
	    }

	    /// <summary>
	    /// Sets the value of the member with the specified name to the JSON representation of the
	    /// specified <code>double</code> value. If this object does not contain a member with this name, a
	    /// new member is added at the end of the object. If this object contains multiple members with
	    /// this name, only the last one is changed.
	    /// <para />This method should <strong>only be used to modify existing objects</strong>. To fill a new
	    /// object with members, the method <code>add(name, value)</code> should be preferred which is much
	    /// faster (as it does not need to search for existing members).
	    /// </summary>
	    /// <param name="name">the name of the member to add</param>
	    /// <param name="value">the value of the member to add</param>
	    /// <returns>the object itself, to enable method chaining</returns>
	    public JsonObject Set(string name, double value) {
	        Set(name, Json.Value(value));
	        return this;
	    }

	    /// <summary>
	    /// Sets the value of the member with the specified name to the JSON representation of the
	    /// specified <code>boolean</code> value. If this object does not contain a member with this name,
	    /// a new member is added at the end of the object. If this object contains multiple members with
	    /// this name, only the last one is changed.
	    /// <para />This method should <strong>only be used to modify existing objects</strong>. To fill a new
	    /// object with members, the method <code>add(name, value)</code> should be preferred which is much
	    /// faster (as it does not need to search for existing members).
	    /// </summary>
	    /// <param name="name">the name of the member to add</param>
	    /// <param name="value">the value of the member to add</param>
	    /// <returns>the object itself, to enable method chaining</returns>
	    public JsonObject Set(string name, bool value) {
	        Set(name, Json.Value(value));
	        return this;
	    }

	    /// <summary>
	    /// Sets the value of the member with the specified name to the JSON representation of the
	    /// specified string. If this object does not contain a member with this name, a new member is
	    /// added at the end of the object. If this object contains multiple members with this name, only
	    /// the last one is changed.
	    /// <para />This method should <strong>only be used to modify existing objects</strong>. To fill a new
	    /// object with members, the method <code>add(name, value)</code> should be preferred which is much
	    /// faster (as it does not need to search for existing members).
	    /// </summary>
	    /// <param name="name">the name of the member to add</param>
	    /// <param name="value">the value of the member to add</param>
	    /// <returns>the object itself, to enable method chaining</returns>
	    public JsonObject Set(string name, string value) {
	        Set(name, Json.Value(value));
	        return this;
	    }

	    /// <summary>
	    /// Sets the value of the member with the specified name to the specified JSON value. If this
	    /// object does not contain a member with this name, a new member is added at the end of the
	    /// object. If this object contains multiple members with this name, only the last one is changed.
	    /// <para />This method should <strong>only be used to modify existing objects</strong>. To fill a new
	    /// object with members, the method <code>add(name, value)</code> should be preferred which is much
	    /// faster (as it does not need to search for existing members).
	    /// </summary>
	    /// <param name="name">the name of the member to add</param>
	    /// <param name="value">the value of the member to add, must not be &lt;code&gt;null&lt;/code&gt;</param>
	    /// <returns>the object itself, to enable method chaining</returns>
	    public JsonObject Set(string name, JsonValue value) {
	        if (name == null) {
	            throw new NullPointerException("name is null");
	        }
	        if (value == null) {
	            throw new NullPointerException("value is null");
	        }
	        int index = IndexOf(name);
	        if (index != -1) {
	            values.Set(index, value);
	        } else {
	            table.Add(name, names.Count);
	            names.Add(name);
	            values.Add(value);
	        }
	        return this;
	    }

	    /// <summary>
	    /// Removes a member with the specified name from this object. If this object contains multiple
	    /// members with the given name, only the last one is removed. If this object does not contain a
	    /// member with the specified name, the object is not modified.
	    /// </summary>
	    /// <param name="name">the name of the member to remove</param>
	    /// <returns>the object itself, to enable method chaining</returns>
	    public JsonObject Remove(string name) {
	        if (name == null) {
	            throw new NullPointerException("name is null");
	        }
	        int index = IndexOf(name);
	        if (index != -1) {
	            table.Remove(index);
	            names.Remove(index);
	            values.Remove(index);
	        }
	        return this;
	    }

	    /// <summary>
	    /// Checks if a specified member is present as a child of this object. This will not test if
	    /// this object contains the literal <code>null</code>, {@link JsonValue#isNull()} should be used
	    /// for this purpose.
	    /// </summary>
	    /// <param name="name">the name of the member to check for</param>
	    /// <returns>whether or not the member is present</returns>
	    public bool Contains(string name) {
	        return names.Contains(name);
	    }

	    /// <summary>
	    /// Copies all members of the specified object into this object. When the specified object contains
	    /// members with names that also exist in this object, the existing values in this object will be
	    /// replaced by the corresponding values in the specified object.
	    /// </summary>
	    /// <param name="object">the object to merge</param>
	    /// <returns>the object itself, to enable method chaining</returns>
	    public JsonObject Merge(JsonObject object) {
	        if (object == null) {
	            throw new NullPointerException("object is null");
	        }
	        foreach (Member member in object) {
	            this.Set(member.name, member.value);
	        }
	        return this;
	    }

	    /// <summary>
	    /// Returns the value of the member with the specified name in this object. If this object contains
	    /// multiple members with the given name, this method will return the last one.
	    /// </summary>
	    /// <param name="name">the name of the member whose value is to be returned</param>
	    /// <returns>the value of the last member with the specified name, or &lt;code&gt;null&lt;/code&gt; if thisobject does not contain a member with that name
	    /// </returns>
	    public JsonValue Get(string name) {
	        if (name == null) {
	            throw new NullPointerException("name is null");
	        }
	        int index = IndexOf(name);
	        return index != -1 ? values.Get(index) : null;
	    }

	    /// <summary>
	    /// Returns exists-flag.
	    /// </summary>
	    /// <param name="name">to check</param>
	    /// <returns>indicator</returns>
	    public bool Exists(string name) {
	        if (name == null) {
	            throw new NullPointerException("name is null");
	        }
	        return IndexOf(name) != -1;
	    }

	    /// <summary>
	    /// Returns the <code>int</code> value of the member with the specified name in this object. If
	    /// this object does not contain a member with this name, the given default value is returned. If
	    /// this object contains multiple members with the given name, the last one will be picked. If this
	    /// member's value does not represent a JSON number or if it cannot be interpreted as Java
	    /// <code>int</code>, an exception is thrown.
	    /// </summary>
	    /// <param name="name">the name of the member whose value is to be returned</param>
	    /// <param name="defaultValue">the value to be returned if the requested member is missing</param>
	    /// <returns>the value of the last member with the specified name, or the given default value ifthis object does not contain a member with that name
	    /// </returns>
	    public int GetInt(string name, int defaultValue) {
	        JsonValue value = Get(name);
	        return value != null ? value.AsInt() : defaultValue;
	    }

	    /// <summary>
	    /// Returns the <code>long</code> value of the member with the specified name in this object. If
	    /// this object does not contain a member with this name, the given default value is returned. If
	    /// this object contains multiple members with the given name, the last one will be picked. If this
	    /// member's value does not represent a JSON number or if it cannot be interpreted as Java
	    /// <code>long</code>, an exception is thrown.
	    /// </summary>
	    /// <param name="name">the name of the member whose value is to be returned</param>
	    /// <param name="defaultValue">the value to be returned if the requested member is missing</param>
	    /// <returns>the value of the last member with the specified name, or the given default value ifthis object does not contain a member with that name
	    /// </returns>
	    public long GetLong(string name, long defaultValue) {
	        JsonValue value = Get(name);
	        return value != null ? value.AsLong() : defaultValue;
	    }

	    /// <summary>
	    /// Returns the <code>float</code> value of the member with the specified name in this object. If
	    /// this object does not contain a member with this name, the given default value is returned. If
	    /// this object contains multiple members with the given name, the last one will be picked. If this
	    /// member's value does not represent a JSON number or if it cannot be interpreted as Java
	    /// <code>float</code>, an exception is thrown.
	    /// </summary>
	    /// <param name="name">the name of the member whose value is to be returned</param>
	    /// <param name="defaultValue">the value to be returned if the requested member is missing</param>
	    /// <returns>the value of the last member with the specified name, or the given default value ifthis object does not contain a member with that name
	    /// </returns>
	    public float GetFloat(string name, float defaultValue) {
	        JsonValue value = Get(name);
	        return value != null ? value.AsFloat() : defaultValue;
	    }

	    /// <summary>
	    /// Returns the <code>double</code> value of the member with the specified name in this object. If
	    /// this object does not contain a member with this name, the given default value is returned. If
	    /// this object contains multiple members with the given name, the last one will be picked. If this
	    /// member's value does not represent a JSON number or if it cannot be interpreted as Java
	    /// <code>double</code>, an exception is thrown.
	    /// </summary>
	    /// <param name="name">the name of the member whose value is to be returned</param>
	    /// <param name="defaultValue">the value to be returned if the requested member is missing</param>
	    /// <returns>the value of the last member with the specified name, or the given default value ifthis object does not contain a member with that name
	    /// </returns>
	    public double GetDouble(string name, double defaultValue) {
	        JsonValue value = Get(name);
	        return value != null ? value.AsDouble() : defaultValue;
	    }

	    /// <summary>
	    /// Returns the <code>boolean</code> value of the member with the specified name in this object. If
	    /// this object does not contain a member with this name, the given default value is returned. If
	    /// this object contains multiple members with the given name, the last one will be picked. If this
	    /// member's value does not represent a JSON <code>true</code> or <code>false</code> value, an
	    /// exception is thrown.
	    /// </summary>
	    /// <param name="name">the name of the member whose value is to be returned</param>
	    /// <param name="defaultValue">the value to be returned if the requested member is missing</param>
	    /// <returns>the value of the last member with the specified name, or the given default value ifthis object does not contain a member with that name
	    /// </returns>
	    public bool GetBoolean(string name, bool defaultValue) {
	        JsonValue value = Get(name);
	        return value != null ? value.AsBoolean() : defaultValue;
	    }

	    /// <summary>
	    /// Returns the <code>String</code> value of the member with the specified name in this object. If
	    /// this object does not contain a member with this name, the given default value is returned. If
	    /// this object contains multiple members with the given name, the last one is picked. If this
	    /// member's value does not represent a JSON string, an exception is thrown.
	    /// </summary>
	    /// <param name="name">the name of the member whose value is to be returned</param>
	    /// <param name="defaultValue">the value to be returned if the requested member is missing</param>
	    /// <returns>the value of the last member with the specified name, or the given default value ifthis object does not contain a member with that name
	    /// </returns>
	    public string GetString(string name, string defaultValue) {
	        JsonValue value = Get(name);
	        return value != null ? value.AsString() : defaultValue;
	    }

	    /// <summary>
	    /// Returns the number of members (name/value pairs) in this object.
	    /// </summary>
	    /// <returns>the number of members in this object</returns>
	    public int Size() {
	        return names.Count;
	    }

	    /// <summary>
	    /// Returns <code>true</code> if this object contains no members.
	    /// </summary>
	    /// <returns>&lt;code&gt;true&lt;/code&gt; if this object contains no members</returns>
	    public bool IsEmpty() {
	        return names.IsEmpty();
	    }

	    /// <summary>
	    /// Returns a list of the names in this object in document order. The returned list is backed by
	    /// this object and will reflect subsequent changes. It cannot be used to modify this object.
	    /// Attempts to modify the returned list will result in an exception.
	    /// </summary>
	    /// <returns>a list of the names in this object</returns>
	    public IList<string> Names() {
	        return Collections.UnmodifiableList(names);
	    }

	    /// <summary>
	    /// Returns an iterator over the members of this object in document order. The returned iterator
	    /// cannot be used to modify this object.
	    /// </summary>
	    /// <returns>an iterator over the members of this object</returns>
	    public IEnumerator<Member> Iterator() {
	        IEnumerator<string> namesIterator = names.Iterator();
	        IEnumerator<JsonValue> valuesIterator = values.Iterator();
	        return new IEnProxyIterator<Member>() {

	            ProcHasNext = () =>  {
	                return namesIterator.HasNext;
	            },

	            ProcNext = () =>  {
	                string name = namesIterator.Next();
	                JsonValue value = valuesIterator.Next();
	                return new Member(name, value);
	            },

	            ProcRemove = () =>  {
	                throw new UnsupportedOperationException();
	            },

	        };
	    }

	    @Override
	    void Write(JsonWriter writer) {
	        writer.WriteObjectOpen();
	        IEnumerator<string> namesIterator = names.Iterator();
	        IEnumerator<JsonValue> valuesIterator = values.Iterator();
	        if (namesIterator.HasNext) {
	            writer.WriteMemberName(namesIterator.Next());
	            writer.WriteMemberSeparator();
	            valuesIterator.Next().Write(writer);
	            while (namesIterator.HasNext) {
	                writer.WriteObjectSeparator();
	                writer.WriteMemberName(namesIterator.Next());
	                writer.WriteMemberSeparator();
	                valuesIterator.Next().Write(writer);
	            }
	        }
	        writer.WriteObjectClose();
	    }

	    @Override
	    public bool IsObject() {
	        return true;
	    }

	    @Override
	    public JsonObject AsObject() {
	        return this;
	    }

	    @Override
	    public int HashCode() {
	        int result = 1;
	        result = 31 * result + names.HasHCode;
	        result = 31 * result + values.HasHCode;
	        return result;
	    }

	    @Override
	    public bool Equals(object obj) {
	        if (this == obj) {
	            return true;
	        }
	        if (obj == null) {
	            return false;
	        }
	        if (GetType() != obj.GetType()) {
	            return false;
	        }
	        JsonObject other = (JsonObject) obj;
	        return names.Equals(other.names) && values.Equals(other.values);
	    }

	    int IndexOf(string name) {
	        int index = table.Get(name);
	        if (index != -1 && name.Equals(names.Get(index))) {
	            return index;
	        }
	        return names.LastIndexOf(name);
	    }

	    private synchronized void ReadObject(ObjectInputStream inputStream)
	        {
	        inputStream.DefaultReadObject();
	        table = new HashIndexTable();
	        UpdateHashIndex();
	    }

	    private void UpdateHashIndex() {
	        int size = names.Count;
	        for (int i = 0; i < size; i++) {
	            table.Add(names.Get(i), i);
	        }
	    }

	    static class HashIndexTable {

	        private readonly byte[] hashTable = new byte[32]; // must be a power of two

	        HashIndexTable() {
	        }

	        HashIndexTable(HashIndexTable original) {
	            System.Arraycopy(original.hashTable, 0, hashTable, 0, hashTable.Length);
	        }

	        void Add(string name, int index) {
	            int slot = HashSlotFor(name);
	            if (index < 0xff) {
	                // increment by 1, 0 stands for empty
	                hashTable[slot] = (byte) (index + 1);
	            } else {
	                hashTable[slot] = 0;
	            }
	        }

	        void Remove(int index) {
	            for (int i = 0; i < hashTable.Length; i++) {
	                if ((hashTable[i] & 0xff) == index + 1) {
	                    hashTable[i] = 0;
	                } else if ((hashTable[i] & 0xff) > index + 1) {
	                    hashTable[i]--;
	                }
	            }
	        }

	        int Get(object name) {
	            int slot = HashSlotFor(name);
	            // subtract 1, 0 stands for empty
	            return (hashTable[slot] & 0xff) - 1;
	        }

	        private int HashSlotFor(object element) {
	            return element.HasHCode & hashTable.Length - 1;
	        }

	    }

	}
} // end of namespace
