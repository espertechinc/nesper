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
    /// <summary>Getter for map array.</summary>
    public class ObjectArrayFragmentArrayPropertyGetter : ObjectArrayEventPropertyGetter
    {
        private readonly int _propertyIndex;
        private readonly EventType _fragmentEventType;
        private readonly EventAdapterService _eventAdapterService;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="propertyIndex">property index</param>
        /// <param name="fragmentEventType">event type of fragment</param>
        /// <param name="eventAdapterService">for creating event instances</param>
        public ObjectArrayFragmentArrayPropertyGetter(int propertyIndex, EventType fragmentEventType, EventAdapterService eventAdapterService)
        {
            this._propertyIndex = propertyIndex;
            this._fragmentEventType = fragmentEventType;
            this._eventAdapterService = eventAdapterService;
        }

        public Object GetObjectArray(Object[] array)
        {
            return array[_propertyIndex];
        }

        public bool IsObjectArrayExistsProperty(Object[] array)
        {
            return true;
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
            if (value is EventBean[])
            {
                return value;
            }
            return BaseNestableEventUtil.GetBNFragmentArray(value, _fragmentEventType, _eventAdapterService);
        }

        private string GetFragmentCodegen(ICodegenContext context)
        {
            var mSvc = context.MakeAddMember(typeof(EventAdapterService), _eventAdapterService);
            var mType = context.MakeAddMember(typeof(EventType), _fragmentEventType);
            return context.AddMethod(typeof(Object), typeof(Object[]), "oa", this.GetType())
                    .DeclareVar(typeof(Object), "value", CodegenUnderlyingGet(Ref("oa"), context))
                    .IfInstanceOf("value", typeof(EventBean[]))
                    .BlockReturn(Ref("value"))
                    .MethodReturn(StaticMethod(typeof(BaseNestableEventUtil), "GetBNFragmentArray",
                        Ref("value"),
                        Ref(mType.MemberName),
                        Ref(mSvc.MemberName)));
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
            return LocalMethod(GetFragmentCodegen(context), underlyingExpression);
        }
    }
} // end of namespace