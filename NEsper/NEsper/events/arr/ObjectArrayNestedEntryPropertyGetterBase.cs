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

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.arr
{
    public abstract class ObjectArrayNestedEntryPropertyGetterBase : ObjectArrayEventPropertyGetter
    {
        private readonly int _propertyIndex;
        private readonly EventType _fragmentType;
        private readonly EventAdapterService _eventAdapterService;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="propertyIndex">the property to look at</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        /// <param name="fragmentType">type of the entry returned</param>
        public ObjectArrayNestedEntryPropertyGetterBase(int propertyIndex, EventType fragmentType, EventAdapterService eventAdapterService)
        {
            _propertyIndex = propertyIndex;
            _fragmentType = fragmentType;
            _eventAdapterService = eventAdapterService;
        }

        internal int PropertyIndex => _propertyIndex;
        internal EventType FragmentType => _fragmentType;
        internal EventAdapterService EventAdapterService => _eventAdapterService;

        public abstract Object HandleNestedValue(Object value);

        public abstract bool HandleNestedValueExists(Object value);

        public abstract Object HandleNestedValueFragment(Object value);

        public abstract ICodegenExpression HandleNestedValueCodegen(ICodegenExpression refName, ICodegenContext context);

        public abstract ICodegenExpression HandleNestedValueExistsCodegen(ICodegenExpression refName, ICodegenContext context);

        public abstract ICodegenExpression HandleNestedValueFragmentCodegen(ICodegenExpression refName, ICodegenContext context);

        public Object GetObjectArray(Object[] array)
        {
            var value = array[_propertyIndex];
            if (value == null)
            {
                return null;
            }
            return HandleNestedValue(value);
        }

        public bool IsObjectArrayExistsProperty(Object[] array)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public Object Get(EventBean obj)
        {
            return GetObjectArray(BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(obj));
        }

        private string GetCodegen(ICodegenContext context)
        {
            return context.AddMethod(typeof(Object), typeof(Object[]), "array", this.GetType())
                .DeclareVar(typeof(Object), "value", ArrayAtIndex(Ref("array"), Constant(_propertyIndex)))
                .IfRefNullReturnNull("value")
                .MethodReturn(HandleNestedValueCodegen(Ref("value"), context));
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            Object[] array = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(eventBean);
            Object value = array[_propertyIndex];
            if (value == null)
            {
                return false;
            }
            return HandleNestedValueExists(value);
        }

        private string IsExistsPropertyCodegen(ICodegenContext context)
        {
            return context.AddMethod(typeof(bool), typeof(Object[]), "array", this.GetType())
                .DeclareVar(typeof(Object), "value", ArrayAtIndex(Ref("array"), Constant(_propertyIndex)))
                .IfRefNullReturnFalse("value")
                .MethodReturn(HandleNestedValueExistsCodegen(Ref("value"), context));
        }

        public Object GetFragment(EventBean obj)
        {
            Object[] array = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(obj);
            Object value = array[_propertyIndex];
            if (value == null)
            {
                return null;
            }
            return HandleNestedValueFragment(value);
        }

        private string GetFragmentCodegen(ICodegenContext context)
        {
            return context.AddMethod(typeof(Object), typeof(Object[]), "array", this.GetType())
                .DeclareVar(typeof(Object), "value", ArrayAtIndex(Ref("array"), Constant(_propertyIndex)))
                .IfRefNullReturnFalse("value")
                .MethodReturn(HandleNestedValueFragmentCodegen(Ref("value"), context));
        }

        public ICodegenExpression CodegenEventBeanGet(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingGet(CastUnderlying(typeof(Object[]), beanExpression), context);
        }

        public ICodegenExpression CodegenEventBeanExists(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingExists(CastUnderlying(typeof(Object[]), beanExpression), context);
        }

        public ICodegenExpression CodegenEventBeanFragment(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingFragment(CastUnderlying(typeof(Object[]), beanExpression), context);
        }

        public ICodegenExpression CodegenUnderlyingGet(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return LocalMethod(GetCodegen(context), underlyingExpression);
        }

        public ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return LocalMethod(IsExistsPropertyCodegen(context), underlyingExpression);
        }

        public ICodegenExpression CodegenUnderlyingFragment(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return LocalMethod(GetFragmentCodegen(context), underlyingExpression);
        }
    }
} // end of namespace