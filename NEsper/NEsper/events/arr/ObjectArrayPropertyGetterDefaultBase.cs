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
    /// <summary>Getter for map entry.</summary>
    public abstract class ObjectArrayPropertyGetterDefaultBase : ObjectArrayEventPropertyGetter
    {
        private readonly int _propertyIndex;
        protected readonly EventType _fragmentEventType;
        protected readonly EventAdapterService _eventAdapterService;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="propertyIndex">property index</param>
        /// <param name="fragmentEventType">fragment type</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        public ObjectArrayPropertyGetterDefaultBase(int propertyIndex, EventType fragmentEventType, EventAdapterService eventAdapterService)
        {
            this._propertyIndex = propertyIndex;
            this._fragmentEventType = fragmentEventType;
            this._eventAdapterService = eventAdapterService;
        }

        protected abstract Object HandleCreateFragment(Object value);

        protected abstract ICodegenExpression HandleCreateFragmentCodegen(ICodegenExpression value, ICodegenContext context);

        public Object GetObjectArray(Object[] array)
        {
            return array[_propertyIndex];
        }

        public bool IsObjectArrayExistsProperty(Object[] array)
        {
            return array.Length > _propertyIndex;
        }

        public Object Get(EventBean obj)
        {
            Object[] array = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(obj);
            return GetObjectArray(array);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true;
        }

        public Object GetFragment(EventBean eventBean)
        {
            Object value = Get(eventBean);
            return HandleCreateFragment(value);
        }

        private string GetFragmentCodegen(ICodegenExpression value, ICodegenContext context)
        {
            return context.AddMethod(typeof(Object), typeof(Object[]), "oa", this.GetType())
                    .DeclareVar(typeof(Object), "value", CodegenUnderlyingGet(Ref("oa"), context))
                    .MethodReturn(HandleCreateFragmentCodegen(Ref("value"), context));
        }

        public ICodegenExpression CodegenEventBeanGet(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingGet(CastUnderlying(typeof(Object[]), beanExpression), context);
        }

        public ICodegenExpression CodegenEventBeanExists(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return ConstantTrue();
        }

        public ICodegenExpression CodegenEventBeanFragment(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingFragment(CastUnderlying(typeof(Object[]), beanExpression), context);
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
            if (_fragmentEventType == null)
            {
                return ConstantNull();
            }
            return LocalMethod(GetFragmentCodegen(underlyingExpression, context), underlyingExpression);
        }
    }
} // end of namespace