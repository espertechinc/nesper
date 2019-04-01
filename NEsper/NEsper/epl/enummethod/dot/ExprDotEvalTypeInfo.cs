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
using com.espertech.esper.util;

namespace com.espertech.esper.epl.enummethod.dot
{
    public class ExprDotEvalTypeInfo
    {
        public Type Scalar { get; private set; }

        public Type Component { get; private set; }

        public EventType EventType { get; private set; }

        public EventType EventTypeColl { get; private set; }

        private ExprDotEvalTypeInfo(Type scalar, Type component, EventType eventType, EventType eventTypeColl)
        {
            Scalar = scalar;
            Component = component;
            EventType = eventType;
            EventTypeColl = eventTypeColl;
        }

        public static ExprDotEvalTypeInfo From(Type inputType, Type collectionComponentType, EventType lambdaType)
        {
            var info = new ExprDotEvalTypeInfo(null, null, null, null);
            if (lambdaType != null)
            {
                info.EventTypeColl = lambdaType;
            }
            else if (collectionComponentType != null)
            {
                info.Scalar = typeof(ICollection<object>);
                info.Component = collectionComponentType;
            }
            else
            {
                info.Scalar = inputType;
            }
            return info;
        }

        public bool IsScalar
        {
            get { return Scalar != null; }
        }

        public static ExprDotEvalTypeInfo FromMethod(MethodInfo method)
        {
            var returnType = method.ReturnType;
            if (returnType.IsImplementsInterface(typeof(ICollection<object>)))
            {
                var componentType = TypeHelper.GetGenericReturnType(method, true);
                return ComponentColl(componentType);
            }
            return ScalarOrUnderlying(method.ReturnType);
        }

        public static ExprDotEvalTypeInfo ScalarOrUnderlying(Type scalar)
        {
            return new ExprDotEvalTypeInfo(scalar, null, null, null);
        }

        public static ExprDotEvalTypeInfo ComponentColl(Type component)
        {
            return new ExprDotEvalTypeInfo(typeof(ICollection<object>), component, null, null);
        }

        public static ExprDotEvalTypeInfo EventColl(EventType eventColl)
        {
            return new ExprDotEvalTypeInfo(null, null, null, eventColl);
        }

        public static ExprDotEvalTypeInfo Event(EventType theEvent)
        {
            return new ExprDotEvalTypeInfo(null, null, theEvent, null);
        }

        public String ToTypeName()
        {
            if (Component != null)
            {
                return "collection of " + Component.Name;
            }
            else if (EventType != null)
            {
                return "event type '" + EventType.Name + "'";
            }
            else if (EventTypeColl != null)
            {
                return "collection of events of type '" + EventTypeColl.Name + "'";
            }
            else if (Scalar != null)
            {
                return "class " + TypeHelper.GetCleanName(Scalar);
            }
            else
            {
                return "an incompatible type";
            }
        }
    }
}
