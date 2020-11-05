///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.json.compiletime;
using com.espertech.esper.common.@internal.@event.json.getter.core;
using com.espertech.esper.common.@internal.@event.json.getter.fromschema;
using com.espertech.esper.common.@internal.@event.json.getter.provided;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.common.@internal.@event.property;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.json.core
{
	public class EventTypeNestableGetterFactoryJson : EventTypeNestableGetterFactory
	{
		private readonly JsonEventTypeDetail _detail;

		public EventTypeNestableGetterFactoryJson(JsonEventTypeDetail detail)
		{
			this._detail = detail;
		}

		public EventPropertyGetterSPI GetPropertyDynamicGetter(
			IDictionary<string, object> nestableTypes,
			string propertyExpression,
			DynamicProperty prop,
			EventBeanTypedEventFactory eventBeanTypedEventFactory,
			BeanEventTypeFactory beanEventTypeFactory)
		{
			var type = nestableTypes.Get(prop.PropertyNameAtomic);
			if (type == null && _detail.IsDynamic) { // we do not know this property
				if (prop is PropertySimple) {
					return new JsonGetterDynamicSimpleSchema(prop.PropertyNameAtomic);
				}

				if (prop is DynamicIndexedProperty dynamicIndexedProperty) {
					return new JsonGetterDynamicIndexedSchema(dynamicIndexedProperty.PropertyNameAtomic, dynamicIndexedProperty.Index);
				}

				if (prop is DynamicMappedProperty mappedProperty) {
					return new JsonGetterDynamicMappedSchema(mappedProperty.PropertyNameAtomic, mappedProperty.Key);
				}

				throw new IllegalStateException("Unrecognized dynamic property " + prop);
			}

			// we do know this property
			if (prop is DynamicSimpleProperty) {
				if (type == null) {
					return null;
				}

				if (type is Type) {
					return GetGetterProperty(prop.PropertyNameAtomic, null, eventBeanTypedEventFactory);
				}

				if (type is TypeBeanOrUnderlying typeBeanOrUnderlying) {
					var eventType = typeBeanOrUnderlying.EventType;
					return GetGetterBeanNested(prop.PropertyNameAtomic, eventType, eventBeanTypedEventFactory);
				}

				return null;
			}

			if (prop is DynamicIndexedProperty indexed) {
				if (type == null) {
					return null;
				}

				if (type is Type asType && asType.IsArray) {
					return GetGetterIndexedClassArray(
						prop.PropertyNameAtomic,
						indexed.Index,
						eventBeanTypedEventFactory,
						asType.GetElementType(),
						beanEventTypeFactory);
				}

				if (type is TypeBeanOrUnderlying[]) {
					return GetGetterIndexedUnderlyingArray(prop.PropertyNameAtomic, indexed.Index, eventBeanTypedEventFactory, null, beanEventTypeFactory);
				}

				return null;
			}

			if (prop is DynamicMappedProperty dynamicMappedProperty) {
				if (type == null) {
					return null;
				}

				if (type is IDictionary<string, object>) {
					return GetGetterMappedProperty(prop.PropertyNameAtomic, dynamicMappedProperty.Key);
				}

				var asType = type as Type;
				if (asType != null && asType.IsGenericStringDictionary()) {
					return GetGetterMappedProperty(prop.PropertyNameAtomic, dynamicMappedProperty.Key);
				}

				return null;
			}

			return null;
		}

		public EventPropertyGetterMappedSPI GetPropertyProvidedGetterMap(
			IDictionary<string, object> nestableTypes,
			string mappedPropertyName,
			MappedProperty mappedProperty,
			EventBeanTypedEventFactory eventBeanTypedEventFactory,
			BeanEventTypeFactory beanEventTypeFactory)
		{
			var field = FindField(mappedPropertyName);
			if (field == null) {
				return null;
			}

			if (field.OptionalField != null) {
				return new JsonGetterMapRuntimeKeyedProvided(field.OptionalField);
			}

			return new JsonGetterMapRuntimeKeyedSchema(field);
		}

		public EventPropertyGetterIndexedSPI GetPropertyProvidedGetterIndexed(
			IDictionary<string, object> nestableTypes,
			string indexedPropertyName,
			IndexedProperty indexedProperty,
			EventBeanTypedEventFactory eventBeanTypedEventFactory,
			BeanEventTypeFactory beanEventTypeFactory)
		{
			var field = FindField(indexedPropertyName);
			if (field == null) {
				return null;
			}

			if (field.OptionalField != null) {
				return new JsonGetterIndexedRuntimeIndexProvided(field.OptionalField);
			}

			return new JsonGetterIndexedRuntimeIndexSchema(field);
		}

		public EventPropertyGetterSPI GetGetterProperty(
			string name,
			BeanEventType nativeFragmentType,
			EventBeanTypedEventFactory eventBeanTypedEventFactory)
		{
			var field = FindField(name);
			if (field == null) {
				return null;
			}

			if (field.OptionalField != null) {
				if (field.OptionalField.FieldType.IsArray) {
					return new JsonGetterSimpleProvidedWFragmentArray(field.OptionalField, nativeFragmentType, eventBeanTypedEventFactory);
				}

				return new JsonGetterSimpleProvidedWFragmentSimple(field.OptionalField, nativeFragmentType, eventBeanTypedEventFactory);
			}

			return new JsonGetterSimpleSchemaWFragment(field, _detail.UnderlyingClassName, null, eventBeanTypedEventFactory);
		}

		public EventPropertyGetterSPI GetGetterEventBean(
			string name,
			Type underlyingType)
		{
			throw MakeIllegalState();
		}

		public EventPropertyGetterSPI GetGetterEventBeanArray(
			string name,
			EventType eventType)
		{
			throw MakeIllegalState();
		}

		public EventPropertyGetterSPI GetGetterBeanNested(
			string name,
			EventType eventType,
			EventBeanTypedEventFactory eventBeanTypedEventFactory)
		{
			var field = FindField(name);
			if (field == null) {
				return null;
			}

			if (field.OptionalField != null) {
				return new JsonGetterSimpleProvidedWFragmentSimple(field.OptionalField, eventType, eventBeanTypedEventFactory);
			}

			return new JsonGetterSimpleSchemaWFragment(field, _detail.UnderlyingClassName, eventType, eventBeanTypedEventFactory);
		}

		public EventPropertyGetterSPI GetGetterBeanNestedArray(
			string name,
			EventType eventType,
			EventBeanTypedEventFactory eventBeanTypedEventFactory)
		{
			var field = FindField(name);
			if (field == null) {
				return null;
			}

			if (field.OptionalField != null) {
				return new JsonGetterSimpleProvidedWFragmentArray(field.OptionalField, eventType, eventBeanTypedEventFactory);
			}

			return new JsonGetterSimpleSchemaWFragmentArray(field, _detail.UnderlyingClassName, eventType, eventBeanTypedEventFactory);
		}

		public EventPropertyGetterSPI GetGetterIndexedEventBean(
			string propertyNameAtomic,
			int index)
		{
			throw MakeIllegalState();
		}

		public EventPropertyGetterSPI GetGetterIndexedUnderlyingArray(
			string propertyNameAtomic,
			int index,
			EventBeanTypedEventFactory eventBeanTypedEventFactory,
			EventType innerType,
			BeanEventTypeFactory beanEventTypeFactory)
		{
			var field = FindField(propertyNameAtomic);
			if (field == null) {
				return null;
			}

			if (field.OptionalField != null) {
				return new JsonGetterIndexedProvided(index, _detail.UnderlyingClassName, innerType, eventBeanTypedEventFactory, field.OptionalField);
			}

			return new JsonGetterIndexedSchema(index, _detail.UnderlyingClassName, innerType, eventBeanTypedEventFactory, field);
		}

		public EventPropertyGetterSPI GetGetterIndexedClassArray(
			string propertyNameAtomic,
			int index,
			EventBeanTypedEventFactory eventBeanTypedEventFactory,
			Type componentType,
			BeanEventTypeFactory beanEventTypeFactory)
		{
			var field = FindField(propertyNameAtomic);
			if (field == null) {
				return null;
			}

			if (field.OptionalField != null) {
				return new JsonGetterIndexedProvidedBaseNative(eventBeanTypedEventFactory, beanEventTypeFactory, componentType, field.OptionalField, index);
			}

			return new JsonGetterIndexedSchema(index, _detail.UnderlyingClassName, null, eventBeanTypedEventFactory, field);
		}

		public EventPropertyGetterSPI GetGetterMappedProperty(
			string propertyNameAtomic,
			string key)
		{
			var field = FindField(propertyNameAtomic);
			if (field == null) {
				return null;
			}

			if (field.OptionalField != null) {
				return new JsonGetterMappedProvided(key, _detail.UnderlyingClassName, field.OptionalField);
			}

			return new JsonGetterMappedSchema(key, _detail.UnderlyingClassName, field);
		}

		public EventPropertyGetterSPI GetGetterNestedEntryBeanArray(
			string propertyNameAtomic,
			int index,
			EventPropertyGetter getter,
			EventType innerType,
			EventBeanTypedEventFactory eventBeanTypedEventFactory)
		{
			var field = FindField(propertyNameAtomic);
			if (field == null) {
				return null;
			}

			if (field.OptionalField != null) {
				return new JsonGetterNestedArrayIndexedProvided(index, (JsonEventPropertyGetter) getter, _detail.UnderlyingClassName, field.OptionalField);
			}

			return new JsonGetterNestedArrayIndexedSchema(index, (JsonEventPropertyGetter) getter, _detail.UnderlyingClassName, field);
		}

		public EventPropertyGetterSPI GetGetterIndexedEntryEventBeanArrayElement(
			string propertyNameAtomic,
			int index,
			EventPropertyGetterSPI nestedGetter)
		{
			throw MakeIllegalState();
		}

		public EventPropertyGetterSPI GetGetterIndexedEntryPONO(
			string propertyNameAtomic,
			int index,
			BeanEventPropertyGetter nestedGetter,
			EventBeanTypedEventFactory eventBeanTypedEventFactory,
			BeanEventTypeFactory beanEventTypeFactory,
			Type propertyTypeGetter)
		{
			var field = FindField(propertyNameAtomic);
			if (field.OptionalField == null) {
				throw MakeIllegalState();
			}

			return new JsonGetterIndexedEntryPONOProvided(
				field.OptionalField,
				index,
				nestedGetter,
				eventBeanTypedEventFactory,
				beanEventTypeFactory,
				propertyTypeGetter);
		}

		public EventPropertyGetterSPI GetGetterNestedMapProp(
			string propertyName,
			MapEventPropertyGetter getterNestedMap)
		{
			return null;
		}

		public EventPropertyGetterSPI GetGetterNestedPONOProp(
			string propertyName,
			BeanEventPropertyGetter nestedGetter,
			EventBeanTypedEventFactory eventBeanTypedEventFactory,
			BeanEventTypeFactory beanEventTypeFactory,
			Type nestedReturnType,
			Type nestedComponentType)
		{
			var field = FindField(propertyName);
			if (field == null || field.OptionalField == null) {
				return null;
			}

			return new JsonGetterNestedPONOPropProvided(
				eventBeanTypedEventFactory,
				beanEventTypeFactory,
				nestedReturnType,
				nestedComponentType,
				field.OptionalField,
				nestedGetter);
		}

		public EventPropertyGetterSPI GetGetterNestedEventBean(
			string propertyName,
			EventPropertyGetterSPI nestedGetter)
		{
			return null;
		}

		public EventPropertyGetterSPI GetGetterNestedPropertyProvidedGetterDynamic(
			IDictionary<string, object> nestableTypes,
			string propertyName,
			EventPropertyGetter nestedGetter,
			EventBeanTypedEventFactory eventBeanTypedEventFactory)
		{
			return new JsonGetterDynamicNestedSchema(propertyName, (JsonEventPropertyGetter) nestedGetter, _detail.UnderlyingClassName);
		}

		public EventPropertyGetterSPI GetGetterNestedEntryBean(
			string propertyName,
			EventPropertyGetter innerGetter,
			EventType innerType,
			EventBeanTypedEventFactory eventBeanTypedEventFactory)
		{
			var field = FindField(propertyName);
			if (field == null) {
				return null;
			}

			if (field.OptionalField != null) {
				return new JsonGetterNestedProvided((JsonEventPropertyGetter) innerGetter, _detail.UnderlyingClassName, field.OptionalField);
			}

			return new JsonGetterNestedSchema((JsonEventPropertyGetter) innerGetter, _detail.UnderlyingClassName, field);
		}

		public EventPropertyGetterSPI GetGetterRootedDynamicNested(
			Property prop,
			EventBeanTypedEventFactory eventBeanTypedEventFactory,
			BeanEventTypeFactory beanEventTypeFactory)
		{
			return GetGetterRootedDynamicNestedInternal(
				prop,
				eventBeanTypedEventFactory,
				beanEventTypeFactory);
		}

		public JsonEventPropertyGetter GetGetterRootedDynamicNestedInternal(
			Property prop,
			EventBeanTypedEventFactory eventBeanTypedEventFactory,
			BeanEventTypeFactory beanEventTypeFactory)
		{
			if (prop is DynamicSimpleProperty) {
				return new JsonGetterDynamicSimpleSchema(prop.PropertyNameAtomic);
			}
			else if (prop is DynamicIndexedProperty indexed) {
				return new JsonGetterDynamicIndexedSchema(indexed.PropertyNameAtomic, indexed.Index);
			}
			else if (prop is DynamicMappedProperty mapped) {
				return new JsonGetterDynamicMappedSchema(mapped.PropertyNameAtomic, mapped.Key);
			}
			else if (prop is NestedProperty nested) {
				var getters = new JsonEventPropertyGetter[nested.Properties.Count];
				for (var i = 0; i < nested.Properties.Count; i++) {
					getters[i] = GetGetterRootedDynamicNestedInternal(
						nested.Properties[i],
						eventBeanTypedEventFactory,
						beanEventTypeFactory);
				}

				return new JsonGetterDynamicNestedChain(_detail.UnderlyingClassName, getters);
			}
			else {
				throw new IllegalStateException("Rerecognized dynamic property " + prop);
			}
		}

		private JsonUnderlyingField FindField(string name)
		{
			return _detail.FieldDescriptors.Get(name);
		}

		private IllegalStateException MakeIllegalState()
		{
			return new IllegalStateException("An implementation of this getter is not available for Json event types");
		}
	}
} // end of namespace
