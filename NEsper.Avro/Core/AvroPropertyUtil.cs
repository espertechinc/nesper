///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Avro;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.avro;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.property;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using NEsper.Avro.Extensions;
using NEsper.Avro.Getter;

namespace NEsper.Avro.Core
{
    public class AvroPropertyUtil
    {
        public static Type PropertyType(
            Schema fieldSchema,
            Property property)
        {
            var desc = AvroFieldUtil.FieldForProperty(fieldSchema, property);
            if (desc == null) {
                return null;
            }

            if (desc.IsDynamic) {
                return typeof(object);
            }

            Schema typeSchema = desc.Field.Schema;
            if (desc.IsAccessedByIndex) {
                typeSchema = desc.Field.Schema.AsArraySchema().ItemSchema;
            }
            else if (desc.IsAccessedByKey) {
                typeSchema = desc.Field.Schema.AsMapSchema().ValueSchema;
            }

            return AvroTypeUtil.PropertyType(typeSchema);
        }

        public static EventPropertyGetterSPI GetGetter(
            Schema avroSchema,
            string moduleName,
            Dictionary<string, EventPropertyGetterSPI> propertyGetterCache,
            IDictionary<string, PropertySetDescriptorItem> propertyDescriptors,
            string propertyName,
            bool addToCache,
            EventBeanTypedEventFactory eventAdapterService,
            EventTypeAvroHandler eventTypeAvroHandler,
            AvroEventTypeFragmentTypeCache fragmentTypeCache)
        {
            var getter = propertyGetterCache.Get(propertyName);
            if (getter != null) {
                return getter;
            }

            var unescapePropName = StringValue.UnescapeDot(propertyName);
            var item = propertyDescriptors.Get(unescapePropName);
            if (item != null) {
                getter = item.PropertyGetter;
                MayAddToGetterCache(propertyName, propertyGetterCache, getter, true);
                return getter;
            }

            // see if this is a nested property
            var index = StringValue.UnescapedIndexOfDot(propertyName);
            if (index == -1) {
                var prop = PropertyParser.ParseAndWalkLaxToSimple(propertyName);
                if (prop is IndexedProperty indexedProp) {
                    Field field = avroSchema.GetField(indexedProp.PropertyNameAtomic);
                    if (field == null || field.Schema.Tag != Schema.Type.Array) {
                        return null;
                    }

                    var fragmentEventType = AvroFragmentTypeUtil.GetFragmentEventTypeForField(
                        field.Schema,
                        moduleName,
                        eventAdapterService,
                        eventTypeAvroHandler,
                        fragmentTypeCache);
                    getter = new AvroEventBeanGetterIndexed(
                        field,
                        indexedProp.Index,
                        fragmentEventType?.FragmentType,
                        eventAdapterService);
                    MayAddToGetterCache(propertyName, propertyGetterCache, getter, addToCache);
                    return getter;
                }

                if (prop is MappedProperty mappedProp) {
                    Field field = avroSchema.GetField(mappedProp.PropertyNameAtomic);
                    if (field == null || field.Schema.Tag != Schema.Type.Map) {
                        return null;
                    }

                    getter = new AvroEventBeanGetterMapped(field, mappedProp.Key);
                    MayAddToGetterCache(propertyName, propertyGetterCache, getter, addToCache);
                    return getter;
                }

                if (prop is DynamicIndexedProperty dynamicIndexedProp) {
                    getter = new AvroEventBeanGetterIndexedDynamic(
                        dynamicIndexedProp.PropertyNameAtomic,
                        dynamicIndexedProp.Index);
                    MayAddToGetterCache(propertyName, propertyGetterCache, getter, addToCache);
                    return getter;
                }

                if (prop is DynamicMappedProperty dynamicMappedProp) {
                    getter = new AvroEventBeanGetterMappedDynamic(
                        dynamicMappedProp.PropertyNameAtomic,
                        dynamicMappedProp.Key);
                    MayAddToGetterCache(propertyName, propertyGetterCache, getter, addToCache);
                    return getter;
                }

                if (prop is DynamicSimpleProperty) {
                    getter = new AvroEventBeanGetterSimpleDynamic(prop.PropertyNameAtomic);
                    MayAddToGetterCache(propertyName, propertyGetterCache, getter, addToCache);
                    return getter;
                }

                return null; // simple property already cached
            }

            // Take apart the nested property into a map key and a nested value class property name
            var propertyTop = StringValue.UnescapeDot(propertyName.Substring(0, index));
            var propertyNested = propertyName.Substring(index + 1);
            var isRootedDynamic = false;

            // If the property is dynamic, remove the ? since the property type is defined without
            if (propertyTop.EndsWith("?")) {
                propertyTop = propertyTop.Substring(0, propertyTop.Length - 1);
                isRootedDynamic = true;
            }

            var propTop = PropertyParser.ParseAndWalkLaxToSimple(propertyTop);
            Field fieldTop = avroSchema.GetField(propTop.PropertyNameAtomic);

            // field is known and is a record
            if (fieldTop != null && fieldTop.Schema.Tag == Schema.Type.Record && propTop is SimpleProperty) {
                var factory = new GetterNestedFactoryRootedSimple(eventAdapterService, fieldTop);
                var property = PropertyParser.ParseAndWalk(propertyNested, isRootedDynamic);
                getter = PropertyGetterNested(
                    factory,
                    fieldTop.Schema,
                    property,
                    moduleName,
                    eventAdapterService,
                    eventTypeAvroHandler,
                    fragmentTypeCache);
                MayAddToGetterCache(propertyName, propertyGetterCache, getter, addToCache);
                return getter;
            }

            // field is known and is a record
            if (fieldTop != null &&
                fieldTop.Schema.Tag == Schema.Type.Array &&
                propTop is IndexedProperty indexedProperty) {
                var factory = new GetterNestedFactoryRootedIndexed(
                    eventAdapterService,
                    fieldTop,
                    indexedProperty.Index);
                var property = PropertyParser.ParseAndWalk(propertyNested, isRootedDynamic);
                getter = PropertyGetterNested(
                    factory,
                    fieldTop.Schema.AsArraySchema().ItemSchema,
                    property,
                    moduleName,
                    eventAdapterService,
                    eventTypeAvroHandler,
                    fragmentTypeCache);
                MayAddToGetterCache(propertyName, propertyGetterCache, getter, addToCache);
                return getter;
            }

            // field is not known or is not a record
            if (!isRootedDynamic) {
                return null;
            }

            var propertyX = PropertyParser.ParseAndWalk(propertyNested, true);
            var innerGetter = GetDynamicGetter(propertyX);
            getter = new AvroEventBeanGetterNestedDynamicPoly(propertyTop, innerGetter);
            MayAddToGetterCache(propertyName, propertyGetterCache, getter, addToCache);
            return getter;
        }

        private static EventPropertyGetterSPI PropertyGetterNested(
            GetterNestedFactory factory,
            Schema fieldSchema,
            Property property,
            string moduleName,
            EventBeanTypedEventFactory eventAdapterService,
            EventTypeAvroHandler eventTypeAvroHandler,
            AvroEventTypeFragmentTypeCache fragmentTypeCache)
        {
            if (property is SimpleProperty) {
                Field fieldNested = fieldSchema.GetField(property.PropertyNameAtomic);
                if (fieldNested == null) {
                    return null;
                }

                var fragmentEventType = AvroFragmentTypeUtil.GetFragmentEventTypeForField(
                    fieldNested.Schema,
                    moduleName,
                    eventAdapterService,
                    eventTypeAvroHandler,
                    fragmentTypeCache);
                return factory.MakeSimple(
                    fieldNested,
                    fragmentEventType?.FragmentType,
                    AvroTypeUtil.PropertyType(fieldNested.Schema));
            }

            if (property is IndexedProperty indexedProperty) {
                Field fieldNested = fieldSchema.GetField(indexedProperty.PropertyNameAtomic);
                if (fieldNested == null || fieldNested.Schema.Tag != Schema.Type.Array) {
                    return null;
                }

                var fragmentEventType = AvroFragmentTypeUtil.GetFragmentEventTypeForField(
                    fieldNested.Schema,
                    moduleName,
                    eventAdapterService,
                    eventTypeAvroHandler,
                    fragmentTypeCache);
                return factory.MakeIndexed(fieldNested, indexedProperty.Index, fragmentEventType?.FragmentType);
            }

            if (property is MappedProperty mappedProperty) {
                Field fieldNested = fieldSchema.GetField(mappedProperty.PropertyNameAtomic);
                if (fieldNested == null || fieldNested.Schema.Tag != Schema.Type.Map) {
                    return null;
                }

                return factory.MakeMapped(fieldNested, mappedProperty.Key);
            }

            if (property is DynamicProperty) {
                if (property is DynamicSimpleProperty) {
                    return factory.MakeDynamicSimple(property.PropertyNameAtomic);
                }

                throw new NotSupportedException();
            }

            var nested = (NestedProperty) property;
            var allSimple = true;
            foreach (var levelProperty in nested.Properties) {
                if (!(levelProperty is SimpleProperty)) {
                    allSimple = false;
                    break;
                }
            }

            if (allSimple) {
                var currentSchema = fieldSchema;
                var count = 0;
                var path = new Field[nested.Properties.Count];
                var types = new Type[nested.Properties.Count];
                foreach (var levelProperty in nested.Properties) {
                    if (currentSchema.Tag != Schema.Type.Record) {
                        return null;
                    }

                    Field fieldNested = currentSchema.GetField(levelProperty.PropertyNameAtomic);
                    if (fieldNested == null) {
                        return null;
                    }

                    currentSchema = fieldNested.Schema;
                    path[count] = fieldNested;
                    types[count] = AvroTypeUtil.PropertyType(currentSchema);
                    count++;
                }

                var fragmentEventType = AvroFragmentTypeUtil.GetFragmentEventTypeForField(
                    currentSchema,
                    moduleName,
                    eventAdapterService,
                    eventTypeAvroHandler,
                    fragmentTypeCache);
                return factory.MakeNestedSimpleMultiLevel(path, types, fragmentEventType?.FragmentType);
            }

            var getters = new AvroEventPropertyGetter[nested.Properties.Count];
            var countX = 0;
            var currentSchemaX = fieldSchema;
            foreach (var levelProperty in nested.Properties) {
                if (currentSchemaX == null) {
                    return null;
                }

                if (levelProperty is SimpleProperty) {
                    Field fieldNested = currentSchemaX.GetField(levelProperty.PropertyNameAtomic);
                    if (fieldNested == null) {
                        return null;
                    }

                    var fragmentEventType = AvroFragmentTypeUtil.GetFragmentEventTypeForField(
                        fieldNested.Schema,
                        moduleName,
                        eventAdapterService,
                        eventTypeAvroHandler,
                        fragmentTypeCache);
                    var propertyType = AvroTypeUtil.PropertyType(fieldNested.Schema);
                    getters[countX] = new AvroEventBeanGetterSimple(
                        fieldNested,
                        fragmentEventType?.FragmentType,
                        eventAdapterService,
                        propertyType);
                    currentSchemaX = fieldNested.Schema;
                }
                else if (levelProperty is IndexedProperty indexed) {
                    Field fieldIndexed = currentSchemaX.GetField(indexed.PropertyNameAtomic);
                    if (fieldIndexed == null || fieldIndexed.Schema.Tag != Schema.Type.Array) {
                        return null;
                    }

                    var fragmentEventType = AvroFragmentTypeUtil.GetFragmentEventTypeForField(
                        fieldIndexed.Schema,
                        moduleName,
                        eventAdapterService,
                        eventTypeAvroHandler,
                        fragmentTypeCache);
                    getters[countX] = new AvroEventBeanGetterIndexed(
                        fieldIndexed,
                        indexed.Index,
                        fragmentEventType?.FragmentType,
                        eventAdapterService);
                    currentSchemaX = fieldIndexed.Schema.AsArraySchema().ItemSchema;
                }
                else if (levelProperty is MappedProperty mapped) {
                    Field fieldMapped = currentSchemaX.GetField(mapped.PropertyNameAtomic);
                    if (fieldMapped == null || fieldMapped.Schema.Tag != Schema.Type.Map) {
                        return null;
                    }

                    getters[countX] = new AvroEventBeanGetterMapped(fieldMapped, mapped.Key);
                    currentSchemaX = fieldMapped.Schema;
                }
                else if (levelProperty is DynamicSimpleProperty) {
                    if (currentSchemaX.Tag != Schema.Type.Record) {
                        return null;
                    }

                    Field fieldDynamic = currentSchemaX.GetField(levelProperty.PropertyNameAtomic);
                    getters[countX] = new AvroEventBeanGetterSimpleDynamic(levelProperty.PropertyNameAtomic);
                    if (fieldDynamic.Schema.Tag == Schema.Type.Record) {
                        currentSchemaX = fieldDynamic.Schema;
                    }
                    else if (fieldDynamic.Schema.Tag == Schema.Type.Union) {
                        currentSchemaX = AvroSchemaUtil.FindUnionRecordSchemaSingle(fieldDynamic.Schema);
                    }
                }
                else {
                    throw new NotSupportedException();
                }

                countX++;
            }

            return factory.MakeNestedPolyMultiLevel(getters);
        }

        private static AvroEventPropertyGetter GetDynamicGetter(Property property)
        {
            if (property is PropertySimple) {
                return new AvroEventBeanGetterSimpleDynamic(property.PropertyNameAtomic);
            }

            if (property is PropertyWithIndex propertyWithIndex) {
                var index = propertyWithIndex.Index;
                return new AvroEventBeanGetterIndexedDynamic(property.PropertyNameAtomic, index);
            }

            if (property is PropertyWithKey propertyWithKey) {
                var key = propertyWithKey.Key;
                return new AvroEventBeanGetterMappedDynamic(property.PropertyNameAtomic, key);
            }

            var nested = (NestedProperty) property;
            var getters = new AvroEventPropertyGetter[nested.Properties.Count];
            var count = 0;
            foreach (var levelProperty in nested.Properties) {
                getters[count] = GetDynamicGetter(levelProperty);
                count++;
            }

            return new AvroEventBeanGetterDynamicPoly(getters);
        }

        private static void MayAddToGetterCache(
            string propertyName,
            Dictionary<string, EventPropertyGetterSPI> propertyGetterCache,
            EventPropertyGetterSPI getter,
            bool add)
        {
            if (!add) {
                return;
            }

            propertyGetterCache.Put(propertyName, getter);
        }

        private interface GetterNestedFactory
        {
            EventPropertyGetterSPI MakeSimple(
                Field posNested,
                EventType fragmentEventType,
                Type propertyType);

            EventPropertyGetterSPI MakeIndexed(
                Field posNested,
                int index,
                EventType fragmentEventType);

            EventPropertyGetterSPI MakeMapped(
                Field posNested,
                string key);

            EventPropertyGetterSPI MakeDynamicSimple(string propertyName);

            EventPropertyGetterSPI MakeNestedSimpleMultiLevel(
                Field[] path,
                Type[] propertyTypes,
                EventType fragmentEventType);

            EventPropertyGetterSPI MakeNestedPolyMultiLevel(AvroEventPropertyGetter[] getters);
        }

        private class GetterNestedFactoryRootedSimple : GetterNestedFactory
        {
            private readonly EventBeanTypedEventFactory _eventAdapterService;
            private readonly Field _posTop;

            public GetterNestedFactoryRootedSimple(
                EventBeanTypedEventFactory eventAdapterService,
                Field posTop)
            {
                _eventAdapterService = eventAdapterService;
                _posTop = posTop;
            }

            public EventPropertyGetterSPI MakeSimple(
                Field posNested,
                EventType fragmentEventType,
                Type propertyType)
            {
                return new AvroEventBeanGetterNestedSimple(_posTop, posNested, fragmentEventType, _eventAdapterService);
            }

            public EventPropertyGetterSPI MakeIndexed(
                Field posNested,
                int index,
                EventType fragmentEventType)
            {
                return new AvroEventBeanGetterNestedIndexed(
                    _posTop,
                    posNested,
                    index,
                    fragmentEventType,
                    _eventAdapterService);
            }

            public EventPropertyGetterSPI MakeMapped(
                Field posNested,
                string key)
            {
                return new AvroEventBeanGetterNestedMapped(_posTop, posNested, key);
            }

            public EventPropertyGetterSPI MakeDynamicSimple(string propertyName)
            {
                return new AvroEventBeanGetterNestedDynamicSimple(_posTop, propertyName);
            }

            public EventPropertyGetterSPI MakeNestedSimpleMultiLevel(
                Field[] path,
                Type[] propertyTypes,
                EventType fragmentEventType)
            {
                return new AvroEventBeanGetterNestedMultiLevel(_posTop, path, fragmentEventType, _eventAdapterService);
            }

            public EventPropertyGetterSPI MakeNestedPolyMultiLevel(AvroEventPropertyGetter[] getters)
            {
                return new AvroEventBeanGetterNestedPoly(_posTop, getters);
            }
        }

        private class GetterNestedFactoryRootedIndexed : GetterNestedFactory
        {
            private readonly EventBeanTypedEventFactory _eventAdapterService;
            private readonly int _index;
            private readonly Field _pos;

            public GetterNestedFactoryRootedIndexed(
                EventBeanTypedEventFactory eventAdapterService,
                Field pos,
                int index)
            {
                _eventAdapterService = eventAdapterService;
                _pos = pos;
                _index = index;
            }

            public EventPropertyGetterSPI MakeSimple(
                Field posNested,
                EventType fragmentEventType,
                Type propertyType)
            {
                return new AvroEventBeanGetterNestedIndexRooted(
                    _pos,
                    _index,
                    new AvroEventBeanGetterSimple(posNested, fragmentEventType, _eventAdapterService, propertyType));
            }

            public EventPropertyGetterSPI MakeIndexed(
                Field posNested,
                int index,
                EventType fragmentEventType)
            {
                return new AvroEventBeanGetterNestedIndexRooted(
                    _pos,
                    index,
                    new AvroEventBeanGetterIndexed(posNested, index, fragmentEventType, _eventAdapterService));
            }

            public EventPropertyGetterSPI MakeMapped(
                Field posNested,
                string key)
            {
                return new AvroEventBeanGetterNestedIndexRooted(
                    _pos,
                    _index,
                    new AvroEventBeanGetterMapped(posNested, key));
            }

            public EventPropertyGetterSPI MakeDynamicSimple(string propertyName)
            {
                return new AvroEventBeanGetterNestedIndexRooted(
                    _pos,
                    _index,
                    new AvroEventBeanGetterSimpleDynamic(propertyName));
            }

            public EventPropertyGetterSPI MakeNestedSimpleMultiLevel(
                Field[] path,
                Type[] propertyTypes,
                EventType fragmentEventType)
            {
                var getters = new AvroEventPropertyGetter[path.Length];
                for (var i = 0; i < path.Length; i++) {
                    getters[i] = new AvroEventBeanGetterSimple(
                        path[i],
                        fragmentEventType,
                        _eventAdapterService,
                        propertyTypes[i]);
                }

                return new AvroEventBeanGetterNestedIndexRootedMultilevel(_pos, _index, getters);
            }

            public EventPropertyGetterSPI MakeNestedPolyMultiLevel(AvroEventPropertyGetter[] getters)
            {
                return new AvroEventBeanGetterNestedIndexRootedMultilevel(_pos, _index, getters);
            }
        }
    }
} // end of namespace