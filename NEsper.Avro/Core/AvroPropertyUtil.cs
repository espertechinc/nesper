///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Avro;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.parse;
using com.espertech.esper.events;
using com.espertech.esper.events.property;

using NEsper.Avro.Extensions;
using NEsper.Avro.Getter;

namespace NEsper.Avro.Core
{
    public class AvroPropertyUtil
    {
        public static Type PropertyType(Schema fieldSchema, Property property)
        {
            var desc = AvroFieldUtil.FieldForProperty(fieldSchema, property);
            if (desc == null)
            {
                return null;
            }
            if (desc.IsDynamic)
            {
                return typeof (Object);
            }
            Schema typeSchema = desc.Field.Schema;
            if (desc.IsAccessedByIndex)
            {
                if (desc.Field.Schema.Tag == Schema.Type.Array)
                {
                    typeSchema = desc.Field.Schema.GetElementType();
                }
                else if (desc.Field.Schema.Tag == Schema.Type.String)
                {
                    return typeof(char);
                }
            }
            else if (desc.IsAccessedByKey)
            {
                typeSchema = desc.Field.Schema.GetValueType();
            }
            return AvroTypeUtil.PropertyType(typeSchema);
        }

        public static EventPropertyGetterSPI GetGetter(
            Schema avroSchema,
            Dictionary<string, EventPropertyGetterSPI> propertyGetterCache,
            IDictionary<string, PropertySetDescriptorItem> propertyDescriptors,
            string propertyName,
            bool addToCache,
            EventAdapterService eventAdapterService)
        {
            var getter = propertyGetterCache.Get(propertyName);
            if (getter != null)
            {
                return getter;
            }

            var unescapePropName = ASTUtil.UnescapeDot(propertyName);
            var item = propertyDescriptors.Get(unescapePropName);
            if (item != null)
            {
                getter = item.PropertyGetter;
                MayAddToGetterCache(propertyName, propertyGetterCache, getter, true);
                return getter;
            }

            // see if this is a nested property
            var index = ASTUtil.UnescapedIndexOfDot(propertyName);
            if (index == -1)
            {
                var prop = PropertyParser.ParseAndWalkLaxToSimple(propertyName);
                if (prop is IndexedProperty)
                {
                    var indexedProp = (IndexedProperty) prop;
                    Field field = avroSchema.GetField(prop.PropertyNameAtomic);
                    if (field == null)
                    {
                        return null;
                    }
                    switch(field.Schema.Tag)
                    {
                        case Schema.Type.Array:
                            var fragmentEventType = AvroFragmentTypeUtil.GetFragmentEventTypeForField(
                                field.Schema, eventAdapterService);
                            getter = new AvroEventBeanGetterIndexed(
                                field, indexedProp.Index,
                                fragmentEventType == null ? null : fragmentEventType.FragmentType, eventAdapterService);
                            MayAddToGetterCache(propertyName, propertyGetterCache, getter, addToCache);
                            return getter;

                        case Schema.Type.String:
                            getter = new AvroEventBeanGetterStringIndexed(field, indexedProp.Index);
                            MayAddToGetterCache(propertyName, propertyGetterCache, getter, addToCache);
                            return getter;

                        default:
                            return null;
                    }
                }
                else if (prop is MappedProperty)
                {
                    var mappedProp = (MappedProperty) prop;
                    Field field = avroSchema.GetField(prop.PropertyNameAtomic);
                    if (field == null || field.Schema.Tag != Schema.Type.Map)
                    {
                        return null;
                    }
                    getter = new AvroEventBeanGetterMapped(field, mappedProp.Key);
                    MayAddToGetterCache(propertyName, propertyGetterCache, getter, addToCache);
                    return getter;
                }
                if (prop is DynamicIndexedProperty)
                {
                    var dynamicIndexedProp = (DynamicIndexedProperty) prop;
                    getter = new AvroEventBeanGetterIndexedDynamic(prop.PropertyNameAtomic, dynamicIndexedProp.Index);
                    MayAddToGetterCache(propertyName, propertyGetterCache, getter, addToCache);
                    return getter;
                }
                if (prop is DynamicMappedProperty)
                {
                    var dynamicMappedProp = (DynamicMappedProperty) prop;
                    getter = new AvroEventBeanGetterMappedDynamic(prop.PropertyNameAtomic, dynamicMappedProp.Key);
                    MayAddToGetterCache(propertyName, propertyGetterCache, getter, addToCache);
                    return getter;
                }
                else if (prop is DynamicSimpleProperty)
                {
                    getter = new AvroEventBeanGetterSimpleDynamic(prop.PropertyNameAtomic);
                    MayAddToGetterCache(propertyName, propertyGetterCache, getter, addToCache);
                    return getter;
                }
                return null; // simple property already cached
            }

            // Take apart the nested property into a map key and a nested value class property name
            var propertyTop = ASTUtil.UnescapeDot(propertyName.Substring(0, index));
            var propertyNested = propertyName.Substring(index + 1);
            var isRootedDynamic = false;

            // If the property is dynamic, remove the ? since the property type is defined without
            if (propertyTop.EndsWith("?"))
            {
                propertyTop = propertyTop.Substring(0, propertyTop.Length - 1);
                isRootedDynamic = true;
            }

            var propTop = PropertyParser.ParseAndWalkLaxToSimple(propertyTop);
            Field fieldTop = avroSchema.GetField(propTop.PropertyNameAtomic);

            // field is known and is a record
            if (fieldTop != null && fieldTop.Schema.Tag == Schema.Type.Record && propTop is SimpleProperty)
            {
                var factory = new GetterNestedFactoryRootedSimple(eventAdapterService, fieldTop);
                var property = PropertyParser.ParseAndWalk(propertyNested, isRootedDynamic);
                getter = PropertyGetterNested(factory, fieldTop.Schema, property, eventAdapterService);
                MayAddToGetterCache(propertyName, propertyGetterCache, getter, addToCache);
                return getter;
            }

            // field is known and is a record
            if (fieldTop != null && fieldTop.Schema.Tag == Schema.Type.Array && propTop is IndexedProperty)
            {
                var factory = new GetterNestedFactoryRootedIndexed(
                    eventAdapterService, fieldTop, ((IndexedProperty) propTop).Index);
                var property = PropertyParser.ParseAndWalk(propertyNested, isRootedDynamic);
                getter = PropertyGetterNested(factory, fieldTop.Schema.GetElementType(), property, eventAdapterService);
                MayAddToGetterCache(propertyName, propertyGetterCache, getter, addToCache);
                return getter;
            }

            // field is not known or is not a record
            if (!isRootedDynamic)
            {
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
            EventAdapterService eventAdapterService)
        {
            if (property is SimpleProperty)
            {
                Field fieldNested = fieldSchema.GetField(property.PropertyNameAtomic);
                if (fieldNested == null)
                {
                    return null;
                }
                var fragmentEventType = AvroFragmentTypeUtil.GetFragmentEventTypeForField(
                    fieldNested.Schema, eventAdapterService);
                return factory.MakeSimple(
                    fieldNested, fragmentEventType == null ? null : fragmentEventType.FragmentType);
            }

            if (property is IndexedProperty)
            {
                var indexed = (IndexedProperty) property;
                Field fieldNested = fieldSchema.GetField(property.PropertyNameAtomic);
                if (fieldNested == null || fieldNested.Schema.Tag != Schema.Type.Array)
                {
                    return null;
                }
                var fragmentEventType = AvroFragmentTypeUtil.GetFragmentEventTypeForField(
                    fieldNested.Schema, eventAdapterService);
                return factory.MakeIndexed(
                    fieldNested, indexed.Index, fragmentEventType == null ? null : fragmentEventType.FragmentType);
            }

            if (property is MappedProperty)
            {
                var mapped = (MappedProperty) property;
                Field fieldNested = fieldSchema.GetField(property.PropertyNameAtomic);
                if (fieldNested == null || fieldNested.Schema.Tag != Schema.Type.Map)
                {
                    return null;
                }
                return factory.MakeMapped(fieldNested, mapped.Key);
            }

            if (property is DynamicProperty)
            {
                if (property is DynamicSimpleProperty)
                {
                    return factory.MakeDynamicSimple(property.PropertyNameAtomic);
                }
                throw new UnsupportedOperationException();
            }

            var nested = (NestedProperty) property;
            var allSimple = true;
            foreach (var levelProperty in nested.Properties)
            {
                if (!(levelProperty is SimpleProperty))
                {
                    allSimple = false;
                    break;
                }
            }
            if (allSimple)
            {
                var currentSchemaX = fieldSchema;
                var countX = 0;
                var path = new Field[nested.Properties.Count];
                foreach (var levelProperty in nested.Properties)
                {
                    if (currentSchemaX.Tag != Schema.Type.Record)
                    {
                        return null;
                    }
                    Field fieldNested = currentSchemaX.GetField(levelProperty.PropertyNameAtomic);
                    if (fieldNested == null)
                    {
                        return null;
                    }
                    currentSchemaX = fieldNested.Schema;
                    path[countX] = fieldNested;
                    countX++;
                }
                var fragmentEventType = AvroFragmentTypeUtil.GetFragmentEventTypeForField(
                    currentSchemaX, eventAdapterService);
                return factory.MakeNestedSimpleMultiLevel(
                    path, fragmentEventType == null ? null : fragmentEventType.FragmentType);
            }

            var getters = new AvroEventPropertyGetter[nested.Properties.Count];
            var count = 0;
            var currentSchema = fieldSchema;
            foreach (var levelProperty in nested.Properties)
            {
                if (currentSchema == null)
                {
                    return null;
                }

                if (levelProperty is SimpleProperty)
                {
                    Field fieldNested = currentSchema.GetField(levelProperty.PropertyNameAtomic);
                    if (fieldNested == null)
                    {
                        return null;
                    }
                    FragmentEventType fragmentEventType = AvroFragmentTypeUtil.GetFragmentEventTypeForField(
                        fieldNested.Schema, eventAdapterService);
                    getters[count] = new AvroEventBeanGetterSimple(
                        fieldNested, fragmentEventType == null ? null : fragmentEventType.FragmentType,
                        eventAdapterService);
                    currentSchema = fieldNested.Schema;
                }
                else if (levelProperty is IndexedProperty)
                {
                    var indexed = (IndexedProperty) levelProperty;
                    Field fieldIndexed = currentSchema.GetField(levelProperty.PropertyNameAtomic);
                    if (fieldIndexed == null || fieldIndexed.Schema.Tag != Schema.Type.Array)
                    {
                        return null;
                    }
                    var fragmentEventType = AvroFragmentTypeUtil.GetFragmentEventTypeForField(
                        fieldIndexed.Schema, eventAdapterService);
                    getters[count] = new AvroEventBeanGetterIndexed(
                        fieldIndexed, indexed.Index,
                        fragmentEventType == null ? null : fragmentEventType.FragmentType, eventAdapterService);
                    currentSchema = fieldIndexed.Schema.GetElementType();
                }
                else if (levelProperty is MappedProperty)
                {
                    var mapped = (MappedProperty) levelProperty;
                    Field fieldMapped = currentSchema.GetField(levelProperty.PropertyNameAtomic);
                    if (fieldMapped == null || fieldMapped.Schema.Tag != Schema.Type.Map)
                    {
                        return null;
                    }
                    getters[count] = new AvroEventBeanGetterMapped(fieldMapped, mapped.Key);
                    currentSchema = fieldMapped.Schema;
                }
                else if (levelProperty is DynamicSimpleProperty)
                {
                    if (currentSchema.Tag != Schema.Type.Record)
                    {
                        return null;
                    }
                    Field fieldDynamic = currentSchema.GetField(levelProperty.PropertyNameAtomic);
                    getters[count] = new AvroEventBeanGetterSimpleDynamic(levelProperty.PropertyNameAtomic);
                    if (fieldDynamic.Schema.Tag == Schema.Type.Record)
                    {
                        currentSchema = fieldDynamic.Schema;
                    }
                    else if (fieldDynamic.Schema.Tag == Schema.Type.Union)
                    {
                        currentSchema = AvroSchemaUtil.FindUnionRecordSchemaSingle(fieldDynamic.Schema);
                    }
                }
                else
                {
                    throw new UnsupportedOperationException();
                }
                count++;
            }
            return factory.MakeNestedPolyMultiLevel(getters);
        }

        private static AvroEventPropertyGetter GetDynamicGetter(Property property)
        {
            if (property is PropertySimple)
            {
                return new AvroEventBeanGetterSimpleDynamic(property.PropertyNameAtomic);
            }
            else if (property is PropertyWithIndex)
            {
                var index = ((PropertyWithIndex) property).Index;
                return new AvroEventBeanGetterIndexedDynamic(property.PropertyNameAtomic, index);
            }
            else if (property is PropertyWithKey)
            {
                var key = ((PropertyWithKey) property).Key;
                return new AvroEventBeanGetterMappedDynamic(property.PropertyNameAtomic, key);
            }

            var nested = (NestedProperty) property;
            var getters = new AvroEventPropertyGetter[nested.Properties.Count];
            var count = 0;
            foreach (var levelProperty in nested.Properties)
            {
                getters[count] = GetDynamicGetter(levelProperty);
                count++;
            }
            return new AvroEventBeanGetterDynamicPoly(getters);
        }

        private static void MayAddToGetterCache(
            string propertyName,
            IDictionary<string, EventPropertyGetterSPI> propertyGetterCache,
            EventPropertyGetterSPI getter,
            bool add)
        {
            if (!add)
            {
                return;
            }
            propertyGetterCache.Put(propertyName, getter);
        }

        private interface GetterNestedFactory
        {
            EventPropertyGetterSPI MakeSimple(Field posNested, EventType fragmentEventType);

            EventPropertyGetterSPI MakeIndexed(Field posNested, int index, EventType fragmentEventType);

            EventPropertyGetterSPI MakeMapped(Field posNested, string key);

            EventPropertyGetterSPI MakeDynamicSimple(string propertyName);

            EventPropertyGetterSPI MakeNestedSimpleMultiLevel(Field[] path, EventType fragmentEventType);

            EventPropertyGetterSPI MakeNestedPolyMultiLevel(AvroEventPropertyGetter[] getters);
        }

        private class GetterNestedFactoryRootedSimple : GetterNestedFactory
        {
            private readonly EventAdapterService _eventAdapterService;
            private readonly Field _posTop;

            public GetterNestedFactoryRootedSimple(EventAdapterService eventAdapterService, Field posTop)
            {
                _eventAdapterService = eventAdapterService;
                _posTop = posTop;
            }

            public EventPropertyGetterSPI MakeSimple(Field posNested, EventType fragmentEventType)
            {
                return new AvroEventBeanGetterNestedSimple(_posTop, posNested, fragmentEventType, _eventAdapterService);
            }

            public EventPropertyGetterSPI MakeIndexed(Field posNested, int index, EventType fragmentEventType)
            {
                return new AvroEventBeanGetterNestedIndexed(
                    _posTop, posNested, index, fragmentEventType, _eventAdapterService);
            }

            public EventPropertyGetterSPI MakeMapped(Field posNested, string key)
            {
                return new AvroEventBeanGetterNestedMapped(_posTop, posNested, key);
            }

            public EventPropertyGetterSPI MakeDynamicSimple(string propertyName)
            {
                return new AvroEventBeanGetterNestedDynamicSimple(_posTop, propertyName);
            }

            public EventPropertyGetterSPI MakeNestedSimpleMultiLevel(Field[] path, EventType fragmentEventType)
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
            private readonly EventAdapterService _eventAdapterService;
            private readonly Field _pos;
            private readonly int _index;

            public GetterNestedFactoryRootedIndexed(EventAdapterService eventAdapterService, Field pos, int index)
            {
                _eventAdapterService = eventAdapterService;
                _pos = pos;
                _index = index;
            }

            public EventPropertyGetterSPI MakeSimple(Field posNested, EventType fragmentEventType)
            {
                return new AvroEventBeanGetterNestedIndexRooted(
                    _pos, _index, new AvroEventBeanGetterSimple(posNested, fragmentEventType, _eventAdapterService));
            }

            public EventPropertyGetterSPI MakeIndexed(Field posNested, int index, EventType fragmentEventType)
            {
                return new AvroEventBeanGetterNestedIndexRooted(
                    _pos, index,
                    new AvroEventBeanGetterIndexed(posNested, index, fragmentEventType, _eventAdapterService));
            }

            public EventPropertyGetterSPI MakeMapped(Field posNested, string key)
            {
                return new AvroEventBeanGetterNestedIndexRooted(
                    _pos, _index, new AvroEventBeanGetterMapped(posNested, key));
            }

            public EventPropertyGetterSPI MakeDynamicSimple(string propertyName)
            {
                return new AvroEventBeanGetterNestedIndexRooted(
                    _pos, _index, new AvroEventBeanGetterSimpleDynamic(propertyName));
            }

            public EventPropertyGetterSPI MakeNestedSimpleMultiLevel(Field[] path, EventType fragmentEventType)
            {
                var getters = new AvroEventPropertyGetter[path.Length];
                for (var i = 0; i < path.Length; i++)
                {
                    getters[i] = new AvroEventBeanGetterSimple(path[i], fragmentEventType, _eventAdapterService);
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
