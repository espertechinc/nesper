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

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.dot
{
    public class ExprDotStaticMethodWrapArrayEvents : ExprDotStaticMethodWrap
    {
        private readonly EventBeanTypedEventFactory _eventBeanTypedEventFactory;
        private readonly BeanEventType _type;

        public ExprDotStaticMethodWrapArrayEvents(
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventType type)
        {
            _eventBeanTypedEventFactory = eventBeanTypedEventFactory;
            _type = type;
        }

        public EPChainableType TypeInfo => EPChainableTypeHelper.CollectionOfEvents(_type);

        public object ConvertNonNull(object result)
        {
            if (!result.GetType().IsArray) {
                return null;
            }

            return new WrappingCollection(_eventBeanTypedEventFactory, _type, result);
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
            return NewInstance(
                typeof(WrappingCollection),
                eventSvcMember,
                typeMember,
                result);
        }

        public class WrappingCollection : ICollection<EventBean>
        {
            private readonly EventBeanTypedEventFactory _eventBeanTypedEventFactory;
            private readonly BeanEventType _type;
            private readonly Array _array;

            public WrappingCollection(
                EventBeanTypedEventFactory eventBeanTypedEventFactory,
                BeanEventType type,
                object array)
            {
                _eventBeanTypedEventFactory = eventBeanTypedEventFactory;
                _type = type;
                _array = (Array)array;
            }

            public int Count => _array.Length;

            public bool IsEmpty => _array.Length == 0;

            public bool IsReadOnly => true;

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public IEnumerator<EventBean> GetEnumerator()
            {
                for (var ii = 0; ii < _array.Length; ii++) {
                    yield return _eventBeanTypedEventFactory.AdapterForTypedObject(
                        _array.GetValue(ii),
                        _type);
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