///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text.Json;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.json.parser.core;
using com.espertech.esper.common.@internal.@event.json.writer;
using com.espertech.esper.common.@internal.@event.property;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.json.core
{
	/// <summary>
	/// Implementation of the EventType interface for handling native-type classes.
	/// </summary>
	public class JsonEventType : BaseNestableEventType
	{
		private readonly JsonEventTypeDetail detail;

		private Type delegateType;
		private JsonDelegateFactory delegateFactory;
		private Type underlyingType;
		private EventPropertyDescriptor[] writablePropertyDescriptors;
		private IDictionary<string, Pair<EventPropertyDescriptor, JsonEventBeanPropertyWriter>> propertyWriters;

		public JsonEventType(
			EventTypeMetadata metadata,
			IDictionary<string, object> propertyTypes,
			EventType[] optionalSuperTypes,
			ISet<EventType> optionalDeepSupertypes,
			string startTimestampPropertyName,
			string endTimestampPropertyName,
			EventTypeNestableGetterFactory getterFactory,
			BeanEventTypeFactory beanEventTypeFactory,
			JsonEventTypeDetail detail,
			Type underlyingStandInClass)
			: base(
				metadata,
				propertyTypes,
				optionalSuperTypes,
				optionalDeepSupertypes,
				startTimestampPropertyName,
				endTimestampPropertyName,
				getterFactory,
				beanEventTypeFactory,
				true)
		{
			this.detail = detail;
			this.underlyingType = underlyingStandInClass;
		}

		public override EventPropertyWriterSPI GetWriter(string propertyName)
		{
			return GetInternalWriter(propertyName);
		}
		
		public JsonEventBeanPropertyWriter GetInternalWriter(string propertyName)
		{
			if (writablePropertyDescriptors == null) {
				InitializeWriters();
			}

			var pair = propertyWriters.Get(propertyName);
			if (pair != null) {
				return pair.Second;
			}

			var property = PropertyParser.ParseAndWalkLaxToSimple(propertyName);
			if (property is MappedProperty) {
				var mapProp = (MappedProperty) property;
				var field = detail.FieldDescriptors.Get(mapProp.PropertyNameAtomic);
				if (field == null) {
					return null;
				}

				return new JsonEventBeanPropertyWriterMapProp(this.delegateFactory, field, mapProp.Key);
			}

			if (property is IndexedProperty) {
				var indexedProp = (IndexedProperty) property;
				var field = detail.FieldDescriptors.Get(indexedProp.PropertyNameAtomic);
				if (field == null) {
					return null;
				}

				return new JsonEventBeanPropertyWriterIndexedProp(this.delegateFactory, field, indexedProp.Index);
			}

			return null;
		}

		public override EventPropertyDescriptor[] WriteableProperties {
			get {
				if (writablePropertyDescriptors == null) {
					InitializeWriters();
				}

				return writablePropertyDescriptors;
			}
		}

		public override EventPropertyDescriptor GetWritableProperty(string propertyName)
		{
			if (writablePropertyDescriptors == null) {
				InitializeWriters();
			}

			var pair = propertyWriters.Get(propertyName);
			if (pair != null) {
				return pair.First;
			}

			var property = PropertyParser.ParseAndWalkLaxToSimple(propertyName);
			if (property is MappedProperty) {
				EventPropertyWriter writer = GetWriter(propertyName);
				if (writer == null) {
					return null;
				}

				var mapProp = (MappedProperty) property;
				return new EventPropertyDescriptor(mapProp.PropertyNameAtomic, typeof(object), null, false, true, false, true, false);
			}

			if (property is IndexedProperty) {
				EventPropertyWriter writer = GetWriter(propertyName);
				if (writer == null) {
					return null;
				}

				var indexedProp = (IndexedProperty) property;
				return new EventPropertyDescriptor(indexedProp.PropertyNameAtomic, typeof(object), null, true, false, true, false, false);
			}

			return null;
		}

		public override EventBeanCopyMethodForge GetCopyMethodForge(string[] properties)
		{
			return new JsonEventBeanCopyMethodForge(this);
		}

		public override EventBeanWriter GetWriter(string[] properties)
		{
			if (writablePropertyDescriptors == null) {
				InitializeWriters();
			}

			var writers = new JsonEventBeanPropertyWriter[properties.Length];
			for (var i = 0; i < properties.Length; i++) {
				writers[i] = GetInternalWriter(properties[i]);
				if (writers[i] == null) {
					return null;
				}
			}

			return new JsonEventBeanWriterPerProp(writers);
		}

		public override Type UnderlyingType {
			get {
				if (underlyingType == null) {
					throw new EPException("Underlying type has not been set");
				}

				return underlyingType;
			}
		}

		public void Initialize(ClassLoader classLoader)
		{
			// resolve underlying type
			try {
				underlyingType = classLoader.GetClass(detail.UnderlyingClassName);
			}
			catch (TypeLoadException ex) {
				throw new EPException("Failed to load Json underlying class: " + ex.Message, ex);
			}

			// resolve delegate
			try {
				delegateType = classLoader.GetClass(detail.DelegateClassName);
			}
			catch (TypeLoadException e) {
				throw new EPException("Failed to find class: " + e.Message, e);
			}

			// resolve handler factory
			Type delegateFactory;
			try {
				delegateFactory = classLoader.GetClass(detail.DelegateFactoryClassName);
			}
			catch (TypeLoadException e) {
				throw new EPException("Failed to find class: " + e.Message, e);
			}

			this.delegateFactory = TypeHelper.Instantiate<JsonDelegateFactory>(delegateFactory);
		}

		public object Parse(string json)
		{
			try {
				var deserializer = delegateFactory.Make(null);

				var jsonDocumentOptions = new JsonDocumentOptions();
				var jsonDocument = JsonDocument.Parse(json, jsonDocumentOptions);
				
				deserializer.Deserialize(jsonDocument.RootElement);
				
				return deserializer.GetResult();
			}
			catch (EPException) {
				throw;
			}
			catch (Exception ex) {
				throw new EPException("Failed to parse Json: " + ex.Message, ex);
			}
		}

		public void ParseProperties(
			Utf8JsonReader jsonReader,
			JsonHandlerDelegator handler,
			object currentObject)
		{
			while (jsonReader.Read()) {
				switch (jsonReader.TokenType) {
					case JsonTokenType.None:
						break;
					case JsonTokenType.EndObject:
						handler.EndObject(currentObject);
						break;
					case JsonTokenType.PropertyName:
						handler.StartObjectValue(currentObject, jsonReader.GetString());
						break;

					case JsonTokenType.StartObject:
						ParseProperties(jsonReader, handler, handler.StartObject());
						break;

					case JsonTokenType.String:
						handler.EndString(jsonReader.GetString());
						break;
					case JsonTokenType.Number:
						ReadOnlySpan<byte> span = jsonReader.HasValueSequence 
							? jsonReader.ValueSequence.ToArray()
							: jsonReader.ValueSpan;
						handler.EndNumber(span);
						break;
					case JsonTokenType.True:
						handler.EndBoolean(true);
						break;
					case JsonTokenType.False:
						handler.EndBoolean(false);
						break;
					case JsonTokenType.Null:
						handler.EndNull();
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		public void ParseValue(
			Utf8JsonReader jsonReader,
			JsonHandlerDelegator handler)
		{
			while (jsonReader.Read()) {
				switch (jsonReader.TokenType) {
					case JsonTokenType.None:
						break;
					case JsonTokenType.StartObject:
						ParseProperties(jsonReader, handler, handler.StartObject());
						break;
					case JsonTokenType.StartArray:
						ParseArray(jsonReader, handler, handler.StartArray());
						break;
					case JsonTokenType.Comment:
						break;
					case JsonTokenType.String:
						handler.EndString(jsonReader.GetString());
						break;
					case JsonTokenType.Number:
						jsonReader.GetDouble();
						break;
					case JsonTokenType.True:
						handler.EndBoolean(true);
						break;
					case JsonTokenType.False:
						handler.EndBoolean(false);
						break;
					case JsonTokenType.Null:
						handler.EndNull();
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		public JsonEventTypeDetail Detail => detail;

		public Type DelegateType => delegateType;

		public JsonDelegateFactory DelegateFactory => delegateFactory;

		public int GetColumnNumber(string columnName)
		{
			var field = detail.FieldDescriptors.Get(columnName);
			if (field != null) {
				return field.PropertyNumber;
			}

			throw new IllegalStateException("Unrecognized json-type column name '" + columnName + "'");
		}

		public bool IsDeepEqualsConsiderOrder(JsonEventType other)
		{
			if (other.NestableTypes.Count != NestableTypes.Count) {
				return false;
			}

			foreach (var propMeEntry in NestableTypes) {
				var fieldMe = detail.FieldDescriptors.Get(propMeEntry.Key);
				var fieldOther = other.detail.FieldDescriptors.Get(propMeEntry.Key);
				if (fieldOther == null || fieldMe.PropertyNumber != fieldOther.PropertyNumber) {
					return false;
				}

				var propName = propMeEntry.Key;
				var setOneType = this.NestableTypes.Get(propName);
				var setTwoType = other.NestableTypes.Get(propName);
				var setTwoTypeFound = other.NestableTypes.ContainsKey(propName);

				var comparedMessage = BaseNestableEventUtil.ComparePropType(propName, setOneType, setTwoType, setTwoTypeFound, other.Name);
				if (comparedMessage != null) {
					return false;
				}
			}

			return true;
		}

		private void InitializeWriters()
		{
			var writeableProps = new List<EventPropertyDescriptor>();
			var propertWritersMap = new Dictionary<string, Pair<EventPropertyDescriptor, JsonEventBeanPropertyWriter>>();
			foreach (var prop in PropertyDescriptors) {
				var field = detail.FieldDescriptors.Get(prop.PropertyName);
				if (field == null) {
					continue;
				}

				writeableProps.Add(prop);
				var eventPropertyWriter = new JsonEventBeanPropertyWriter(this.delegateFactory, field);
				propertWritersMap.Put(
					prop.PropertyName,
					new Pair<EventPropertyDescriptor, JsonEventBeanPropertyWriter>(prop, eventPropertyWriter));
			}

			propertyWriters = propertWritersMap;
			writablePropertyDescriptors = writeableProps.ToArray();
		}
	}
} // end of namespace
