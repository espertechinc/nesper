///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.codegen.core;
using com.espertech.esper.codegen.model.blocks;
using com.espertech.esper.codegen.model.expression;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.magic;
using com.espertech.esper.events.arr;
using com.espertech.esper.events.bean;
using com.espertech.esper.events.map;
using com.espertech.esper.events.property;
using com.espertech.esper.util;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events
{
    using Map = IDictionary<string, object>;

    public class BaseNestableEventUtil
    {
        public static IDictionary<string, object> CheckedCastUnderlyingMap(EventBean theEvent)
        {
            return (IDictionary<string, object>) theEvent.Underlying;
        }

        public static object[] CheckedCastUnderlyingObjectArray(EventBean theEvent)
        {
            return (object[]) theEvent.Underlying;
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="index">index</param>
        /// <returns>array value or null</returns>
        public static object GetBNArrayValueAtIndex(object value, int index)
        {
            if (value is Array array)
            {
                if (array.Length <= index)
                {
                    return null;
                }

                return array.GetValue(index);
            }

            return null;
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="index">index</param>
        /// <returns>array value or null</returns>
        public static object GetBNArrayValueAtIndexWithNullCheck(object value, int index)
        {
            if (value == null)
            {
                return null;
            }

            return GetBNArrayValueAtIndex(value, index);
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="fragmentEventType">fragment type</param>
        /// <param name="eventAdapterService">event adapter service</param>
        /// <returns>fragment</returns>
        public static object HandleBNCreateFragmentMap(
            object value, 
            EventType fragmentEventType,
            EventAdapterService eventAdapterService)
        {
            if (value == null) {
                return null;
            }

            if (value is Map valueAsMap) {
                return eventAdapterService.AdapterForTypedMap(valueAsMap, fragmentEventType);
            }

            if (value.GetType().IsGenericStringDictionary()) {
                var valueAsStringMap = value.AsStringDictionary();
                return eventAdapterService.AdapterForTypedMap(valueAsStringMap, fragmentEventType);
            }

            if (value is EventBean) {
                return value;
            }

            return null;
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="result">result</param>
        /// <param name="eventType">type</param>
        /// <param name="eventAdapterService">event service</param>
        /// <returns>fragment</returns>
        public static object GetBNFragmentPono(
            object result, 
            BeanEventType eventType,
            EventAdapterService eventAdapterService)
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

            if (result is Array valueAsArray)
            {
                var len = valueAsArray.Length;
                var events = new EventBean[len];
                for (var i = 0; i < events.Length; i++)
                {
                    events[i] = eventAdapterService.AdapterForTypedObject(
                        valueAsArray.GetValue(i), eventType);
                }

                return events;
            }

            return eventAdapterService.AdapterForTypedObject(result, eventType);
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="fragmentEventType">fragment type</param>
        /// <param name="eventAdapterService">service</param>
        /// <returns>fragment</returns>
        public static object HandleBNCreateFragmentObjectArray(object value, EventType fragmentEventType,
            EventAdapterService eventAdapterService)
        {
            if (value is object[] valueAsArray)
            {
                return eventAdapterService.AdapterForTypedObjectArray(valueAsArray, fragmentEventType);
            }

            if (value is EventBean)
            {
                return value;
            }

            return null;
        }

        public static object HandleNestedValueArrayWithMap(
            object value, 
            int index, 
            MapEventPropertyGetter getter)
        {
            var valueMap = GetBNArrayValueAtIndex(value, index);
            if (valueMap is Map valueAsMap)
            {
                return getter.GetMap(valueAsMap);
            }

            if (valueMap is EventBean eventBean)
            {
                return getter.Get(eventBean);
            }

            return null;
        }

        public static ICodegenExpression HandleNestedValueArrayWithMapCode(
            int index, 
            MapEventPropertyGetter getter, 
            ICodegenExpression @ref, 
            ICodegenContext context, 
            Type generator)
        {
            var method = CodegenBlockPropertyBeanOrUnd.From(context, typeof(Map), getter,
                CodegenBlockPropertyBeanOrUnd.AccessType.GET, generator);
            return LocalMethod(method, StaticMethodTakingExprAndConst(typeof(BaseNestableEventUtil), "GetBNArrayValueAtIndex", @ref, index));
        }

        public static object HandleBNNestedValueArrayWithMapFragment(object value, int index,
            MapEventPropertyGetter getter, EventAdapterService eventAdapterService, EventType fragmentType)
        {
            var valueMap = GetBNArrayValueAtIndex(value, index);
            if (valueMap is Map valueMapAsMap)
            {
                // If the map does not contain the key, this is allowed and represented as null
                var eventBean = eventAdapterService.AdapterForTypedMap(valueMapAsMap, fragmentType);
                return getter.GetFragment(eventBean);
            }

            if (value is EventBean bean)
            {
                return getter.GetFragment(bean);
            }

            return null;
        }

        public static ICodegenExpression HandleBNNestedValueArrayWithMapFragmentCode(
            int index,
            MapEventPropertyGetter getter,
            ICodegenExpression @ref,
            ICodegenContext context,
            EventAdapterService eventAdapterService,
            EventType fragmentType,
            Type generator)
        {
            var method = CodegenBlockPropertyBeanOrUnd.From(
                context, typeof(Map), getter,
                CodegenBlockPropertyBeanOrUnd.AccessType.FRAGMENT, generator);
            return LocalMethod(
                method, StaticMethodTakingExprAndConst(
                    typeof(BaseNestableEventUtil), "GetBNArrayValueAtIndex", @ref,
                    index));
        }

        public static bool HandleNestedValueArrayWithMapExists(
            object value,
            int index, 
            MapEventPropertyGetter getter)
        {
            var valueResult = GetBNArrayValueAtIndex(value, index);
            if (!(valueResult is Map valueMap))
            {
                if (valueResult is EventBean eventBean)
                {
                    return getter.IsExistsProperty(eventBean);
                }

                return false;
            }

            return getter.IsMapExistsProperty(valueMap);
        }

        public static ICodegenExpression HandleNestedValueArrayWithMapExistsCode(int index,
            MapEventPropertyGetter getter,
            ICodegenExpression @ref,
            ICodegenContext context,
            EventAdapterService eventAdapterService, 
            EventType fragmentType,
            Type generator)
        {
            var method = CodegenBlockPropertyBeanOrUnd.From(context, typeof(Map), getter,
                CodegenBlockPropertyBeanOrUnd.AccessType.EXISTS, generator);
            return LocalMethod(method,
                StaticMethodTakingExprAndConst(typeof(BaseNestableEventUtil), "GetBNArrayValueAtIndex", @ref,
                    index));
        }

        public static object HandleNestedValueArrayWithObjectArray(
            object value,
            int index,
            ObjectArrayEventPropertyGetter getter)
        {
            var valueArray = GetBNArrayValueAtIndex(value, index);
            if (valueArray is object[] valueArrayReal)
            {
                return getter.GetObjectArray(valueArrayReal);
            }

            if (valueArray is EventBean)
            {
                return getter.Get((EventBean) valueArray);
            }

            return null;
        }

        public static ICodegenExpression HandleNestedValueArrayWithObjectArrayCodegen(
            int index,
            ObjectArrayEventPropertyGetter getter,
            ICodegenExpression @ref, 
            ICodegenContext context, 
            Type generator)
        {
            var method = CodegenBlockPropertyBeanOrUnd.From(context, typeof(object[]), getter,
                CodegenBlockPropertyBeanOrUnd.AccessType.GET, generator);
            return LocalMethod(method,
                StaticMethodTakingExprAndConst(typeof(BaseNestableEventUtil), "GetBNArrayValueAtIndex", @ref,
                    index));
        }

        public static bool HandleNestedValueArrayWithObjectArrayExists(
            object value,
            int index,
            ObjectArrayEventPropertyGetter getter)
        {
            var valueArray = GetBNArrayValueAtIndex(value, index);
            if (valueArray is object[] valueArrayReal)
            {
                return getter.IsObjectArrayExistsProperty(valueArrayReal);
            }

            if (valueArray is EventBean eventBean)
            {
                return getter.IsExistsProperty(eventBean);
            }

            return false;
        }

        public static ICodegenExpression HandleNestedValueArrayWithObjectArrayExistsCodegen(
            int index,
            ObjectArrayEventPropertyGetter getter, 
            ICodegenExpression @ref,
            ICodegenContext context,
            Type generator)
        {
            var method = CodegenBlockPropertyBeanOrUnd.From(context, typeof(object[]), getter,
                CodegenBlockPropertyBeanOrUnd.AccessType.EXISTS, generator);
            return LocalMethod(method,
                StaticMethodTakingExprAndConst(typeof(BaseNestableEventUtil), "GetBNArrayValueAtIndex", @ref,
                    index));
        }

        public static object HandleNestedValueArrayWithObjectArrayFragment(
            object value, 
            int index,
            ObjectArrayEventPropertyGetter getter, 
            EventType fragmentType,
            EventAdapterService eventAdapterService)
        {
            var valueArray = GetBNArrayValueAtIndex(value, index);
            if (!(valueArray is object[]))
            {
                if (value is EventBean)
                {
                    return getter.GetFragment((EventBean) value);
                }

                return null;
            }

            // If the map does not contain the key, this is allowed and represented as null
            var eventBean = eventAdapterService.AdapterForTypedObjectArray((object[]) valueArray, fragmentType);
            return getter.GetFragment(eventBean);
        }

        public static ICodegenExpression HandleNestedValueArrayWithObjectArrayFragmentCodegen(
            int index,
            ObjectArrayEventPropertyGetter getter, 
            ICodegenExpression @ref, 
            ICodegenContext context, 
            Type generator)
        {
            var method = CodegenBlockPropertyBeanOrUnd.From(context, typeof(object[]), getter,
                CodegenBlockPropertyBeanOrUnd.AccessType.FRAGMENT, generator);
            return LocalMethod(method,
                StaticMethodTakingExprAndConst(typeof(BaseNestableEventUtil), "GetBNArrayValueAtIndex", @ref,
                    index));
        }

        public static object GetMappedPropertyValue(object value, string key)
        {
            if (value == null) {
                return null;
            }
            if (value is Map innerMap) {
                return innerMap.Get(key);
            }
            if (value.GetType().IsGenericStringDictionary()) {
                return value.SDGet(key);
            }

            return null;
        }

        public static bool GetMappedPropertyExists(object value, string key)
        {
            if (value == null) {
                return false;
            }
            if (value is Map innerMap) {
                return innerMap.ContainsKey(key);
            }
            if (value.GetType().IsGenericStringDictionary()) {
                return value.SDContainsKey(key);
            }

            return false;
        }

        public static MapIndexedPropPair GetIndexedAndMappedProps(string[] properties)
        {
            var mapPropertiesToCopy = new HashSet<string>();
            var arrayPropertiesToCopy = new HashSet<string>();
            for (var i = 0; i < properties.Length; i++)
            {
                var prop = PropertyParser.ParseAndWalkLaxToSimple(properties[i]);
                if (prop is MappedProperty)
                {
                    var mappedProperty = (MappedProperty) prop;
                    mapPropertiesToCopy.Add(mappedProperty.PropertyNameAtomic);
                }

                if (prop is IndexedProperty)
                {
                    var indexedProperty = (IndexedProperty) prop;
                    arrayPropertiesToCopy.Add(indexedProperty.PropertyNameAtomic);
                }
            }

            return new MapIndexedPropPair(mapPropertiesToCopy, arrayPropertiesToCopy);
        }

        public static bool IsExistsIndexedValue(object value, int index)
        {
            if (value is Array valueAsArray)
            {
                return index < valueAsArray.Length;
            }

            return false;
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="fragmentUnderlying">fragment</param>
        /// <param name="fragmentEventType">type</param>
        /// <param name="eventAdapterService">svc</param>
        /// <returns>bean</returns>
        public static EventBean GetBNFragmentNonPono(
            object fragmentUnderlying, 
            EventType fragmentEventType,
            EventAdapterService eventAdapterService)
        {
            if (fragmentUnderlying == null)
            {
                return null;
            }

            if (fragmentEventType is MapEventType)
            {
                return eventAdapterService.AdapterForTypedMap((IDictionary<string, object>) fragmentUnderlying,
                    fragmentEventType);
            }

            return eventAdapterService.AdapterForTypedObjectArray((object[]) fragmentUnderlying, fragmentEventType);
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="fragmentEventType">fragment type</param>
        /// <param name="eventAdapterService">svc</param>
        /// <returns>fragment</returns>
        public static object GetBNFragmentArray(
            object value, 
            EventType fragmentEventType,
            EventAdapterService eventAdapterService)
        {
            if (value is object[] subEvents)
            {
                var countNullInner = 0;
                foreach (var subEvent in subEvents)
                {
                    if (subEvent != null)
                    {
                        countNullInner++;
                    }
                }

                var outEvents = new EventBean[countNullInner];
                var countInner = 0;
                foreach (var item in subEvents)
                {
                    if (item != null)
                    {
                        outEvents[countInner++] = GetBNFragmentNonPono(item, fragmentEventType, eventAdapterService);
                    }
                }

                return outEvents;
            }

            if (!(value is Map[]))
            {
                return null;
            }

            var mapTypedSubEvents = (Map[]) value;

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

        public static object GetBeanArrayValue(BeanEventPropertyGetter nestedGetter, object value, int index)
        {
            if (value is Array valueAsArray)
            {
                if (valueAsArray.Length <= index)
                {
                    return null;
                }

                var arrayItem = valueAsArray.GetValue(index);
                if (arrayItem == null)
                {
                    return null;
                }

                return nestedGetter.GetBeanProp(arrayItem);
            }

            return null;
        }

        public static string GetBeanArrayValueCodegen(
            ICodegenContext context,
            BeanEventPropertyGetter nestedGetter,
            int index)
        {
            return context.AddMethod(typeof(object), typeof(object), "value", typeof(BaseNestableEventUtil))
                .IfRefNullReturnNull("value")
                .IfConditionReturnConst(
                    Not(ExprDotMethodChain(Ref("value")).AddNoParam("getClass")
                        .AddNoParam("isArray")), null)
                .IfConditionReturnConst(
                    Relational(StaticMethod(typeof(Array), "getLength", Ref("value")),
                        CodegenRelational.LE, Constant(index)), null)
                .DeclareVar(typeof(object), "arrayItem",
                    StaticMethod(typeof(Array), "get", Ref("value"), Constant(index)))
                .IfRefNullReturnNull("arrayItem")
                .MethodReturn(
                    nestedGetter.CodegenUnderlyingGet(Cast(nestedGetter.TargetType, Ref("arrayItem")),
                        context));
        }

        public static object GetArrayPropertyValue(EventBean[] wrapper, int index, EventPropertyGetter nestedGetter)
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

        public static string GetArrayPropertyValueCodegen(ICodegenContext context, int index,
            EventPropertyGetterSPI nestedGetter)
        {
            return context.AddMethod(typeof(object), typeof(EventBean[]), "wrapper", typeof(BaseNestableEventUtil))
                .IfRefNullReturnNull("wrapper")
                .IfConditionReturnConst(
                    Relational(ArrayLength(Ref("wrapper")), CodegenRelational.LE,
                        Constant(index)), null)
                .DeclareVar(typeof(EventBean), "inner",
                    ArrayAtIndex(Ref("wrapper"), Constant(index)))
                .MethodReturn(nestedGetter.CodegenEventBeanGet(Ref("inner"), context));
        }

        public static object GetArrayPropertyFragment(EventBean[] wrapper, int index, EventPropertyGetter nestedGetter)
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

        public static string GetArrayPropertyFragmentCodegen(
            ICodegenContext context,
            int index,
            EventPropertyGetterSPI nestedGetter)
        {
            return context.AddMethod(typeof(object), typeof(EventBean[]), "wrapper", typeof(BaseNestableEventUtil))
                .IfRefNullReturnNull("wrapper")
                .IfConditionReturnConst(
                    Relational(ArrayLength(Ref("wrapper")), CodegenRelational.LE,
                        Constant(index)), null)
                .DeclareVar(typeof(EventBean), "inner",
                    ArrayAtIndex(Ref("wrapper"), Constant(index)))
                .MethodReturn(nestedGetter.CodegenEventBeanFragment(Ref("inner"), context));
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="wrapper">beans</param>
        /// <param name="index">index</param>
        /// <returns>underlying</returns>
        public static object GetBNArrayPropertyUnderlying(EventBean[] wrapper, int index)
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

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="wrapper">beans</param>
        /// <param name="index">index</param>
        /// <returns>fragment</returns>
        public static object GetBNArrayPropertyBean(EventBean[] wrapper, int index)
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

        public static object GetArrayPropertyAsUnderlyingsArray(Type underlyingType, EventBean[] wrapper)
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

        public static string GetArrayPropertyAsUnderlyingsArrayCodegen(Type underlyingType, ICodegenContext context)
        {
            return context.AddMethod(typeof(object), typeof(EventBean[]), "wrapper", typeof(BaseNestableEventUtil))
                .IfRefNullReturnNull("wrapper")
                .DeclareVar(TypeHelper.GetArrayType(underlyingType), "array",
                    NewArray(underlyingType, ArrayLength(Ref("wrapper"))))
                .ForLoopInt("i", ArrayLength(Ref("wrapper")))
                .AssignArrayElement("array", Ref("i"),
                    Cast(underlyingType,
                        ExprDotMethod(ArrayAtIndex(Ref("wrapper"), Ref("i")),
                            "getUnderlying")))
                .BlockEnd()
                .MethodReturn(Ref("array"));
        }

        public static string ComparePropType(
            string propName, 
            object setOneType, 
            object setTwoType,
            bool setTwoTypeFound, 
            string otherName)
        {
            // allow null for nested event types
            if ((setOneType is string || setOneType is EventType) && setTwoType == null)
            {
                return null;
            }

            if ((setTwoType is string || setTwoType is EventType) && setOneType == null)
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
                return "Type by name '" + otherName + "' in property '" + propName +
                       "' incompatible with null-type or property name not found in target";
            }

            if ((setTwoType is Type) && (setOneType is Type))
            {
                var boxedOther = ((Type) setTwoType).GetBoxedType();
                var boxedThis = ((Type) setOneType).GetBoxedType();
                if (boxedOther != boxedThis)
                {
                    if (!TypeHelper.IsSubclassOrImplementsInterface(boxedOther, boxedThis))
                    {
                        return "Type by name '" + otherName + "' in property '" + propName + "' expected " + Name.Clean(boxedThis) +
                               " but receives " + Name.Clean(boxedOther);
                    }
                }
            }
            else if ((setTwoType is BeanEventType) && (setOneType is Type))
            {
                var boxedOther = ((BeanEventType) setTwoType).UnderlyingType.GetBoxedType();
                var boxedThis = ((Type) setOneType).GetBoxedType();
                if (boxedOther != boxedThis)
                {
                    return "Type by name '" + otherName + "' in property '" + propName + "' expected " + Name.Clean(boxedThis) +
                           " but receives " + Name.Clean(boxedOther);
                }
            }
            else if (setTwoType is EventType[] && ((EventType[]) setTwoType)[0] is BeanEventType &&
                     setOneType is Type && ((Type) setOneType).IsArray)
            {
                var boxedOther = (((EventType[]) setTwoType)[0]).UnderlyingType.GetBoxedType();
                var boxedThis = ((Type) setOneType).GetElementType().GetBoxedType();
                if (boxedOther != boxedThis)
                {
                    return "Type by name '" + otherName + "' in property '" + propName + "' expected " + Name.Clean(boxedThis) +
                           " but receives " + Name.Clean(boxedOther);
                }
            }
            else if ((setTwoType is Map) && (setOneType is Map))
            {
                var messageIsDeepEquals = BaseNestableEventType.IsDeepEqualsProperties(propName,
                    (IDictionary<string, object>) setOneType, (IDictionary<string, object>) setTwoType);
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
                    mismatch = !((EventTypeSPI) setOneType).EqualsCompareType((EventTypeSPI) setTwoType);
                }
                else
                {
                    mismatch = !setOneType.Equals(setTwoType);
                }

                if (mismatch)
                {
                    var setOneEventType = (EventType) setOneType;
                    var setTwoEventType = (EventType) setTwoType;
                    return "Type by name '" + otherName + "' in property '" + propName + "' expected event type '" +
                           setOneEventType.Name + "' but receives event type '" + setTwoEventType.Name + "'";
                }
            }
            else if ((setTwoType is string) && (setOneType is EventType))
            {
                var setOneEventType = (EventType) setOneType;
                var setTwoEventType = (string) setTwoType;
                if (!EventTypeUtility.IsTypeOrSubTypeOf(setTwoEventType, setOneEventType))
                {
                    return "Type by name '" + otherName + "' in property '" + propName + "' expected event type '" +
                           setOneEventType.Name + "' but receives event type '" + setTwoEventType + "'";
                }
            }
            else if ((setTwoType is EventType) && (setOneType is string))
            {
                var setTwoEventType = (EventType) setTwoType;
                var setOneEventType = (string) setOneType;
                if (!EventTypeUtility.IsTypeOrSubTypeOf(setOneEventType, setTwoEventType))
                {
                    return "Type by name '" + otherName + "' in property '" + propName + "' expected event type '" +
                           setOneEventType + "' but receives event type '" + setTwoEventType.Name + "'";
                }
            }
            else if ((setTwoType is string) && (setOneType is string))
            {
                if (!setTwoType.Equals(setOneType))
                {
                    var setOneEventType = (string) setOneType;
                    var setTwoEventType = (string) setTwoType;
                    return "Type by name '" + otherName + "' in property '" + propName + "' expected event type '" +
                           setOneEventType + "' but receives event type '" + setTwoEventType + "'";
                }
            }
            else if ((setTwoType is EventType[]) && (setOneType is string))
            {
                var setTwoTypeArr = (EventType[]) setTwoType;
                var setTwoFragmentType = setTwoTypeArr[0];
                var setOneTypeString = (string) setOneType;
                if (!(setOneTypeString.EndsWith("[]")))
                {
                    return "Type by name '" + otherName + "' in property '" + propName + "' expected event type '" +
                           setOneType + "' but receives event type '" + setTwoFragmentType.Name + "[]'";
                }

                var setOneTypeNoArray = setOneTypeString.RegexReplaceAll("\\[\\]", "");
                if (!(setTwoFragmentType.Name.Equals(setOneTypeNoArray)))
                {
                    return "Type by name '" + otherName + "' in property '" + propName + "' expected event type '" +
                           setOneTypeNoArray + "[]' but receives event type '" + setTwoFragmentType.Name + "'";
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

                return "Type by name '" + otherName + "' in property '" + propName + "' expected " + typeOne +
                       " but receives " + typeTwo;
            }

            return null;
        }

        private static string GetTypeName(object type)
        {
            if (type == null)
            {
                return "null";
            }

            if (type is Type typeValue)
            {
                return typeValue.FullName;
            }

            if (type is EventType eventType)
            {
                return "event type '" + eventType.Name + "'";
            }

            if (type is EventType[] eventTypeArray)
            {
                return "event type array '" + eventTypeArray[0].Name + "'";
            }

            if (type is string typeAsString)
            {
                var boxedType = TypeHelper.GetPrimitiveTypeForName(typeAsString).GetBoxedType();
                if (boxedType != null)
                {
                    return Name.Clean(boxedType);
                }

                return typeAsString;
            }

            return Name.Clean(type.GetType());
        }

        public class MapIndexedPropPair
        {
            public MapIndexedPropPair(ICollection<string> mapProperties, ICollection<string> arrayProperties)
            {
                MapProperties = mapProperties;
                ArrayProperties = arrayProperties;
            }

            public ICollection<string> MapProperties { get; }

            public ICollection<string> ArrayProperties { get; }
        }
    }
} // end of namespace
