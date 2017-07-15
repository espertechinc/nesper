///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;
using com.espertech.esper.events.bean;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.enummethod.dot
{
    public class ExprDotStaticMethodWrapFactory
    {
        public static ExprDotStaticMethodWrap Make(
            MethodInfo method,
            EventAdapterService eventAdapterService,
            IList<ExprChainedSpec> modifiedChain,
            string optionalEventTypeName)
        {
            if (modifiedChain.IsEmpty() || (!modifiedChain[0].Name.IsEnumerationMethod()))
            {
                return null;
            }


            if (method.ReturnType.IsArray)
            {
                var componentType = method.ReturnType.GetElementType();
                if (componentType == typeof(EventBean))
                {
                    EventType eventType = RequireEventType(method, eventAdapterService, optionalEventTypeName);
                    return new ExprDotStaticMethodWrapEventBeanArr(eventType);
                }
                if (componentType == null || componentType.IsBuiltinDataType())
                {
                    return new ExprDotStaticMethodWrapArrayScalar(method.Name, componentType);
                }
                var type = (BeanEventType)eventAdapterService.AddBeanType(componentType.Name, componentType, false, false, false);
                return new ExprDotStaticMethodWrapArrayEvents(eventAdapterService, type);
            }

#if DEFUNCT
            if (method.ReturnType.IsGenericCollection())
            {
                var genericType = TypeHelper.GetGenericReturnType(method, true);
                if (genericType == null || genericType.IsBuiltinDataType())
                {
                    return new ExprDotStaticMethodWrapCollection(method.Name, genericType);
                }
            }
#endif

            if (method.ReturnType.IsGenericEnumerable())
            {
                var genericType = TypeHelper.GetGenericReturnType(method, true);
                if (genericType == typeof(EventBean))
                {
                    EventType eventType = RequireEventType(method, eventAdapterService, optionalEventTypeName);
                    return new ExprDotStaticMethodWrapEventBeanColl(eventType);
                }
                if (genericType == null || genericType.IsBuiltinDataType())
                {
                    return new ExprDotStaticMethodWrapIterableScalar(method.Name, genericType);
                }
                var type = (BeanEventType)eventAdapterService.AddBeanType(genericType.Name, genericType, false, false, false);
                return new ExprDotStaticMethodWrapIterableEvents(eventAdapterService, type);
            }
            return null;
        }

        private static EventType RequireEventType(MethodInfo method, EventAdapterService eventAdapterService, string optionalEventTypeName) 
        {
            return EventTypeUtility.RequireEventType("Method", method.Name, eventAdapterService, optionalEventTypeName);
        }
    }
}
