///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.magic;
using com.espertech.esper.events.arr;
using com.espertech.esper.events.bean;
using com.espertech.esper.events.map;
using com.espertech.esper.events.property;
using com.espertech.esper.util;

namespace com.espertech.esper.events
{
    using DataMap = IDictionary<string, object>;

    public class BaseNestableEventUtil
    {
        public static IDictionary<String, Object> CheckedCastUnderlyingMap(EventBean theEvent)
        {
#if CHECKED_CAST
            if (!(theEvent.Underlying is IDictionary<string, object>))
            {
                System.Diagnostics.Debug.Assert(true, "invalid dictionary");
                throw new InvalidCastException("checked cast to IDictionary<string,object> failed");
            }
#endif

            return (IDictionary<String, Object>)theEvent.Underlying;
        }

        public static Object[] CheckedCastUnderlyingObjectArray(EventBean theEvent)
        {
            return (Object[])theEvent.Underlying;
        }

        public static Object HandleNestedValueArrayWithMap(Object value, int index, MapEventPropertyGetter getter)
        {
            var asArray = value as Array;
            if (asArray == null)
            {
                return null;
            }

            if (asArray.Length <= index)
            {
                return null;
            }

            var valueMap = asArray.GetValue(index);
            if (!(valueMap is DataMap))
            {
                if (valueMap is EventBean)
                {
                    return getter.Get((EventBean)valueMap);
                }
                return null;
            }
            return getter.GetMap((IDictionary<String, Object>)valueMap);
        }

        public static bool HandleNestedValueArrayWithMapExists(Object value, int index, MapEventPropertyGetter getter)
        {
            var asArray = value as Array;
            if (asArray == null)
            {
                return false;
            }

            if (asArray.Length <= index)
            {
                return false;
            }

            var valueMap = asArray.GetValue(index);
            if (!(valueMap is DataMap))
            {
                if (valueMap is EventBean)
                {
                    return getter.IsExistsProperty((EventBean) valueMap);
                }
                return false;
            }
            return getter.IsMapExistsProperty((DataMap) valueMap);
        }

        public static Object HandleNestedValueArrayWithMapFragment(Object value, int index, MapEventPropertyGetter getter, EventAdapterService eventAdapterService, EventType fragmentType)
        {
            var asArray = value as Array;
            if (asArray == null)
            {
                return null;
            }

            if (asArray.Length <= index)
            {
                return null;
            }

            var valueMap = asArray.GetValue(index);
            if (!(valueMap is DataMap))
            {
                if (valueMap is EventBean)
                {
                    return getter.GetFragment((EventBean)valueMap);
                }
                return null;
            }

            // If the map does not contain the key, this is allowed and represented as null
            var eventBean = eventAdapterService.AdapterForTypedMap((IDictionary<String, Object>)valueMap, fragmentType);
            return getter.GetFragment(eventBean);
        }

        public static Object HandleNestedValueArrayWithObjectArray(Object value, int index, ObjectArrayEventPropertyGetter getter)
        {
            var asArray = value as Array;
            if (asArray == null)
            {
                return null;
            }

            if (asArray.Length <= index)
            {
                return null;
            }

            var valueArray = asArray.GetValue(index);
            if (!(valueArray is Object[]))
            {
                if (valueArray is EventBean)
                {
                    return getter.Get((EventBean)valueArray);
                }
                return null;
            }
            return getter.GetObjectArray((Object[])valueArray);
        }

        public static bool HandleNestedValueArrayWithObjectArrayExists(Object value, int index, ObjectArrayEventPropertyGetter getter)
        {
            var asArray = value as Array;
            if (asArray == null)
            {
                return false;
            }

            if (asArray.Length <= index)
            {
                return false;
            }

            var valueArray = asArray.GetValue(index);
            if (!(valueArray is Object[]))
            {
                if (valueArray is EventBean)
                {
                    return getter.IsExistsProperty((EventBean) valueArray);
                }
                return false;
            }
            return getter.IsObjectArrayExistsProperty((Object[]) valueArray);
        }

        public static Object HandleNestedValueArrayWithObjectArrayFragment(Object value, int index, ObjectArrayEventPropertyGetter getter, EventType fragmentType, EventAdapterService eventAdapterService)
        {
            var asArray = value as Array;
            if (asArray == null)
            {
                return null;
            }

            if (asArray.Length <= index)
            {
                return null;
            }

            var valueArray = asArray.GetValue(index);
            if (!(valueArray is Object[]))
            {
                if (valueArray is EventBean)
                {
                    return getter.GetFragment((EventBean)valueArray);
                }
                return null;
            }

            // If the map does not contain the key, this is allowed and represented as null
            var eventBean = eventAdapterService.AdapterForTypedObjectArray((Object[])valueArray, fragmentType);
            return getter.GetFragment(eventBean);
        }

        public static Object HandleCreateFragmentMap(Object value, EventType fragmentEventType, EventAdapterService eventAdapterService)
        {
            if (!(value is DataMap))
            {
                if (value is EventBean)
                {
                    return value;
                }
                return null;
            }
            var subEvent = (DataMap)value;
            return eventAdapterService.AdapterForTypedMap(subEvent, fragmentEventType);
        }

        public static Object HandleCreateFragmentObjectArray(Object value, EventType fragmentEventType, EventAdapterService eventAdapterService)
        {
            if (!(value is Object[]))
            {
                if (value is EventBean)
                {
                    return value;
                }
                return null;
            }
            var subEvent = (Object[])value;
            return eventAdapterService.AdapterForTypedObjectArray(subEvent, fragmentEventType);
        }

        public static Object GetMappedPropertyValue(Object value, String key)
        {
            if (value == null)
            {
                return null;
            }
            if (value is DataMap)
            {
                return ((DataMap)value).Get(key);
            }
            if (value.GetType().IsGenericStringDictionary())
            {
                return MagicMarker.GetStringDictionary(value).Get(key);
            }

            return null;
        }

        public static bool GetMappedPropertyExists(Object value, String key)
        {
            if (value == null)
            {
                return false;
            }
            if (value is DataMap)
            {
                return ((DataMap)value).ContainsKey(key);
            }
            if (value.GetType().IsGenericStringDictionary())
            {
                return MagicMarker.GetStringDictionary(value).ContainsKey(key);
            }

            return false;
        }

        public static MapIndexedPropPair GetIndexedAndMappedProps(String[] properties)
        {
            ICollection<String> mapPropertiesToCopy = new HashSet<String>();
            ICollection<String> arrayPropertiesToCopy = new HashSet<String>();
            for (var i = 0; i < properties.Length; i++)
            {
                var prop = PropertyParser.ParseAndWalkLaxToSimple(properties[i]);
                if (prop is MappedProperty)
                {
                    var mappedProperty = (MappedProperty)prop;
                    mapPropertiesToCopy.Add(mappedProperty.PropertyNameAtomic);
                }
                if (prop is IndexedProperty)
                {
                    var indexedProperty = (IndexedProperty)prop;
                    arrayPropertiesToCopy.Add(indexedProperty.PropertyNameAtomic);
                }
            }
            return new MapIndexedPropPair(mapPropertiesToCopy, arrayPropertiesToCopy);
        }

        public static Object GetIndexedValue(Object value, int index)
        {
            if (value == null)
            {
                return null;
            }

            var asArray = value as Array;
            if (asArray != null)
            {
                if (asArray.Length <= index)
                {
                    return null;
                }

                return asArray.GetValue(index);
            }

            var asString = value as string;
            if (asString != null)
            {
                if (asString.Length <= index)
                {
                    return null;
                }

                return asString[index];
            }

            var asType = value.GetType();
            var asGenericList = asType.FindGenericInterface(typeof(IList<>));
            if (asGenericList != null)
            {
                var asList = MagicMarker.GetListFactory(value.GetType()).Invoke(value);
                if (asList.Count <= index)
                {
                    return null;
                }

                return asList[index];
            }

            return null;
        }

        public static bool IsExistsIndexedValue(Object value, int index)
        {
            if (value == null)
            {
                return false;
            }

            var asArray = value as Array;
            if (asArray != null)
            {
                return asArray.Length > index;
            }

            var asString = value as string;
            if (asString != null)
            {
                return asString.Length > index;
            }

            var asType = value.GetType();
            var asGenericList = asType.FindGenericInterface(typeof(IList<>));
            if (asGenericList != null)
            {
                var asList = MagicMarker.GetListFactory(value.GetType()).Invoke(value);
                return asList.Count > index;
            }

            return false;
        }

        public static EventBean GetFragmentNonPono(EventAdapterService eventAdapterService, Object fragmentUnderlying, EventType fragmentEventType)
        {
            if (fragmentUnderlying == null)
            {
                return null;
            }
            if (fragmentEventType is MapEventType)
            {
                return eventAdapterService.AdapterForTypedMap((IDictionary<String, Object>)fragmentUnderlying, fragmentEventType);
            }
            return eventAdapterService.AdapterForTypedObjectArray(fragmentUnderlying.UnwrapIntoArray<object>(true), fragmentEventType);
        }

        public static Object GetFragmentArray(EventAdapterService eventAdapterService, Object value, EventType fragmentEventType)
        {
            var mapTypedSubEvents = value as DataMap[];
            if (mapTypedSubEvents != null)
            {
                var countNull = 0;
                foreach (var map in mapTypedSubEvents)
                {
                    if (map != null)
                    {
                        countNull++;
                    }
                }

                var mapEvents = new EventBean[countNull];
                var count = 0;
                foreach (var map in mapTypedSubEvents)
                {
                    if (map != null)
                    {
                        mapEvents[count++] = eventAdapterService.AdapterForTypedMap(map, fragmentEventType);
                    }
                }

                return mapEvents;
            }

            var subEvents = value as Object[];
            if (subEvents != null)
            {
                var subCountNull = subEvents.Count(subEvent => subEvent != null);

                var outEvents = new EventBean[subCountNull];
                var outCount = 0;

                foreach (var item in subEvents)
                {
                    if (item != null)
                    {
                        outEvents[outCount++] = BaseNestableEventUtil.GetFragmentNonPono(eventAdapterService, item, fragmentEventType);
                    }
                }

                return outEvents;
            }

            return null;
        }

        public static Object GetBeanArrayValue(BeanEventPropertyGetter nestedGetter, Object value, int index)
        {
            var asArray = value as Array;
            if (asArray == null)
            {
                return null;
            }

            if (asArray.Length <= index)
            {
                return null;
            }

            var arrayItem = asArray.GetValue(index);
            if (arrayItem == null)
            {
                return null;
            }

            return nestedGetter.GetBeanProp(arrayItem);
        }

        public static Object GetFragmentPono(Object result, BeanEventType eventType, EventAdapterService eventAdapterService)
        {
            if (result == null)
            {
                return null;
            }
            if (result is EventBean[])
            {
                return result;
            }
            if (result is EventBean)
            {
                return result;
            }
            if (result is Array)
            {
                var arrayX = (Array)result;
                var len = arrayX.Length;
                var events = new EventBean[len];
                for (var i = 0; i < events.Length; i++)
                {
                    events[i] = eventAdapterService.AdapterForTypedObject(arrayX.GetValue(i), eventType);
                }
                return events;
            }
            return eventAdapterService.AdapterForTypedObject(result, eventType);
        }

        public static Object GetArrayPropertyValue(EventBean[] wrapper, int index, EventPropertyGetter nestedGetter)
        {
            if (wrapper == null)
            {
                return null;
            }
            if (wrapper.Length <= index)
            {
                return null;
            }
            var innerArrayEvent = wrapper[index];
            return nestedGetter.Get(innerArrayEvent);
        }

        public static Object GetArrayPropertyFragment(EventBean[] wrapper, int index, EventPropertyGetter nestedGetter)
        {
            if (wrapper == null)
            {
                return null;
            }
            if (wrapper.Length <= index)
            {
                return null;
            }
            var innerArrayEvent = wrapper[index];
            return nestedGetter.GetFragment(innerArrayEvent);
        }

        public static Object GetArrayPropertyUnderlying(EventBean[] wrapper, int index)
        {
            if (wrapper == null)
            {
                return null;
            }
            if (wrapper.Length <= index)
            {
                return null;
            }

            return wrapper[index].Underlying;
        }

        public static Object GetArrayPropertyBean(EventBean[] wrapper, int index)
        {
            if (wrapper == null)
            {
                return null;
            }
            if (wrapper.Length <= index)
            {
                return null;
            }

            return wrapper[index];
        }

        public static Object GetArrayPropertyAsUnderlyingsArray(Type underlyingType, EventBean[] wrapper)
        {
            if (wrapper != null)
            {
                var array = Array.CreateInstance(underlyingType, wrapper.Length);
                for (var i = 0; i < wrapper.Length; i++)
                {
                    array.SetValue(wrapper[i].Underlying, i);
                }
                return array;
            }

            return null;
        }

        public static String ComparePropType(String propName, Object setOneType, Object setTwoType, bool setTwoTypeFound, String otherName)
        {
            // allow null for nested event types
            if ((setOneType is String || setOneType is EventType) && setTwoType == null)
            {
                return null;
            }
            if ((setTwoType is String || setTwoType is EventType) && setOneType == null)
            {
                return null;
            }
            if (!setTwoTypeFound)
            {
                return "The property '" + propName + "' is not provided but required";
            }
            if (setTwoType == null)
            {
                return null;
            }
            if (setOneType == null)
            {
                return "Type by name '" + otherName + "' in property '" + propName + "' incompatible with null-type or property name not found in target";
            }

            if ((setTwoType is Type) && (setOneType is Type))
            {
                var boxedOther = ((Type)setTwoType).GetBoxedType();
                var boxedThis = ((Type)setOneType).GetBoxedType();
                if (boxedOther != boxedThis)
                {
                    if (!TypeHelper.IsSubclassOrImplementsInterface(boxedOther, boxedThis))
                    {
                        return string.Format("Type by name '{0}' in property '{1}' expected {2} but receives {3}",
                            otherName, propName, Name.Of(boxedThis), Name.Of(boxedOther));
                    }
                }
            }
            else if ((setTwoType is BeanEventType) && (setOneType is Type))
            {
                Type boxedOther = ((BeanEventType)setTwoType).UnderlyingType.GetBoxedType();
                var boxedThis = ((Type)setOneType).GetBoxedType();
                if (boxedOther != boxedThis)
                {
                    return "Type by name '" + otherName + "' in property '" + propName + "' expected " + boxedThis + " but receives " + boxedOther;
                }
            }
            else if (setTwoType is EventType[] && ((EventType[])setTwoType)[0] is BeanEventType && setOneType is Type && ((Type)setOneType).IsArray)
            {
                var boxedOther = (((EventType[])setTwoType)[0]).UnderlyingType.GetBoxedType();
                var boxedThis = ((Type)setOneType).GetElementType().GetBoxedType();
                if (boxedOther != boxedThis)
                {
                    return "Type by name '" + otherName + "' in property '" + propName + "' expected " + boxedThis + " but receives " + boxedOther;
                }
            }
            else if ((setTwoType is IDictionary<String, Object>) && (setOneType is IDictionary<String, Object>))
            {
                var messageIsDeepEquals = BaseNestableEventType.IsDeepEqualsProperties(propName, (IDictionary<String, Object>)setOneType, (IDictionary<String, Object>)setTwoType);
                if (messageIsDeepEquals != null)
                {
                    return messageIsDeepEquals;
                }
            }
            else if ((setTwoType is EventType) && (setOneType is EventType))
            {
                bool mismatch;
                if (setTwoType is EventTypeSPI && setOneType is EventTypeSPI)
                {
                    mismatch = !((EventTypeSPI)setOneType).EqualsCompareType((EventTypeSPI)setTwoType);
                }
                else
                {
                    mismatch = !setOneType.Equals(setTwoType);
                }
                if (mismatch)
                {
                    var setOneEventType = (EventType)setOneType;
                    var setTwoEventType = (EventType)setTwoType;
                    return "Type by name '" + otherName + "' in property '" + propName + "' expected event type '" + setOneEventType.Name + "' but receives event type '" + setTwoEventType.Name + "'";
                }
            }
            else if ((setTwoType is String) && (setOneType is EventType))
            {
                var setOneEventType = (EventType)setOneType;
                var setTwoEventType = (String)setTwoType;
                if (!EventTypeUtility.IsTypeOrSubTypeOf(setTwoEventType, setOneEventType))
                {
                    return "Type by name '" + otherName + "' in property '" + propName + "' expected event type '" + setOneEventType.Name + "' but receives event type '" + setTwoEventType + "'";
                }
            }
            else if ((setTwoType is EventType) && (setOneType is String))
            {
                var setTwoEventType = (EventType)setTwoType;
                var setOneEventType = (String)setOneType;
                if (!EventTypeUtility.IsTypeOrSubTypeOf(setOneEventType, setTwoEventType))
                {
                    return "Type by name '" + otherName + "' in property '" + propName + "' expected event type '" + setOneEventType + "' but receives event type '" + setTwoEventType.Name + "'";
                }
            }
            else if ((setTwoType is String) && (setOneType is String))
            {
                if (!setTwoType.Equals(setOneType))
                {
                    var setOneEventType = (String)setOneType;
                    var setTwoEventType = (String)setTwoType;
                    return "Type by name '" + otherName + "' in property '" + propName + "' expected event type '" + setOneEventType + "' but receives event type '" + setTwoEventType + "'";
                }
            }
            else if ((setTwoType is EventType[]) && (setOneType is String))
            {
                var setTwoTypeArr = (EventType[])setTwoType;
                var setTwoFragmentType = setTwoTypeArr[0];
                var setOneTypeString = (String)setOneType;
                if (!(setOneTypeString.EndsWith("[]")))
                {
                    return "Type by name '" + otherName + "' in property '" + propName + "' expected event type '" + setOneType + "' but receives event type '" + setTwoFragmentType.Name + "[]'";
                }
                var setOneTypeNoArray = (setOneTypeString).RegexReplaceAll("\\[\\]", "");
                if (!(setTwoFragmentType.Name.Equals(setOneTypeNoArray)))
                {
                    return "Type by name '" + otherName + "' in property '" + propName + "' expected event type '" + setOneTypeNoArray + "[]' but receives event type '" + setTwoFragmentType.Name + "'";
                }
            }
            else
            {
                var typeOne = GetTypeName(setOneType);
                var typeTwo = GetTypeName(setTwoType);
                if (typeOne.Equals(typeTwo))
                {
                    return null;
                }
                return "Type by name '" + otherName + "' in property '" + propName + "' expected " + typeOne + " but receives " + typeTwo;
            }

            return null;
        }

        private static String GetTypeName(Object type)
        {
            if (type == null)
            {
                return "null";
            }
            if (type is Type)
            {
                return ((Type)type).FullName;
            }
            if (type is EventType)
            {
                return "event type '" + ((EventType)type).Name + "'";
            }
            if (type is String)
            {
                var boxedType = TypeHelper.GetPrimitiveTypeForName((String)type).GetBoxedType();
                if (boxedType != null)
                {
                    return boxedType.FullName;
                }

                return (string)type;
            }
            return type.GetType().FullName;
        }

        public class MapIndexedPropPair
        {
            public MapIndexedPropPair(ICollection<String> mapProperties, ICollection<String> arrayProperties)
            {
                MapProperties = mapProperties;
                ArrayProperties = arrayProperties;
            }

            public ICollection<string> MapProperties { get; private set; }

            public ICollection<string> ArrayProperties { get; private set; }
        }
    }
}
