///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.collection;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.rettype
{
    /// <summary>
    ///     Carries return type information related to the return values returned by expressions.
    ///     <para>
    ///         Use factory methods to initialize return type information according to the return values
    ///         that your expression is going to provide.
    ///     </para>
    ///     <ol>
    ///         <li>
    ///             Use <see cref="EPChainableTypeHelper.CollectionOfEvents(com.espertech.esper.common.client.EventType)" />
    ///             to indicate that the expression returns a collection of events.
    ///         </li>
    ///         <li>
    ///             Use <see cref="EPChainableTypeHelper.SingleEvent(com.espertech.esper.common.client.EventType)" />
    ///             to indicate that the expression returns a single event.
    ///         </li>
    ///         <li>
    ///             Use <see cref="EPChainableTypeHelper.CollectionOfSingleValue(Type)" />
    ///             to indicate that the expression returns a collection of single values.
    ///             A single value can be any object including null.
    ///         </li>
    ///         <li>
    ///             Use <see cref="EPChainableTypeHelper.Array(Type)" />
    ///             to indicate that the expression returns an array of single values.
    ///             A single value can be any object including null.
    ///         </li>
    ///         <li>
    ///             Use <see cref="SingleValueNonNull" />
    ///             to indicate that the expression returns a single value.
    ///             A single value can be any object including null.
    ///             Such expression results cannot be used as input to enumeration methods, for example.
    ///         </li>
    ///     </ol>
    /// </summary>
    public static class EPChainableTypeHelper
    {
        public static EventType GetEventTypeMultiValued(EPChainableType type)
        {
            return type is EPChainableTypeEventMulti typeEventMulti ? typeEventMulti.Component : null;
        }

        public static Type GetCollectionOrArrayComponentTypeOrNull(EPChainableType type) {
            if (type is EPChainableTypeClass classInfo) {
                var clazz = classInfo.Clazz;
                if (clazz.IsArray) {
                    return clazz.GetElementType();
                }

                if (clazz.GetGenericTypeDefinition() == typeof(FlexCollection<>)) {
                    return clazz.GetGenericArguments()[0];
                }
                    
                if (clazz.IsGenericCollection()) {
                    return GenericExtensions.GetCollectionItemType(clazz);
                }
            }
            return null;
        }

        public static bool IsCarryEvent(this EPChainableType chainableType)
        {
            return chainableType is EPChainableTypeEventMulti || chainableType is EPChainableTypeEventSingle;
        }

        public static EventType GetEventType(this EPChainableType chainableType)
        {
            if (chainableType is EPChainableTypeEventMulti typeEventMulti) {
                return typeEventMulti.Component;
            }

            return (chainableType as EPChainableTypeEventSingle)?.EventType;
        }

        /// <summary>
        ///     Indicate that the expression return type is an array of a given component type.
        /// </summary>
        /// <param name="arrayComponentType">array component type</param>
        /// <returns>array of single value expression result type</returns>
        public static EPChainableType Array(Type arrayComponentType)
        {
            if (arrayComponentType == null) {
                throw new ArgumentException("Invalid null array component type");
            }

            var arrayType = TypeHelper.GetArrayType(arrayComponentType);
            return new EPChainableTypeClass(arrayType);
        }

        public static EPChainableTypeClass SingleValueNonNull(Type typeClass)
        {
            if (typeClass.IsNullTypeSafe()) {
                throw new ArgumentException("Null-type not supported as a return type");
            }

            return new EPChainableTypeClass(typeClass);
        }

        public static EPChainableType SingleValue(Type type)
        {
            return type.IsNullType()
                ? (EPChainableType) EPChainableTypeNull.INSTANCE
                : new EPChainableTypeClass(type);
        }
        
        public static EPChainableType NullValue()
        {
            return EPChainableTypeNull.INSTANCE;
        }

        /// <summary>
        ///     Indicate that the expression return type is a collection of a given component type.
        /// </summary>
        /// <param name="collectionComponentType">collection component type</param>
        /// <returns>collection of single value expression result type</returns>
        public static EPChainableType CollectionOfSingleValue(Type collectionComponentType)
        {
            if (collectionComponentType == null) {
                throw new ArgumentException("Invalid null collection component type");
            }

            var typeClass = typeof(FlexCollection<>)
                .MakeGenericType(collectionComponentType.GetBoxedType());
            return new EPChainableTypeClass(typeClass);

            // var containerType = typeof(FlexCollection);
            // return new ClassMultiValuedEPType(
            //     containerType,
            //     collectionComponentType);
        }

        /// <summary>
        ///     Indicate that the expression return type is a collection of a given type of events.
        /// </summary>
        /// <param name="eventTypeOfCollectionEvents">the event type of the events that are part of the collection</param>
        /// <returns>collection of events expression result type</returns>
        public static EPChainableType CollectionOfEvents(EventType eventTypeOfCollectionEvents)
        {
            if (eventTypeOfCollectionEvents == null) {
                throw new ArgumentException("Invalid null event type");
            }

            return new EPChainableTypeEventMulti(typeof(FlexCollection), eventTypeOfCollectionEvents);
            //return new EventMultiValuedEPType(typeof(ICollection<EventBean>), eventTypeOfCollectionEvents);
        }

        /// <summary>
        ///Indicate that the expression return type is an array of events of given type.
        /// </summary>
        /// <param name="eventTypeOfArrayEvents">the event type of the events that are part of the array</param>
        /// <returns>array of events expression result type</returns> 
        public static EPChainableType ArrayOfEvents(EventType eventTypeOfArrayEvents)
        {
            if (eventTypeOfArrayEvents == null) {
                throw new ArgumentException("Invalid null event type");
            }

            return new EPChainableTypeEventMulti(typeof(EventBean[]), eventTypeOfArrayEvents);
        }

        /// <summary>
        ///     Indicate that the expression return type is single event of a given event type.
        /// </summary>
        /// <param name="eventTypeOfSingleEvent">the event type of the event returned</param>
        /// <returns>single-event expression result type</returns>
        public static EPChainableType SingleEvent(EventType eventTypeOfSingleEvent)
        {
            if (eventTypeOfSingleEvent == null) {
                throw new ArgumentException("Invalid null event type");
            }

            return new EPChainableTypeEventSingle(eventTypeOfSingleEvent);
        }

        /// <summary>
        ///     Interrogate the provided method and determine whether it returns
        ///     single-value, array of single-value or collection of single-value and
        ///     their component type.
        /// </summary>
        /// <param name="method">the class methods</param>
        /// <returns>expression return type</returns>
        public static EPChainableType FromMethod(MethodInfo method)
        {
            var returnType = method.ReturnType;
            if (returnType.IsArray) {
                var componentType = method.ReturnType.GetElementType();
                return Array(componentType);
            }

            if (returnType.IsGenericCollection()) {
                var componentType = TypeHelper.GetGenericReturnType(method, true);
                return CollectionOfSingleValue(componentType);
            }

            return SingleValueNonNull(method.ReturnType.GetBoxedType());
        }

        /// <summary>
        ///     Returns a nice text detailing the expression result type.
        /// </summary>
        /// <param name="chainableType">type</param>
        /// <returns>descriptive text</returns>
        public static string ToTypeDescriptive(this EPChainableType chainableType)
        {
            switch (chainableType) {
                case EPChainableTypeEventSingle typeEventSingle:
                    return "event type '" + typeEventSingle.EventType.Name + "'";
                case EPChainableTypeEventMulti typeEventMulti when typeEventMulti.Container == typeof(EventType[]):
                    return "array of events of type '" + typeEventMulti.Component.Name + "'";
                case EPChainableTypeEventMulti typeEventMulti:
                    return "collection of events of type '" + typeEventMulti.Component.Name + "'";
                case EPChainableTypeClass typeClass:
                    return typeClass.Clazz.TypeSafeName();
                case EPChainableTypeNull _:
                    return "null type";
                default:
                    throw new ArgumentException("Unrecognized type " + chainableType);
            }
        }

        public static Type GetNormalizedClass(this EPChainableType theType)
        {
            switch (theType) {
                case EPChainableTypeEventMulti typeEventMulti:
                    return TypeHelper.GetArrayType(typeEventMulti.Component.UnderlyingType);
                case EPChainableTypeEventSingle typeEventSingle:
                    return typeEventSingle.EventType.UnderlyingType;
                case EPChainableTypeClass chainableTypeClass:
                    return chainableTypeClass.Clazz;
                case EPChainableTypeNull _:
                    return null;
                default:
                    throw new ArgumentException("Unrecognized type " + theType);
            }
        }

        public static Type GetCodegenReturnType(this EPChainableType theType)
        {
            switch (theType) {
                case EPChainableTypeEventMulti typeEventMulti:
                    return typeEventMulti.Container;
                case EPChainableTypeEventSingle _:
                    return typeof(EventBean);
                case EPChainableTypeClass typeClass:
                    return typeClass.Clazz.GetBoxedType();
                case EPChainableTypeNull _:
                    return null;
                default:
                    throw new ArgumentException("Unrecognized type " + theType);
            }
        }

        public static EPChainableType OptionalFromEnumerationExpr(
            StatementRawInfo raw,
            StatementCompileTimeServices services,
            ExprNode exprNode)
        {
            if (!(exprNode is ExprEnumerationForge)) {
                return null;
            }

            var enumInfo = (ExprEnumerationForge) exprNode;
            if (enumInfo.ComponentTypeCollection != null) {
                return CollectionOfSingleValue(
                    enumInfo.ComponentTypeCollection);
            }

            var eventTypeSingle = enumInfo.GetEventTypeSingle(raw, services);
            if (eventTypeSingle != null) {
                return SingleEvent(eventTypeSingle);
            }

            var eventTypeColl = enumInfo.GetEventTypeCollection(raw, services);
            if (eventTypeColl != null) {
                return CollectionOfEvents(eventTypeColl);
            }

            return null;
        }

        public static EventType OptionalIsEventTypeColl(this EPChainableType type)
        {
            if (type is EPChainableTypeEventMulti typeEventMulti) {
                return typeEventMulti.Component;
            }

            return null;
        }

        public static EventType OptionalIsEventTypeSingle(this EPChainableType type)
        {
            if (type is EPChainableTypeEventSingle typeEventSingle) {
                return typeEventSingle.EventType;
            }

            return null;
        }
        
        
        public static void TraverseAnnotations<T>(
            IList<Type> classes, 
            BiConsumer<Type, T> consumer)
            where T : Attribute
        {
            var annotationClass = typeof(T);
            WalkAnnotations(classes, (annotation, clazz) => {
                if (annotation.GetType() == annotationClass) {
                    consumer.Invoke(clazz, (T) annotation);
                }
            });
        }

        private static void WalkAnnotations(
            IEnumerable<Type> classes,
            AnnotationConsumer consumer)
        {
            if (classes == null) {
                return;
            }
            foreach (var clazz in classes) {
                foreach (var annotation in clazz.UnwrapAttributes(true)) {
                    consumer.Invoke(annotation, clazz);
                }
            }
        }

        public delegate void AnnotationConsumer(Attribute annotation, Type type);
    }
} // end of namespace