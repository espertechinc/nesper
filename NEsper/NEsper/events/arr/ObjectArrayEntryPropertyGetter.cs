///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.codegen.core;
using com.espertech.esper.codegen.model.expression;
using com.espertech.esper.events.bean;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.arr
{
    /// <summary>
    /// A getter for use with Map-based events simply returns the value for the key.
    /// </summary>
    public class ObjectArrayEntryPropertyGetter : ObjectArrayEventPropertyGetter
    {
        private readonly EventAdapterService _eventAdapterService;
        private readonly BeanEventType _eventType;
        private readonly int _propertyIndex;

        /// <summary>Ctor. </summary>
        /// <param name="propertyIndex">index</param>
        /// <param name="eventType">type of the entry returned</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        public ObjectArrayEntryPropertyGetter(int propertyIndex,
                                              BeanEventType eventType,
                                              EventAdapterService eventAdapterService)
        {
            _propertyIndex = propertyIndex;
            _eventAdapterService = eventAdapterService;
            _eventType = eventType;
        }

        public Object GetObjectArray(Object[] array)
        {
            return array[_propertyIndex];
        }

        public bool IsObjectArrayExistsProperty(Object[] array)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public Object Get(EventBean eventBean)
        {
            Object[] arr = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(eventBean);
            return GetObjectArray(arr);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public Object GetFragment(EventBean eventBean)
        {
            if (_eventType == null)
            {
                return null;
            }
            Object result = Get(eventBean);
            return BaseNestableEventUtil.GetBNFragmentPono(result, _eventType, _eventAdapterService);
        }

        public ICodegenExpression CodegenEventBeanGet(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return BeanUndCastArrayAtIndex(typeof(object[]), beanExpression, _propertyIndex);
        }

        public ICodegenExpression CodegenEventBeanExists(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return ConstantTrue();
        }

        public ICodegenExpression CodegenEventBeanFragment(ICodegenExpression beanExpression, ICodegenContext context)
        {
            if (_eventType == null)
            {
                return ConstantNull();
            }
            return CodegenUnderlyingFragment(
                CastUnderlying(typeof(object[]), beanExpression), context);
        }

        public ICodegenExpression CodegenUnderlyingGet(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return ArrayAtIndex(underlyingExpression, Constant(_propertyIndex));
        }

        public ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return ConstantTrue();
        }

        public ICodegenExpression CodegenUnderlyingFragment(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            if (_eventType == null)
            {
                return ConstantNull();
            }
            var mSvc = context.MakeAddMember(typeof(EventAdapterService), _eventAdapterService);
            var mType = context.MakeAddMember(typeof(BeanEventType), _eventType);
            return StaticMethod(typeof(BaseNestableEventUtil), "GetBNFragmentPono",
                CodegenUnderlyingGet(underlyingExpression, context),
                Ref(mType.MemberName), 
                Ref(mSvc.MemberName));
        }
    }
}