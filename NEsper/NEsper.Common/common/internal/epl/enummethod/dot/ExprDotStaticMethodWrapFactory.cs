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
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.enummethod.dot
{
    public class ExprDotStaticMethodWrapFactory
    {
        public static ExprDotStaticMethodWrap Make(
            MethodInfo method,
            IList<ExprChainedSpec> modifiedChain,
            string optionalEventTypeName,
            ExprValidationContext validationContext)
        {
            if (modifiedChain.IsEmpty() || !EnumMethodEnum.IsEnumerationMethod(modifiedChain[0].Name)) {
                return null;
            }

            if (method.ReturnType.IsArray) {
                var componentType = method.ReturnType.GetElementType();
                if (componentType == typeof(EventBean)) {
                    var eventType = RequireEventType(method, optionalEventTypeName, validationContext);
                    return new ExprDotStaticMethodWrapEventBeanArr(eventType);
                }

                if (componentType == null || componentType.IsBuiltinDataType()) {
                    return new ExprDotStaticMethodWrapArrayScalar(method.Name, method.ReturnType);
                }

                var type = MakeBeanType(method.Name, componentType, validationContext);
                return new ExprDotStaticMethodWrapArrayEvents(null, type);
            }

#if DEFUNCT
            if (method.ReturnType.IsGenericCollection())
            {
                var genericType = TypeHelper.GetGenericReturnType(method, true);
                if (genericType == typeof(EventBean)) {
                    var eventType = RequireEventType(method, optionalEventTypeName, validationContext);
                    return new ExprDotStaticMethodWrapEventBeanColl(eventType);
                }

                if (genericType == null || TypeHelper.IsJavaBuiltinDataType(genericType)) {
                    return new ExprDotStaticMethodWrapCollection(method.Name, genericType);
                }
            }
#endif

            if (method.ReturnType.IsGenericEnumerable()) {
                var genericType = TypeHelper.GetGenericReturnType(method, true);
                if (genericType == null || TypeHelper.IsBuiltinDataType(genericType)) {
                    return new ExprDotStaticMethodWrapIterableScalar(method.Name, genericType);
                }

                var type = MakeBeanType(method.Name, genericType, validationContext);
                return new ExprDotStaticMethodWrapIterableEvents(validationContext.EventBeanTypedEventFactory, type);
            }

            return null;
        }

        private static BeanEventType MakeBeanType(
            string methodName,
            Type clazz,
            ExprValidationContext validationContext)
        {
            var eventTypeName =
                validationContext.StatementCompileTimeService.EventTypeNameGeneratorStatement
                    .GetAnonymousTypeNameUDFMethod(methodName, clazz.Name);
            var metadata = new EventTypeMetadata(
                eventTypeName, validationContext.ModuleName, EventTypeTypeClass.UDFDERIVED,
                EventTypeApplicationType.CLASS, NameAccessModifier.TRANSIENT, EventTypeBusModifier.NONBUS, false,
                EventTypeIdPair.Unassigned());
            var stem = validationContext.StatementCompileTimeService.BeanEventTypeStemService
                .GetCreateStem(clazz, null);
            var beantype = new BeanEventType(
                stem, metadata, validationContext.StatementCompileTimeService.BeanEventTypeFactoryPrivate, null, null,
                null, null);
            validationContext.StatementCompileTimeService.EventTypeCompileTimeRegistry.NewType(beantype);
            return beantype;
        }

        private static EventType RequireEventType(
            MethodInfo method,
            string optionalEventTypeName,
            ExprValidationContext ctx)
        {
            return EventTypeUtility.RequireEventType(
                "Method", method.Name, optionalEventTypeName,
                ctx.StatementCompileTimeService.EventTypeCompileTimeResolver);
        }
    }
} // end of namespace