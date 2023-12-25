///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.collection;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.dot
{
    public class ExprDotStaticMethodWrapIterableEvents : ExprDotStaticMethodWrap
    {
        private readonly EventBeanTypedEventFactory _eventBeanTypedEventFactory;
        private readonly BeanEventType _type;

        public ExprDotStaticMethodWrapIterableEvents(
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventType type)
        {
            _eventBeanTypedEventFactory = eventBeanTypedEventFactory;
            _type = type;
        }

        public EPChainableType TypeInfo => EPChainableTypeHelper.CollectionOfEvents(_type);

        public object ConvertNonNull(object result)
        {
            if (result is ICollection<EventBean> eventBeanCollection)
                return eventBeanCollection;
            
            // there is a need to read the iterator to the cache since if it's iterated twice, the iterator is already exhausted
            return result
                .UnwrapEnumerable<object>()
                .Select(v => _eventBeanTypedEventFactory.AdapterForTypedObject(v, _type))
                .ToList();

            //return new WrappingCollection(eventBeanTypedEventFactory, type, ((IEnumerable) result).GetEnumerator());
        }

        public CodegenExpression CodegenConvertNonNull(
            CodegenExpression result,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope classScope)
        {
            var eventSvcMember =
                classScope.AddOrGetDefaultFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
            var typeMember = classScope.AddDefaultFieldUnshared(
                true,
                typeof(BeanEventType),
                Cast(
                    typeof(BeanEventType),
                    EventTypeUtility.ResolveTypeCodegen(_type, EPStatementInitServicesConstants.REF)));
            return StaticMethod(
                typeof(ExprDotStaticMethodWrapIterableEvents),
                "UnwrapEventBeans",
                eventSvcMember,
                typeMember,
                result);
        }

        public static ICollection<EventBean> UnwrapEventBeans(
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventType type,
            IEnumerable enumerable)
        {
            return enumerable
                .Cast<object>()
                .Select(value => eventBeanTypedEventFactory.AdapterForTypedObject(value, type))
                .ToList();
        }
    }
} // end of namespace