///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.common.@internal.@event.property;
using com.espertech.esper.common.@internal.@event.util;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.
    CodegenRelational;

namespace com.espertech.esper.common.@internal.@event.core
{
    using Map = IDictionary<string, object>;

    public class BaseNestableEventUtil
    {
        public static MapEventType MakeMapTypeCompileTime(
            EventTypeMetadata metadata,
            IDictionary<string, object> propertyTypes,
            EventType[] optionalSuperTypes,
            ISet<EventType> optionalDeepSupertypes,
            string startTimestampPropertyName,
            string endTimestampPropertyName,
            BeanEventTypeFactory beanEventTypeFactory,
            EventTypeCompileTimeResolver eventTypeCompileTimeResolver)
        {
            if (metadata.ApplicationType != EventTypeApplicationType.MAP) {
                throw new ArgumentException("Invalid application type " + metadata.ApplicationType);
            }

            IDictionary<string, object> verified = ResolvePropertyTypes(propertyTypes, eventTypeCompileTimeResolver);
            return new MapEventType(
                metadata, verified, optionalSuperTypes, optionalDeepSupertypes, startTimestampPropertyName,
                endTimestampPropertyName, beanEventTypeFactory);
        }

        public static ObjectArrayEventType MakeOATypeCompileTime(
            EventTypeMetadata metadata,
            IDictionary<string, object> properyTypes,
            EventType[] optionalSuperTypes,
            ISet<EventType> optionalDeepSupertypes,
            string startTimestampName,
            string endTimestampName,
            BeanEventTypeFactory beanEventTypeFactory,
            EventTypeCompileTimeResolver eventTypeCompileTimeResolver)
        {
            if (metadata.ApplicationType != EventTypeApplicationType.OBJECTARR) {
                throw new ArgumentException("Invalid application type " + metadata.ApplicationType);
            }

            IDictionary<string, object> verified = ResolvePropertyTypes(properyTypes, eventTypeCompileTimeResolver);
            return new ObjectArrayEventType(
                metadata, verified, optionalSuperTypes, optionalDeepSupertypes, startTimestampName, endTimestampName,
                beanEventTypeFactory);
        }

        public static LinkedHashMap<string, object> ResolvePropertyTypes(
            IDictionary<string, object> propertyTypes,
            EventTypeNameResolver eventTypeNameResolver)
        {
            var verified = new LinkedHashMap<string, object>();
            foreach (var prop in propertyTypes) {
                var propertyName = prop.Key;
                var propertyType = prop.Value;

                if (propertyType is Type || propertyType is EventType || propertyType == null ||
                    propertyType is TypeBeanOrUnderlying) {
                    verified.Put(propertyName, propertyType);
                    continue;
                }

                if (propertyType is EventType[]) {
                    var types = (EventType[]) propertyType;
                    if (types.Length != 1 || types[0] == null) {
                        throw new ArgumentException("Invalid null event type array");
                    }

                    verified.Put(propertyName, propertyType);
                    continue;
                }

                if (propertyType is TypeBeanOrUnderlying[]) {
                    var types = (TypeBeanOrUnderlying[]) propertyType;
                    if (types.Length != 1 || types[0] == null) {
                        throw new ArgumentException("Invalid null event type array");
                    }

                    verified.Put(propertyName, propertyType);
                    continue;
                }

                if (propertyType is IDictionary<string, object>) {
                    IDictionary<string, object> inner = ResolvePropertyTypes(
                        (IDictionary<string, object>) propertyType, eventTypeNameResolver);
                    verified.Put(propertyName, inner);
                    continue;
                }

                if (!(propertyType is string)) {
                    throw MakeUnexpectedTypeException(propertyType.ToString(), propertyName);
                }

                var propertyTypeName = propertyType.ToString();
                var isArray = EventTypeUtility.IsPropertyArray(propertyTypeName);
                if (isArray) {
                    propertyTypeName = EventTypeUtility.GetPropertyRemoveArray(propertyTypeName);
                }

                var eventType = eventTypeNameResolver.GetTypeByName(propertyTypeName);
                if (!(eventType is BaseNestableEventType) && !(eventType is BeanEventType)) {
                    var clazz = TypeHelper.GetPrimitiveTypeForName(propertyTypeName);
                    if (clazz != null) {
                        verified.Put(propertyName, clazz);
                        continue;
                    }

                    throw MakeUnexpectedTypeException(propertyTypeName, propertyName);
                }

                if (eventType is BaseNestableEventType) {
                    object typeEntity = !isArray
                        ? (object) new TypeBeanOrUnderlying(eventType)
                        : (object) new[] {new TypeBeanOrUnderlying(eventType)};
                    verified.Put(propertyName, typeEntity);
                    continue;
                }

                var beanEventType = (BeanEventType) eventType;
                object type = !isArray
                    ? beanEventType.UnderlyingType
                    : TypeHelper.GetArrayType(beanEventType.UnderlyingType);
                verified.Put(propertyName, type);
            }

            return verified;
        }

        private static EPException MakeUnexpectedTypeException(
            string propertyTypeName,
            string propertyName)
        {
            return new EPException(
                "Nestable type configuration encountered an unexpected property type name '"
                + propertyTypeName + "' for property '" + propertyName +
                "', expected java.lang.Class or java.util.Map or the name of a previously-declared Map or ObjectArray type");
        }

        public static IDictionary<string, object> CheckedCastUnderlyingMap(EventBean theEvent)
        {
            return (IDictionary<string, object>) theEvent.Underlying;
        }

        public static object[] CheckedCastUnderlyingObjectArray(EventBean theEvent)
        {
            return (object[]) theEvent.Underlying;
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="index">index</param>
        /// <returns>array value or null</returns>
        public static object GetBNArrayValueAtIndex(
            object value,
            int index)
        {
            if (!(value is Array array)) {
                return null;
            }

            if (array.Length <= index) {
                return null;
            }

            return array.GetValue(index);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="index">index</param>
        /// <returns>array value or null</returns>
        public static object GetBNArrayValueAtIndexWithNullCheck(
            object value,
            int index)
        {
            if (value == null) {
                return null;
            }

            return GetBNArrayValueAtIndex(value, index);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="fragmentEventType">fragment type</param>
        /// <param name="eventBeanTypedEventFactory">event adapter service</param>
        /// <returns>fragment</returns>
        public static object HandleBNCreateFragmentMap(
            object value,
            EventType fragmentEventType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            if (!(value is Map)) {
                if (value is EventBean) {
                    return value;
                }

                return null;
            }

            var subEvent = (Map) value;
            return eventBeanTypedEventFactory.AdapterForTypedMap(subEvent, fragmentEventType);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="result">result</param>
        /// <param name="eventType">type</param>
        /// <param name="eventBeanTypedEventFactory">event service</param>
        /// <returns>fragment</returns>
        public static object GetBNFragmentPojo(
            object result,
            BeanEventType eventType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            if (result == null) {
                return null;
            }

            if (result is EventBean[]) {
                return result;
            }

            if (result is EventBean) {
                return result;
            }

            if (result is Array array) {
                var len = array.Length;
                var events = new EventBean[len];
                for (var i = 0; i < events.Length; i++) {
                    events[i] = eventBeanTypedEventFactory
                        .AdapterForTypedBean(array.GetValue(i), eventType);
                }

                return events;
            }

            return eventBeanTypedEventFactory.AdapterForTypedBean(result, eventType);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="fragmentEventType">fragment type</param>
        /// <param name="eventBeanTypedEventFactory">service</param>
        /// <returns>fragment</returns>
        public static object HandleBNCreateFragmentObjectArray(
            object value,
            EventType fragmentEventType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            if (!(value is object[])) {
                if (value is EventBean) {
                    return value;
                }

                return null;
            }

            var subEvent = (object[]) value;
            return eventBeanTypedEventFactory.AdapterForTypedObjectArray(subEvent, fragmentEventType);
        }

        public static object HandleNestedValueArrayWithMap(
            object value,
            int index,
            MapEventPropertyGetter getter)
        {
            var valueMap = GetBNArrayValueAtIndex(value, index);
            if (!(valueMap is Map)) {
                if (valueMap is EventBean) {
                    return getter.Get((EventBean) valueMap);
                }

                return null;
            }

            return getter.GetMap((IDictionary<string, object>) valueMap);
        }

        public static CodegenExpression HandleNestedValueArrayWithMapCode(
            int index,
            MapEventPropertyGetter getter,
            CodegenExpression @ref,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope,
            Type generator)
        {
            var method = CodegenLegoPropertyBeanOrUnd.From(
                codegenMethodScope, codegenClassScope, typeof(Map), getter, CodegenLegoPropertyBeanOrUnd.AccessType.GET,
                generator);
            return LocalMethod(
                method, StaticMethod(typeof(BaseNestableEventUtil), "getBNArrayValueAtIndex", @ref, Constant(index)));
        }

        public static object HandleBNNestedValueArrayWithMapFragment(
            object value,
            int index,
            MapEventPropertyGetter getter,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            EventType fragmentType)
        {
            var valueMap = GetBNArrayValueAtIndex(value, index);
            if (!(valueMap is Map)) {
                if (value is EventBean) {
                    return getter.GetFragment((EventBean) value);
                }

                return null;
            }

            // If the map does not contain the key, this is allowed and represented as null
            EventBean eventBean = eventBeanTypedEventFactory.AdapterForTypedMap(
                (IDictionary<string, object>) valueMap, fragmentType);
            return getter.GetFragment(eventBean);
        }

        public static CodegenExpression HandleBNNestedValueArrayWithMapFragmentCode(
            int index,
            MapEventPropertyGetter getter,
            CodegenExpression @ref,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            EventType fragmentType,
            Type generator)
        {
            var method = CodegenLegoPropertyBeanOrUnd.From(
                codegenMethodScope, codegenClassScope, typeof(Map), getter,
                CodegenLegoPropertyBeanOrUnd.AccessType.FRAGMENT, generator);
            return LocalMethod(
                method, StaticMethod(typeof(BaseNestableEventUtil), "getBNArrayValueAtIndex", @ref, Constant(index)));
        }

        public static bool HandleNestedValueArrayWithMapExists(
            object value,
            int index,
            MapEventPropertyGetter getter)
        {
            var valueMap = GetBNArrayValueAtIndex(value, index);
            if (!(valueMap is Map)) {
                if (valueMap is EventBean) {
                    return getter.IsExistsProperty((EventBean) valueMap);
                }

                return false;
            }

            return getter.IsMapExistsProperty((IDictionary<string, object>) valueMap);
        }

        public static CodegenExpression HandleNestedValueArrayWithMapExistsCode(
            int index,
            MapEventPropertyGetter getter,
            CodegenExpression @ref,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            EventType fragmentType,
            Type generator)
        {
            var method = CodegenLegoPropertyBeanOrUnd.From(
                codegenMethodScope, codegenClassScope, typeof(Map), getter,
                CodegenLegoPropertyBeanOrUnd.AccessType.EXISTS, generator);
            return LocalMethod(
                method, StaticMethod(typeof(BaseNestableEventUtil), "getBNArrayValueAtIndex", @ref, Constant(index)));
        }

        public static object HandleNestedValueArrayWithObjectArray(
            object value,
            int index,
            ObjectArrayEventPropertyGetter getter)
        {
            var valueArray = GetBNArrayValueAtIndex(value, index);
            if (!(valueArray is object[])) {
                if (valueArray is EventBean) {
                    return getter.Get((EventBean) valueArray);
                }

                return null;
            }

            return getter.GetObjectArray((object[]) valueArray);
        }

        public static CodegenExpression HandleNestedValueArrayWithObjectArrayCodegen(
            int index,
            ObjectArrayEventPropertyGetter getter,
            CodegenExpression @ref,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope,
            Type generator)
        {
            var method = CodegenLegoPropertyBeanOrUnd.From(
                codegenMethodScope, codegenClassScope, typeof(object[]), getter,
                CodegenLegoPropertyBeanOrUnd.AccessType.GET, generator);
            return LocalMethod(
                method, StaticMethod(typeof(BaseNestableEventUtil), "getBNArrayValueAtIndex", @ref, Constant(index)));
        }

        public static bool HandleNestedValueArrayWithObjectArrayExists(
            object value,
            int index,
            ObjectArrayEventPropertyGetter getter)
        {
            var valueArray = GetBNArrayValueAtIndex(value, index);
            if (!(valueArray is object[])) {
                if (valueArray is EventBean) {
                    return getter.IsExistsProperty((EventBean) valueArray);
                }

                return false;
            }

            return getter.IsObjectArrayExistsProperty((object[]) valueArray);
        }

        public static CodegenExpression HandleNestedValueArrayWithObjectArrayExistsCodegen(
            int index,
            ObjectArrayEventPropertyGetter getter,
            CodegenExpression @ref,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope,
            Type generator)
        {
            var method = CodegenLegoPropertyBeanOrUnd.From(
                codegenMethodScope, codegenClassScope, typeof(object[]), getter,
                CodegenLegoPropertyBeanOrUnd.AccessType.EXISTS, generator);
            return LocalMethod(
                method, StaticMethod(typeof(BaseNestableEventUtil), "getBNArrayValueAtIndex", @ref, Constant(index)));
        }

        public static object HandleNestedValueArrayWithObjectArrayFragment(
            object value,
            int index,
            ObjectArrayEventPropertyGetter getter,
            EventType fragmentType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            var valueArray = GetBNArrayValueAtIndex(value, index);
            if (!(valueArray is object[])) {
                if (value is EventBean) {
                    return getter.GetFragment((EventBean) value);
                }

                return null;
            }

            // If the map does not contain the key, this is allowed and represented as null
            EventBean eventBean =
                eventBeanTypedEventFactory.AdapterForTypedObjectArray((object[]) valueArray, fragmentType);
            return getter.GetFragment(eventBean);
        }

        public static CodegenExpression HandleNestedValueArrayWithObjectArrayFragmentCodegen(
            int index,
            ObjectArrayEventPropertyGetter getter,
            CodegenExpression @ref,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope,
            Type generator)
        {
            var method = CodegenLegoPropertyBeanOrUnd.From(
                codegenMethodScope, codegenClassScope, typeof(object[]), getter,
                CodegenLegoPropertyBeanOrUnd.AccessType.FRAGMENT, generator);
            return LocalMethod(
                method, StaticMethod(typeof(BaseNestableEventUtil), "getBNArrayValueAtIndex", @ref, Constant(index)));
        }

        public static object GetMappedPropertyValue(
            object value,
            string key)
        {
            if (value == null) {
                return null;
            }

            if (!(value is Map)) {
                return null;
            }

            var innerMap = (Map) value;
            return innerMap.Get(key);
        }

        public static bool GetMappedPropertyExists(
            object value,
            string key)
        {
            if (value == null) {
                return false;
            }

            if (!(value is Map)) {
                return false;
            }

            var innerMap = (Map) value;
            return innerMap.ContainsKey(key);
        }

        public static MapIndexedPropPair GetIndexedAndMappedProps(string[] properties)
        {
            ISet<string> mapPropertiesToCopy = new HashSet<string>();
            ISet<string> arrayPropertiesToCopy = new HashSet<string>();
            for (var i = 0; i < properties.Length; i++) {
                var prop = PropertyParser.ParseAndWalkLaxToSimple(properties[i]);
                if (prop is MappedProperty) {
                    var mappedProperty = (MappedProperty) prop;
                    mapPropertiesToCopy.Add(mappedProperty.PropertyNameAtomic);
                }

                if (prop is IndexedProperty) {
                    var indexedProperty = (IndexedProperty) prop;
                    arrayPropertiesToCopy.Add(indexedProperty.PropertyNameAtomic);
                }
            }

            return new MapIndexedPropPair(mapPropertiesToCopy, arrayPropertiesToCopy);
        }

        public static bool IsExistsIndexedValue(
            object value,
            int index)
        {
            if (value == null) {
                return false;
            }

            if (!(value is Array array))
            {
                return false;
            }

            if (index >= array.Length) {
                return false;
            }

            return true;
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="fragmentUnderlying">fragment</param>
        /// <param name="fragmentEventType">type</param>
        /// <param name="eventBeanTypedEventFactory">svc</param>
        /// <returns>bean</returns>
        public static EventBean GetBNFragmentNonPojo(
            object fragmentUnderlying,
            EventType fragmentEventType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            if (fragmentUnderlying == null) {
                return null;
            }

            if (fragmentEventType is MapEventType) {
                return eventBeanTypedEventFactory.AdapterForTypedMap(
                    (IDictionary<string, object>) fragmentUnderlying, fragmentEventType);
            }

            return eventBeanTypedEventFactory.AdapterForTypedObjectArray(
                (object[]) fragmentUnderlying, fragmentEventType);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="fragmentEventType">fragment type</param>
        /// <param name="eventBeanTypedEventFactory">svc</param>
        /// <returns>fragment</returns>
        public static object GetBNFragmentArray(
            object value,
            EventType fragmentEventType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            if (value is object[]) {
                var subEvents = (object[]) value;

                var countNull = 0;
                foreach (var subEvent in subEvents) {
                    if (subEvent != null) {
                        countNull++;
                    }
                }

                var outEvents = new EventBean[countNull];
                var count = 0;
                foreach (var item in subEvents) {
                    if (item != null) {
                        outEvents[count++] = GetBNFragmentNonPojo(item, fragmentEventType, eventBeanTypedEventFactory);
                    }
                }

                return outEvents;
            }

            if (!(value is Map[])) {
                return null;
            }

            var mapTypedSubEvents = (Map[]) value;

            var countNull = 0;
            foreach (var map in mapTypedSubEvents) {
                if (map != null) {
                    countNull++;
                }
            }

            var mapEvents = new EventBean[countNull];
            var count = 0;
            foreach (var map in mapTypedSubEvents) {
                if (map != null) {
                    mapEvents[count++] = eventBeanTypedEventFactory.AdapterForTypedMap(map, fragmentEventType);
                }
            }

            return mapEvents;
        }

        public static object GetBeanArrayValue(
            BeanEventPropertyGetter nestedGetter,
            object value,
            int index)
        {
            if (value == null) {
                return null;
            }

            if (!(value is Array array)) {
                return null;
            }

            if (array.Length <= index) {
                return null;
            }

            object arrayItem = array.GetValue(index);
            if (arrayItem == null) {
                return null;
            }

            return nestedGetter.GetBeanProp(arrayItem);
        }

        public static CodegenMethod GetBeanArrayValueCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope,
            BeanEventPropertyGetter nestedGetter,
            int index)
        {
            return codegenMethodScope.MakeChild(typeof(object), typeof(BaseNestableEventUtil), codegenClassScope)
                .AddParam(typeof(object), "value").Block
                .IfRefNullReturnNull("value")
                .IfConditionReturnConst(Not(ExprDotMethodChain(Ref("value")).Add("getClass").Add("isArray")), null)
                .IfConditionReturnConst(
                    Relational(StaticMethod(typeof(Array), "getLength", Ref("value")), LE, Constant(index)), null)
                .DeclareVar(
                    typeof(object), "arrayItem", StaticMethod(typeof(Array), "get", Ref("value"), Constant(index)))
                .IfRefNullReturnNull("arrayItem")
                .MethodReturn(
                    nestedGetter.UnderlyingGetCodegen(
                        Cast(nestedGetter.TargetType, Ref("arrayItem")), codegenMethodScope, codegenClassScope));
        }

        public static object GetArrayPropertyValue(
            EventBean[] wrapper,
            int index,
            EventPropertyGetter nestedGetter)
        {
            if (wrapper == null) {
                return null;
            }

            if (wrapper.Length <= index) {
                return null;
            }

            var innerArrayEvent = wrapper[index];
            return nestedGetter.Get(innerArrayEvent);
        }

        public static CodegenMethod GetArrayPropertyValueCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope,
            int index,
            EventPropertyGetterSPI nestedGetter)
        {
            return codegenMethodScope.MakeChild(typeof(object), typeof(BaseNestableEventUtil), codegenClassScope)
                .AddParam(typeof(EventBean[]), "wrapper").Block
                .IfRefNullReturnNull("wrapper")
                .IfConditionReturnConst(Relational(ArrayLength(Ref("wrapper")), LE, Constant(index)), null)
                .DeclareVar(typeof(EventBean), "inner", ArrayAtIndex(Ref("wrapper"), Constant(index)))
                .MethodReturn(nestedGetter.EventBeanGetCodegen(Ref("inner"), codegenMethodScope, codegenClassScope));
        }

        public static object GetArrayPropertyFragment(
            EventBean[] wrapper,
            int index,
            EventPropertyGetter nestedGetter)
        {
            if (wrapper == null) {
                return null;
            }

            if (wrapper.Length <= index) {
                return null;
            }

            var innerArrayEvent = wrapper[index];
            return nestedGetter.GetFragment(innerArrayEvent);
        }

        public static CodegenMethod GetArrayPropertyFragmentCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope,
            int index,
            EventPropertyGetterSPI nestedGetter)
        {
            return codegenMethodScope.MakeChild(typeof(object), typeof(BaseNestableEventUtil), codegenClassScope)
                .AddParam(typeof(EventBean[]), "wrapper").Block
                .IfRefNullReturnNull("wrapper")
                .IfConditionReturnConst(Relational(ArrayLength(Ref("wrapper")), LE, Constant(index)), null)
                .DeclareVar(typeof(EventBean), "inner", ArrayAtIndex(Ref("wrapper"), Constant(index)))
                .MethodReturn(
                    nestedGetter.EventBeanFragmentCodegen(Ref("inner"), codegenMethodScope, codegenClassScope));
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="wrapper">beans</param>
        /// <param name="index">index</param>
        /// <returns>underlying</returns>
        public static object GetBNArrayPropertyUnderlying(
            EventBean[] wrapper,
            int index)
        {
            if (wrapper == null) {
                return null;
            }

            if (wrapper.Length <= index) {
                return null;
            }

            return wrapper[index].Underlying;
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="wrapper">beans</param>
        /// <param name="index">index</param>
        /// <returns>fragment</returns>
        public static object GetBNArrayPropertyBean(
            EventBean[] wrapper,
            int index)
        {
            if (wrapper == null) {
                return null;
            }

            if (wrapper.Length <= index) {
                return null;
            }

            return wrapper[index];
        }

        public static object GetArrayPropertyAsUnderlyingsArray(
            Type underlyingType,
            EventBean[] wrapper)
        {
            if (wrapper != null) {
                var array = Array.CreateInstance(underlyingType, wrapper.Length);
                for (var i = 0; i < wrapper.Length; i++) {
                    array.SetValue(wrapper[i].Underlying, i);
                }

                return array;
            }

            return null;
        }

        public static CodegenMethod GetArrayPropertyAsUnderlyingsArrayCodegen(
            Type underlyingType,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(typeof(object), typeof(BaseNestableEventUtil), codegenClassScope)
                .AddParam(typeof(EventBean[]), "wrapper").Block
                .IfRefNullReturnNull("wrapper")
                .DeclareVar(
                    TypeHelper.GetArrayType(underlyingType), "array",
                    NewArrayByLength(underlyingType, ArrayLength(Ref("wrapper"))))
                .ForLoopIntSimple("i", ArrayLength(Ref("wrapper")))
                .AssignArrayElement(
                    "array", Ref("i"),
                    Cast(underlyingType, ExprDotMethod(ArrayAtIndex(Ref("wrapper"), Ref("i")), "getUnderlying")))
                .BlockEnd()
                .MethodReturn(Ref("array"));
        }

        public static ExprValidationException ComparePropType(
            string propName,
            object setOneType,
            object setTwoType,
            bool setTwoTypeFound,
            string otherName)
        {
            // allow null for nested event types
            if (IsNestedType(setOneType) && setTwoType == null) {
                return null;
            }

            if (IsNestedType(setTwoType) && setOneType == null) {
                return null;
            }

            if (!setTwoTypeFound) {
                return new ExprValidationException("The property '" + propName + "' is not provided but required");
            }

            if (setTwoType == null) {
                return null;
            }

            if (setOneType == null) {
                return new ExprValidationException(
                    "Type by name '" + otherName + "' in property '" + propName +
                    "' incompatible with null-type or property name not found in target");
            }

            if (setTwoType is Type && setOneType is Type) {
                var boxedOther = ((Type) setTwoType).GetBoxedType();
                var boxedThis = ((Type) setOneType).GetBoxedType();
                if (!boxedOther.Equals(boxedThis)) {
                    if (!TypeHelper.IsSubclassOrImplementsInterface(boxedOther, boxedThis)) {
                        return new ExprValidationException(
                            "Type by name '" + otherName + "' in property '" + propName + "' expected " + boxedThis +
                            " but receives " + boxedOther);
                    }
                }
            }
            else if (setTwoType is BeanEventType && setOneType is Type) {
                var boxedOther = ((BeanEventType) setTwoType).UnderlyingType.GetBoxedType();
                var boxedThis = ((Type) setOneType).GetBoxedType();
                if (!boxedOther.Equals(boxedThis)) {
                    return new ExprValidationException(
                        "Type by name '" + otherName + "' in property '" + propName + "' expected " + boxedThis +
                        " but receives " + boxedOther);
                }
            }
            else if (setTwoType is EventType[] && ((EventType[]) setTwoType)[0] is BeanEventType &&
                     setOneType is Type && ((Type) setOneType).IsArray) {
                var boxedOther = ((EventType[]) setTwoType)[0].UnderlyingType.GetBoxedType();
                var boxedThis = ((Type) setOneType).GetElementType().GetBoxedType();
                if (!boxedOther.Equals(boxedThis)) {
                    return new ExprValidationException(
                        "Type by name '" + otherName + "' in property '" + propName + "' expected " + boxedThis +
                        " but receives " + boxedOther);
                }
            }
            else if (setTwoType is Map && setOneType is Map) {
                var messageIsDeepEquals = BaseNestableEventType.IsDeepEqualsProperties(
                    propName, (IDictionary<string, object>) setOneType, (IDictionary<string, object>) setTwoType);
                if (messageIsDeepEquals != null) {
                    return messageIsDeepEquals;
                }
            }
            else if (setTwoType is EventType && setOneType is EventType) {
                bool mismatch;
                if (setTwoType is EventTypeSPI && setOneType is EventTypeSPI) {
                    var compared =
                        ((EventTypeSPI) setOneType).EqualsCompareType((EventTypeSPI) setTwoType);
                    mismatch = compared != null;
                }
                else {
                    mismatch = !setOneType.Equals(setTwoType);
                }

                if (mismatch) {
                    var setOneEventType = (EventType) setOneType;
                    var setTwoEventType = (EventType) setTwoType;
                    return GetMismatchMessageEventType(otherName, propName, setOneEventType, setTwoEventType);
                }
            }
            else if (setTwoType is TypeBeanOrUnderlying && setOneType is EventType) {
                var setOneEventType = (EventType) setOneType;
                var setTwoEventType = ((TypeBeanOrUnderlying) setTwoType).EventType;
                if (!EventTypeUtility.IsTypeOrSubTypeOf(setTwoEventType, setOneEventType)) {
                    return GetMismatchMessageEventType(otherName, propName, setOneEventType, setTwoEventType);
                }
            }
            else if (setTwoType is EventType && setOneType is TypeBeanOrUnderlying) {
                var setOneEventType = ((TypeBeanOrUnderlying) setOneType).EventType;
                var setTwoEventType = (EventType) setTwoType;
                if (!EventTypeUtility.IsTypeOrSubTypeOf(setTwoEventType, setOneEventType)) {
                    return GetMismatchMessageEventType(otherName, propName, setOneEventType, setTwoEventType);
                }
            }
            else if (setTwoType is TypeBeanOrUnderlying && setOneType is TypeBeanOrUnderlying) {
                var setOneEventType = ((TypeBeanOrUnderlying) setOneType).EventType;
                var setTwoEventType = ((TypeBeanOrUnderlying) setTwoType).EventType;
                if (!EventTypeUtility.IsTypeOrSubTypeOf(setOneEventType, setTwoEventType)) {
                    return GetMismatchMessageEventType(otherName, propName, setOneEventType, setTwoEventType);
                }
            }
            else if (setTwoType is EventType[] && setOneType is TypeBeanOrUnderlying[]) {
                var setTwoEventType = ((EventType[]) setTwoType)[0];
                var setOneEventType = ((TypeBeanOrUnderlying[]) setOneType)[0].EventType;
                if (!EventTypeUtility.IsTypeOrSubTypeOf(setOneEventType, setTwoEventType)) {
                    return GetMismatchMessageEventType(otherName, propName, setOneEventType, setTwoEventType);
                }
            }
            else {
                var typeOne = GetTypeName(setOneType);
                var typeTwo = GetTypeName(setTwoType);
                if (typeOne.Equals(typeTwo)) {
                    return null;
                }

                return new ExprValidationException(
                    "Type by name '" + otherName + "' in property '" + propName + "' expected " + typeOne +
                    " but receives " + typeTwo);
            }

            return null;
        }

        private static ExprValidationException GetMismatchMessageEventType(
            string otherName,
            string propName,
            EventType setOneEventType,
            EventType setTwoEventType)
        {
            return new ExprValidationException(
                "Type by name '" + otherName + "' in property '" + propName + "' expected event type '" +
                setOneEventType.Name + "' but receives event type '" + setTwoEventType.Name + "'");
        }

        private static bool IsNestedType(object type)
        {
            return type is TypeBeanOrUnderlying ||
                   type is EventType ||
                   type is TypeBeanOrUnderlying[] ||
                   type is EventType[];
        }

        private static string GetTypeName(object type)
        {
            if (type == null) {
                return "null";
            }

            if (type is Type) {
                return ((Type) type).Name;
            }

            if (type is EventType) {
                return "event type '" + ((EventType) type).Name + "'";
            }

            if (type is EventType[]) {
                return "event type array '" + ((EventType[]) type)[0].Name + "'";
            }

            if (type is TypeBeanOrUnderlying) {
                return "event type '" + ((TypeBeanOrUnderlying) type).EventType.Name + "'";
            }

            if (type is TypeBeanOrUnderlying[]) {
                return "event type array '" + ((TypeBeanOrUnderlying[]) type)[0].EventType.Name + "'";
            }

            return type.GetType().Name;
        }

        public class MapIndexedPropPair
        {
            public MapIndexedPropPair(
                ISet<string> mapProperties,
                ISet<string> arrayProperties)
            {
                MapProperties = mapProperties;
                ArrayProperties = arrayProperties;
            }

            public ISet<string> MapProperties { get; }

            public ISet<string> ArrayProperties { get; }
        }
    }
} // end of namespace