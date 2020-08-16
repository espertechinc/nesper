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
using com.espertech.esper.common.@internal.@event.json.serde;
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
		private readonly JsonEventTypeDetail _detail;
		private Type _deserializerType;
		private JsonSerializationContext _serializationContext;
		private Type _underlyingType;
		private EventPropertyDescriptor[] _writablePropertyDescriptors;
		private IDictionary<string, Pair<EventPropertyDescriptor, JsonEventBeanPropertyWriter>> _propertyWriters;

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
			_detail = detail;
			_underlyingType = underlyingStandInClass;
		}

		public override EventPropertyWriterSPI GetWriter(string propertyName)
		{
			return GetInternalWriter(propertyName);
		}
		
		public JsonEventBeanPropertyWriter GetInternalWriter(string propertyName)
		{
			if (_writablePropertyDescriptors == null) {
				InitializeWriters();
			}

			var pair = _propertyWriters.Get(propertyName);
			if (pair != null) {
				return pair.Second;
			}

			var property = PropertyParser.ParseAndWalkLaxToSimple(propertyName);
			if (property is MappedProperty) {
				var mapProp = (MappedProperty) property;
				var field = _detail.FieldDescriptors.Get(mapProp.PropertyNameAtomic);
				if (field == null) {
					return null;
				}

				return new JsonEventBeanPropertyWriterMapProp(this._serializationContext, field, mapProp.Key);
			}

			if (property is IndexedProperty) {
				var indexedProp = (IndexedProperty) property;
				var field = _detail.FieldDescriptors.Get(indexedProp.PropertyNameAtomic);
				if (field == null) {
					return null;
				}

				return new JsonEventBeanPropertyWriterIndexedProp(this._serializationContext, field, indexedProp.Index);
			}

			return null;
		}

		public override EventPropertyDescriptor[] WriteableProperties {
			get {
				if (_writablePropertyDescriptors == null) {
					InitializeWriters();
				}

				return _writablePropertyDescriptors;
			}
		}

		public override EventPropertyDescriptor GetWritableProperty(string propertyName)
		{
			if (_writablePropertyDescriptors == null) {
				InitializeWriters();
			}

			var pair = _propertyWriters.Get(propertyName);
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
			if (_writablePropertyDescriptors == null) {
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
				if (_underlyingType == null) {
					throw new EPException("Underlying type has not been set");
				}

				return _underlyingType;
			}
		}

		public void Initialize(ClassLoader classLoader)
		{
			// resolve underlying type
			try {
				_underlyingType = classLoader.GetClass(_detail.UnderlyingClassName);
			}
			catch (TypeLoadException ex) {
				throw new EPException("Failed to load Json underlying class: " + ex.Message, ex);
			}

			// resolve delegate
			try {
				_deserializerType = classLoader.GetClass(_detail.DeserializerClassName);
			}
			catch (TypeLoadException e) {
				throw new EPException("Failed to find class: " + e.Message, e);
			}

			// resolve handler factory
			Type deserializerFactoryType;
			try {
				deserializerFactoryType = classLoader.GetClass(_detail.DeserializerFactoryClassName);
			}
			catch (TypeLoadException e) {
				throw new EPException("Failed to find class: " + e.Message, e);
			}

			_serializationContext = TypeHelper.Instantiate<JsonSerializationContext>(deserializerFactoryType);
		}

		public object Parse(string json)
		{
			try {
				var deserializer = _serializationContext.Deserializer;

				var jsonDocumentOptions = new JsonDocumentOptions();
				var jsonDocument = JsonDocument.Parse(json, jsonDocumentOptions);
				
				return deserializer.Invoke(jsonDocument.RootElement);
			}
			catch (EPException) {
				throw;
			}
			catch (Exception ex) {
				throw new EPException("Failed to parse Json: " + ex.Message, ex);
			}
		}

		public JsonEventTypeDetail Detail => _detail;

		public Type DeserializerType => _deserializerType;

		public JsonSerializationContext SerializationContext => _serializationContext;

		public bool IsDeepEqualsConsiderOrder(JsonEventType other)
		{
			if (other.NestableTypes.Count != NestableTypes.Count) {
				return false;
			}

			foreach (var propMeEntry in NestableTypes) {
				var fieldMe = _detail.FieldDescriptors.Get(propMeEntry.Key);
				var fieldOther = other._detail.FieldDescriptors.Get(propMeEntry.Key);
				if (fieldOther == null || fieldMe.FieldName != fieldOther.FieldName) {
					return false;
				}

				var propName = propMeEntry.Key;
				var setOneType = this.NestableTypes.Get(propName);
				var setTwoType = other.NestableTypes.Get(propName);
				var setTwoTypeFound = other.NestableTypes.ContainsKey(propName);

				var comparedMessage = BaseNestableEventUtil.ComparePropType(
					propName,
					setOneType,
					setTwoType,
					setTwoTypeFound,
					other.Name);

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
				var field = _detail.FieldDescriptors.Get(prop.PropertyName);
				if (field == null) {
					continue;
				}

				writeableProps.Add(prop);
				var eventPropertyWriter = new JsonEventBeanPropertyWriter(this._serializationContext, field);
				propertWritersMap.Put(
					prop.PropertyName,
					new Pair<EventPropertyDescriptor, JsonEventBeanPropertyWriter>(prop, eventPropertyWriter));
			}

			_propertyWriters = propertWritersMap;
			_writablePropertyDescriptors = writeableProps.ToArray();
		}
	}
} // end of namespace
