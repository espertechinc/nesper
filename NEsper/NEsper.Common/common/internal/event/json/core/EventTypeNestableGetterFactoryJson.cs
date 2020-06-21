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

		private readonly JsonEventTypeDetail detail;

		public EventTypeNestableGetterFactoryJson(JsonEventTypeDetail detail)
		{
			this.detail = detail;
		}

		public EventPropertyGetterSPI GetPropertyDynamicGetter(
			IDictionary<string, object> nestableTypes,
			string propertyExpression,
			DynamicProperty prop,
			EventBeanTypedEventFactory eventBeanTypedEventFactory,
			BeanEventTypeFactory beanEventTypeFactory)
		{
			object type = nestableTypes.Get(prop.PropertyNameAtomic);
			if (type == null && detail.IsDynamic) { // we do not know this property
				if (prop is PropertySimple) {
					return new JsonGetterDynamicSimpleSchema(prop.PropertyNameAtomic);
				}

				if (prop is DynamicIndexedProperty) {
					DynamicIndexedProperty indexed = (DynamicIndexedProperty) prop;
					return new JsonGetterDynamicIndexedSchema(indexed.PropertyNameAtomic, indexed.Index);
				}

				if (prop is DynamicMappedProperty) {
					DynamicMappedProperty mapped = (DynamicMappedProperty) prop;
					return new JsonGetterDynamicMappedSchema(mapped.PropertyNameAtomic, mapped.Key);
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

				if (type is TypeBeanOrUnderlying) {
					EventType eventType = ((TypeBeanOrUnderlying) type).EventType;
					return GetGetterBeanNested(prop.PropertyNameAtomic, eventType, eventBeanTypedEventFactory);
				}

				return null;
			}

			if (prop is DynamicIndexedProperty) {
				DynamicIndexedProperty indexed = (DynamicIndexedProperty) prop;
				if (type == null) {
					return null;
				}

				if (type is Type && ((Type) type).IsArray) {
					return GetGetterIndexedClassArray(
						prop.PropertyNameAtomic,
						indexed.Index,
						eventBeanTypedEventFactory,
						((Type) type).ComponentType,
						beanEventTypeFactory);
				}

				if (type is TypeBeanOrUnderlying[]) {
					return GetGetterIndexedUnderlyingArray(prop.PropertyNameAtomic, indexed.Index, eventBeanTypedEventFactory, null, beanEventTypeFactory);
				}

				return null;
			}

			if (prop is DynamicMappedProperty) {
				DynamicMappedProperty mapped = (DynamicMappedProperty) prop;
				if (type == null) {
					return null;
				}

				if (type is IDictionary || type == typeof(IDictionary)) {
					return GetGetterMappedProperty(prop.PropertyNameAtomic, mapped.Key);
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
			JsonUnderlyingField field = FindField(mappedPropertyName);
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
			JsonUnderlyingField field = FindField(indexedPropertyName);
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
			JsonUnderlyingField field = FindField(name);
			if (field == null) {
				return null;
			}

			if (field.OptionalField != null) {
				if (field.OptionalField.Type.IsArray) {
					return new JsonGetterSimpleProvidedWFragmentArray(field.OptionalField, nativeFragmentType, eventBeanTypedEventFactory);
				}

				return new JsonGetterSimpleProvidedWFragmentSimple(field.OptionalField, nativeFragmentType, eventBeanTypedEventFactory);
			}

			return new JsonGetterSimpleSchemaWFragment(field, detail.UnderlyingClassName, null, eventBeanTypedEventFactory);
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
			JsonUnderlyingField field = FindField(name);
			if (field == null) {
				return null;
			}

			if (field.OptionalField != null) {
				return new JsonGetterSimpleProvidedWFragmentSimple(field.OptionalField, eventType, eventBeanTypedEventFactory);
			}

			return new JsonGetterSimpleSchemaWFragment(field, detail.UnderlyingClassName, eventType, eventBeanTypedEventFactory);
		}

		public EventPropertyGetterSPI GetGetterBeanNestedArray(
			string name,
			EventType eventType,
			EventBeanTypedEventFactory eventBeanTypedEventFactory)
		{
			JsonUnderlyingField field = FindField(name);
			if (field == null) {
				return null;
			}

			if (field.OptionalField != null) {
				return new JsonGetterSimpleProvidedWFragmentArray(field.OptionalField, eventType, eventBeanTypedEventFactory);
			}

			return new JsonGetterSimpleSchemaWFragmentArray(field, detail.UnderlyingClassName, eventType, eventBeanTypedEventFactory);
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
			JsonUnderlyingField field = FindField(propertyNameAtomic);
			if (field == null) {
				return null;
			}

			if (field.OptionalField != null) {
				return new JsonGetterIndexedProvided(index, detail.UnderlyingClassName, innerType, eventBeanTypedEventFactory, field.OptionalField);
			}

			return new JsonGetterIndexedSchema(index, detail.UnderlyingClassName, innerType, eventBeanTypedEventFactory, field);
		}

		public EventPropertyGetterSPI GetGetterIndexedClassArray(
			string propertyNameAtomic,
			int index,
			EventBeanTypedEventFactory eventBeanTypedEventFactory,
			Type componentType,
			BeanEventTypeFactory beanEventTypeFactory)
		{
			JsonUnderlyingField field = FindField(propertyNameAtomic);
			if (field == null) {
				return null;
			}

			if (field.OptionalField != null) {
				return new JsonGetterIndexedProvidedBaseNative(eventBeanTypedEventFactory, beanEventTypeFactory, componentType, field.OptionalField, index);
			}

			return new JsonGetterIndexedSchema(index, detail.UnderlyingClassName, null, eventBeanTypedEventFactory, field);
		}

		public EventPropertyGetterSPI GetGetterMappedProperty(
			string propertyNameAtomic,
			string key)
		{
			JsonUnderlyingField field = FindField(propertyNameAtomic);
			if (field == null) {
				return null;
			}

			if (field.OptionalField != null) {
				return new JsonGetterMappedProvided(key, detail.UnderlyingClassName, field.OptionalField);
			}

			return new JsonGetterMappedSchema(key, detail.UnderlyingClassName, field);
		}

		public EventPropertyGetterSPI GetGetterNestedEntryBeanArray(
			string propertyNameAtomic,
			int index,
			EventPropertyGetter getter,
			EventType innerType,
			EventBeanTypedEventFactory eventBeanTypedEventFactory)
		{
			JsonUnderlyingField field = FindField(propertyNameAtomic);
			if (field == null) {
				return null;
			}

			if (field.OptionalField != null) {
				return new JsonGetterNestedArrayIndexedProvided(index, (JsonEventPropertyGetter) getter, detail.UnderlyingClassName, field.OptionalField);
			}

			return new JsonGetterNestedArrayIndexedSchema(index, (JsonEventPropertyGetter) getter, detail.UnderlyingClassName, field);
		}

		public EventPropertyGetterSPI GetGetterIndexedEntryEventBeanArrayElement(
			string propertyNameAtomic,
			int index,
			EventPropertyGetterSPI nestedGetter)
		{
			throw MakeIllegalState();
		}

		public EventPropertyGetterSPI GetGetterIndexedEntryPOJO(
			string propertyNameAtomic,
			int index,
			BeanEventPropertyGetter nestedGetter,
			EventBeanTypedEventFactory eventBeanTypedEventFactory,
			BeanEventTypeFactory beanEventTypeFactory,
			Type propertyTypeGetter)
		{
			JsonUnderlyingField field = FindField(propertyNameAtomic);
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

		public EventPropertyGetterSPI GetGetterNestedPOJOProp(
			string propertyName,
			BeanEventPropertyGetter nestedGetter,
			EventBeanTypedEventFactory eventBeanTypedEventFactory,
			BeanEventTypeFactory beanEventTypeFactory,
			Type nestedReturnType,
			Type nestedComponentType)
		{
			JsonUnderlyingField field = FindField(propertyName);
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
			return new JsonGetterDynamicNestedSchema(propertyName, (JsonEventPropertyGetter) nestedGetter, detail.UnderlyingClassName);
		}

		public EventPropertyGetterSPI GetGetterNestedEntryBean(
			string propertyName,
			EventPropertyGetter innerGetter,
			EventType innerType,
			EventBeanTypedEventFactory eventBeanTypedEventFactory)
		{
			JsonUnderlyingField field = FindField(propertyName);
			if (field == null) {
				return null;
			}

			if (field.OptionalField != null) {
				return new JsonGetterNestedProvided((JsonEventPropertyGetter) innerGetter, detail.UnderlyingClassName, field.OptionalField);
			}

			return new JsonGetterNestedSchema((JsonEventPropertyGetter) innerGetter, detail.UnderlyingClassName, field);
		}

		public JsonEventPropertyGetter GetGetterRootedDynamicNested(
			Property prop,
			EventBeanTypedEventFactory eventBeanTypedEventFactory,
			BeanEventTypeFactory beanEventTypeFactory)
		{
			if (prop is DynamicSimpleProperty) {
				return new JsonGetterDynamicSimpleSchema(prop.PropertyNameAtomic);
			}
			else if (prop is DynamicIndexedProperty) {
				DynamicIndexedProperty indexed = (DynamicIndexedProperty) prop;
				return new JsonGetterDynamicIndexedSchema(indexed.PropertyNameAtomic, indexed.Index);
			}
			else if (prop is DynamicMappedProperty) {
				DynamicMappedProperty mapped = (DynamicMappedProperty) prop;
				return new JsonGetterDynamicMappedSchema(mapped.PropertyNameAtomic, mapped.Key);
			}
			else if (prop is NestedProperty) {
				NestedProperty nested = (NestedProperty) prop;
				JsonEventPropertyGetter[] getters = new JsonEventPropertyGetter[nested.Properties.Count];
				for (int i = 0; i < nested.Properties.Count; i++) {
					getters[i] = GetGetterRootedDynamicNested(nested.Properties.Get(i), eventBeanTypedEventFactory, beanEventTypeFactory);
				}

				return new JsonGetterDynamicNestedChain(detail.UnderlyingClassName, getters);
			}
			else {
				throw new IllegalStateException("Rerecognized dynamic property " + prop);
			}
		}

		private JsonUnderlyingField FindField(string name)
		{
			return detail.FieldDescriptors.Get(name);
		}

		private IllegalStateException MakeIllegalState()
		{
			return new IllegalStateException("An implementation of this getter is not available for Json event types");
		}
	}
} // end of namespace
