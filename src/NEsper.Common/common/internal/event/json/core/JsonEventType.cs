///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text.Json;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.json.serializers;
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
		
		// Type of the underlying json representation - usually an native object (class)
		private Type _underlyingType;
		// Indicates that the underlying type is transient and must be replaced.
		private bool _underlyingTypeIsTransient;
		// Type of the delegate
		private Type _delegateType;
		// Type of the deserializer
		private Type _deserializerType;
		// Type of the serializer
		private Type _serializerType;
		// Delegate instance
		private IJsonDelegate _delegate;
		// Deserializer instance
		private IJsonDeserializer _deserializer;
		// Serializer instance
		private IJsonSerializer _serializer;

		private EventPropertyDescriptor[] _writablePropertyDescriptors;
		private IDictionary<string, Pair<EventPropertyDescriptor, JsonEventBeanPropertyWriter>> _propertyWriters;
		
		public JsonEventTypeDetail Detail => _detail;

		public Type DeserializerType => _deserializerType;

		public IJsonDeserializer Deserializer => _deserializer;

		public Type SerializerType => _serializerType;

		public IJsonSerializer Serializer => _serializer;

		public IJsonDelegate Delegate => _delegate;
		
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
			Type underlyingStandInClass,
			bool underlyingTypeIsTransient)
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
			_underlyingTypeIsTransient = underlyingTypeIsTransient;
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
			switch (property) {
				case MappedProperty mapProp: {
					var field = _detail.FieldDescriptors.Get(mapProp.PropertyNameAtomic);
					return field != null
						? new JsonEventBeanPropertyWriterMapProp(_delegate, field, mapProp.Key)
						: null;
				}

				case IndexedProperty indexedProp: {
					var field = _detail.FieldDescriptors.Get(indexedProp.PropertyNameAtomic);
					return field != null 
						? new JsonEventBeanPropertyWriterIndexedProp(_delegate, field, indexedProp.Index)
						: null;
				}

				default:
					return null;
			}
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
			if (property is MappedProperty mapProp) {
				EventPropertyWriter writer = GetWriter(propertyName);
				return writer != null
					? new EventPropertyDescriptor(mapProp.PropertyNameAtomic, typeof(object), null, false, true, false, true, false) 
					: null;
			}

			if (property is IndexedProperty indexedProp) {
				EventPropertyWriter writer = GetWriter(propertyName);
				return writer != null
					? new EventPropertyDescriptor(indexedProp.PropertyNameAtomic, typeof(object), null, true, false, true, false, false)
					: null;
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

		public bool UnderlyingTypeIsTransient => _underlyingTypeIsTransient;

		public override Type UnderlyingType {
			get {
				if (_underlyingType == null) {
					throw new EPException("Underlying type has not been set");
				}
				return _underlyingType;
			}
		}
		
		public void Initialize(TypeResolver typeResolver)
		{
			// resolve underlying type
			try {
				_underlyingType = typeResolver.ResolveType(_detail.UnderlyingClassName, false);
			}
			catch (TypeLoadException ex) {
				throw new EPException("Failed to load Json underlying class: " + ex.Message, ex);
			}
			
			// resolve delegate
	        try {
	            _delegateType = typeResolver.ResolveType(_detail.DelegateClassName, false);
	            _delegate = TypeHelper.Instantiate<IJsonDelegate>(_delegateType);
	        }
	        catch (TypeLoadException e) {
	            throw new EPException("Failed to find class: " + e.Message, e);
	        }

			// resolve deserializer
			try {
				_deserializerType = typeResolver.ResolveType(_detail.DeserializerClassName, false);
				_deserializer = TypeHelper.Instantiate<IJsonDeserializer>(_deserializerType);
			}
			catch (TypeLoadException e) {
				throw new EPException("Failed to find class: " + e.Message, e);
			}

			// resolve serializer
			try {
				_serializerType = typeResolver.ResolveType(_detail.SerializerClassName, false);
				_serializer = TypeHelper.Instantiate<IJsonSerializer>(_serializerType);
			}
			catch (TypeLoadException e) {
				throw new EPException("Failed to find class: " + e.Message, e);
			}

			//_serializationContext = TypeHelper.Instantiate<JsonSerializationContext>(deserializerFactoryType);
		}

		public object Parse(string json)
		{
			var jsonDocumentOptions = new JsonDocumentOptions();
			var jsonDocument = JsonDocument.Parse(json, jsonDocumentOptions);
			return _deserializer.Deserialize(jsonDocument.RootElement);
		}

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
			var writeablePropsMap = new Dictionary<string, Pair<EventPropertyDescriptor, JsonEventBeanPropertyWriter>>();
			foreach (var prop in PropertyDescriptors) {
				var field = _detail.FieldDescriptors.Get(prop.PropertyName);
				if (field == null) {
					continue;
				}

				var eventPropertyWriter = new JsonEventBeanPropertyWriter(_delegate, field);

				writeableProps.Add(prop);
				writeablePropsMap.Put(
					prop.PropertyName,
					new Pair<EventPropertyDescriptor, JsonEventBeanPropertyWriter>(prop, eventPropertyWriter));
			}

			_propertyWriters = writeablePropsMap;
			_writablePropertyDescriptors = writeableProps.ToArray();
		}
	}
} // end of namespace
