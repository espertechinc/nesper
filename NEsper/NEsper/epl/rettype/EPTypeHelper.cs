///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.rettype
{
    /// <summary>
    /// Carries return type information related to the return values returned by expressions.
    /// <para/>
    /// Use factory methods to initialize return type information according to the return
    /// values that your expression is going to provide.
    /// <para/>
    /// Use <seealso cref="EPTypeHelper.CollectionOfEvents(com.espertech.esper.client.EventType)" />
    /// to indicate that the expression returns a collection of events.
    /// Use <seealso cref="EPTypeHelper.SingleEvent(com.espertech.esper.client.EventType)" />
    /// to indicate that the expression returns a single event.
    /// Use <seealso cref="EPTypeHelper.CollectionOfSingleValue(Type)" /> to indicate that 
    /// the expression returns a collection of single values. A single value can be any object including null.
    /// Use <seealso cref="EPTypeHelper.Array(Type)" /> to indicate that the expression returns an array 
    /// of single values. A single value can be any object including null. 
    /// Use <seealso cref="EPTypeHelper.SingleValue(Type)" /> to indicate that the expression 
    /// returns a single value. A single value can be any object including null. Such 
    /// expression results cannot be used as input to enumeration methods, for example.
    /// </summary>
    public static class EPTypeHelper
    {
        public static EventType GetEventTypeSingleValued(this EPType type)
        {
            if (type is EventEPType)
            {
                return ((EventEPType)type).EventType;
            }
            return null;
        }

        public static EventType GetEventTypeMultiValued(this EPType type)
        {
            if (type is EventMultiValuedEPType)
            {
                return ((EventMultiValuedEPType)type).Component;
            }
            return null;
        }

        public static Type GetClassMultiValued(this EPType type)
        {
            if (type is ClassMultiValuedEPType)
            {
                return ((ClassMultiValuedEPType)type).Component;
            }
            return null;
        }

        public static Type GetClassSingleValued(this EPType type)
        {
            if (type is ClassEPType)
            {
                return ((ClassEPType)type).Clazz;
            }
            return null;
        }

        public static bool IsCarryEvent(this EPType epType)
        {
            return epType is EventMultiValuedEPType || epType is EventEPType;
        }

        public static EventType GetEventType(this EPType epType)
        {
            if (epType is EventMultiValuedEPType)
            {
                return ((EventMultiValuedEPType)epType).Component;
            }
            if (epType is EventEPType)
            {
                return ((EventEPType)epType).EventType;
            }
            return null;
        }

        /// <summary>Indicate that the expression return type is an array of a given component type. </summary>
        /// <param name="arrayComponentType">array component type</param>
        /// <returns>array of single value expression result type</returns>
        public static EPType Array(Type arrayComponentType)
        {
            if (arrayComponentType == null)
            {
                throw new ArgumentException("Invalid null array component type");
            }
            return new ClassMultiValuedEPType(TypeHelper.GetArrayType(arrayComponentType), arrayComponentType);
        }

        /// <summary>
        /// Indicate that the expression return type is a single (non-enumerable) value of the given type. 
        /// The expression can still return an array or collection or events however since the engine would 
        /// not know the type of such objects and may not use runtime reflection it may not allow certain 
        /// operations on expression results.
        /// </summary>
        /// <param name="singleValueType">type of single value returned, or null to indicate that the expression always returns null</param>
        /// <returns>single-value expression result type</returns>
        public static EPType SingleValue(Type singleValueType)
        {
            // null value allowed
            if (singleValueType != null && singleValueType.IsArray)
            {
                return new ClassMultiValuedEPType(singleValueType, singleValueType.GetElementType());
            }
            return new ClassEPType(singleValueType);
        }

        public static EPType NullValue
        {
            get { return NullEPType.INSTANCE; }
        }

        /// <summary>
        /// Indicate that the expression return type is a collection of a given component type.
        /// </summary>
        /// <param name="collectionComponentType">collection component type</param>
        /// <returns>
        /// collection of single value expression result type
        /// </returns>
        public static EPType CollectionOfSingleValue(Type collectionComponentType)
        {
            if (collectionComponentType == null)
            {
                throw new ArgumentException("Invalid null collection component type");
            }

            var collectionType = typeof(ICollection<>).MakeGenericType(collectionComponentType);
            return new ClassMultiValuedEPType(collectionType, collectionComponentType);
            //return new ClassMultiValuedEPType(typeof(ICollection<object>), collectionComponentType);
        }

        /// <summary>
        /// Indicate that the expression return type is a collection of a given type of events.
        /// </summary>
        /// <param name="eventTypeOfCollectionEvents">the event type of the events that are part of the collection</param>
        /// <returns>
        /// collection of events expression result type
        /// </returns>
        public static EPType CollectionOfEvents(EventType eventTypeOfCollectionEvents)
        {
            if (eventTypeOfCollectionEvents == null)
            {
                throw new ArgumentException("Invalid null event type");
            }

            return new EventMultiValuedEPType(typeof(ICollection<object>), eventTypeOfCollectionEvents);
        }

        /// <summary>Indicate that the expression return type is single event of a given event type. </summary>
        /// <param name="eventTypeOfSingleEvent">the event type of the event returned</param>
        /// <returns>single-event expression result type</returns>
        public static EPType SingleEvent(EventType eventTypeOfSingleEvent)
        {
            if (eventTypeOfSingleEvent == null)
            {
                throw new ArgumentException("Invalid null event type");
            }
            return new EventEPType(eventTypeOfSingleEvent);
        }

        /// <summary>Interrogate the provided method and determine whether it returns single-value, array of single-value or collection of single-value and their component type. </summary>
        /// <param name="method">the class methods</param>
        /// <returns>expression return type</returns>
        public static EPType FromMethod(MethodInfo method)
        {
            var returnType = method.ReturnType;
            if (returnType.IsGenericCollection())
            {
                var componentType = TypeHelper.GetGenericReturnType(method, true);
                return CollectionOfSingleValue(componentType);
            }
            if (method.ReturnType.IsArray)
            {
                var componentType = method.ReturnType.GetElementType();
                return Array(componentType);
            }
            return SingleValue(method.ReturnType);
        }

        /// <summary>Returns a nice text detailing the expression result type. </summary>
        /// <returns>descriptive text</returns>
        public static String ToTypeDescriptive(this EPType epType)
        {
            if (epType is EventEPType)
            {
                var type = (EventEPType)epType;
                return "event type '" + type.EventType.Name + "'";
            }
            else if (epType is EventMultiValuedEPType)
            {
                var type = (EventMultiValuedEPType)epType;
                if (type.Container == typeof(EventType[]))
                {
                    return "array of events of type '" + type.Component.Name + "'";
                }
                else
                {
                    return "collection of events of type '" + type.Component.Name + "'";
                }
            }
            else if (epType is ClassMultiValuedEPType)
            {
                var type = (ClassMultiValuedEPType)epType;
                if (type.Container.IsArray)
                {
                    return "array of " + type.Component.Name;
                }
                else
                {
                    return "collection of " + type.Component.Name;
                }
            }
            else if (epType is ClassEPType)
            {
                var type = (ClassEPType)epType;
                return "class " + type.Clazz.GetCleanName();
            }
            else if (epType is NullEPType)
            {
                return "null type";
            }
            else
            {
                throw new ArgumentException("Unrecognized type " + epType);
            }
        }

        public static Type GetNormalizedClass(this EPType theType)
        {
            if (theType is EventMultiValuedEPType)
            {
                var type = (EventMultiValuedEPType)theType;
                return TypeHelper.GetArrayType(type.Component.UnderlyingType);
            }
            else if (theType is EventEPType)
            {
                var type = (EventEPType)theType;
                return type.EventType.UnderlyingType;
            }
            else if (theType is ClassMultiValuedEPType)
            {
                var type = (ClassMultiValuedEPType)theType;
                return type.Container;
            }
            else if (theType is ClassEPType)
            {
                var type = (ClassEPType)theType;
                return type.Clazz;
            }
            else if (theType is NullEPType)
            {
                return null;
            }
            throw new ArgumentException("Unrecognized type " + theType);
        }

        public static EPType OptionalFromEnumerationExpr(int statementId, EventAdapterService eventAdapterService, ExprNode exprNode)
        {
            if (!(exprNode is ExprEvaluatorEnumeration))
            {
                return null;
            }
            var enumInfo = (ExprEvaluatorEnumeration)exprNode;
            if (enumInfo.ComponentTypeCollection != null)
            {
                return EPTypeHelper.CollectionOfSingleValue(enumInfo.ComponentTypeCollection);
            }
            var eventTypeSingle = enumInfo.GetEventTypeSingle(eventAdapterService, statementId);
            if (eventTypeSingle != null)
            {
                return EPTypeHelper.SingleEvent(eventTypeSingle);
            }
            var eventTypeColl = enumInfo.GetEventTypeCollection(eventAdapterService, statementId);
            if (eventTypeColl != null)
            {
                return EPTypeHelper.CollectionOfEvents(eventTypeColl);
            }
            return null;
        }

        public static EventType OptionalIsEventTypeColl(EPType type)
        {
            if (type != null && type is EventMultiValuedEPType)
            {
                return ((EventMultiValuedEPType)type).Component;
            }
            return null;
        }

        public static Type OptionalIsComponentTypeColl(EPType type)
        {
            if (type != null && type is ClassMultiValuedEPType)
            {
                return ((ClassMultiValuedEPType)type).Component;
            }
            return null;
        }

        public static EventType OptionalIsEventTypeSingle(EPType type)
        {
            if (type != null && type is EventEPType)
            {
                return ((EventEPType)type).EventType;
            }
            return null;
        }
    }
}
