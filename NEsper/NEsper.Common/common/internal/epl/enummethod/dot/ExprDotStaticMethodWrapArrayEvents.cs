///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.dot
{
    public class ExprDotStaticMethodWrapArrayEvents : ExprDotStaticMethodWrap
    {
        private EventBeanTypedEventFactory eventBeanTypedEventFactory;
        private BeanEventType type;

        public ExprDotStaticMethodWrapArrayEvents(
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventType type)
        {
            this.eventBeanTypedEventFactory = eventBeanTypedEventFactory;
            this.type = type;
        }

        public EPType TypeInfo => EPTypeHelper.CollectionOfEvents(type);

        public ICollection<EventBean> ConvertNonNull(object result)
        {
            if (!result.GetType().IsArray) {
                return null;
            }

            return new WrappingCollection(eventBeanTypedEventFactory, type, result);
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
                    EventTypeUtility.ResolveTypeCodegen(type, EPStatementInitServicesConstants.REF)));
            return NewInstance(
                typeof(ExprDotStaticMethodWrapArrayEvents.WrappingCollection),
                eventSvcMember,
                typeMember,
                result);
        }

        public class WrappingCollection : ICollection<EventBean>
        {
            private EventBeanTypedEventFactory eventBeanTypedEventFactory;
            private BeanEventType type;
            private Array array;

            public WrappingCollection(
                EventBeanTypedEventFactory eventBeanTypedEventFactory,
                BeanEventType type,
                object array)
            {
                this.eventBeanTypedEventFactory = eventBeanTypedEventFactory;
                this.type = type;
                this.array = (Array) array;
            }

            public int Count => array.Length;

            public bool IsEmpty => array.Length == 0;

            public bool IsReadOnly => true;

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public IEnumerator<EventBean> GetEnumerator()
            {
                for (int ii = 0; ii < array.Length; ii++) {
                    yield return eventBeanTypedEventFactory.AdapterForTypedBean(
                        array.GetValue(ii),
                        type);
                }
            }

            public void Add(EventBean item)
            {
                throw new UnsupportedOperationException("Partial implementation");
            }

            public void Clear()
            {
                throw new UnsupportedOperationException("Partial implementation");
            }

            public bool Contains(EventBean item)
            {
                throw new UnsupportedOperationException("Partial implementation");
            }

            public void CopyTo(
                EventBean[] array,
                int arrayIndex)
            {
                throw new UnsupportedOperationException("Partial implementation");
            }

            public bool Remove(EventBean item)
            {
                throw new UnsupportedOperationException("Partial implementation");
            }
        }
    }
} // end of namespace