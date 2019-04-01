///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.@event.core;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.arr
{
    /// <summary>
    ///     Getter for map array.
    /// </summary>
    public class ObjectArrayFragmentArrayPropertyGetter : ObjectArrayEventPropertyGetter
    {
        private readonly EventBeanTypedEventFactory eventBeanTypedEventFactory;
        private readonly EventType fragmentEventType;
        private readonly int propertyIndex;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="propertyIndex">property index</param>
        /// <param name="fragmentEventType">event type of fragment</param>
        /// <param name="eventBeanTypedEventFactory">for creating event instances</param>
        public ObjectArrayFragmentArrayPropertyGetter(
            int propertyIndex, EventType fragmentEventType, EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            this.propertyIndex = propertyIndex;
            this.fragmentEventType = fragmentEventType;
            this.eventBeanTypedEventFactory = eventBeanTypedEventFactory;
        }

        public object GetObjectArray(object[] array)
        {
            return array[propertyIndex];
        }

        public bool IsObjectArrayExistsProperty(object[] array)
        {
            return true;
        }

        public object Get(EventBean obj)
        {
            var array = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(obj);
            return GetObjectArray(array);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true;
        }

        public object GetFragment(EventBean eventBean)
        {
            var value = Get(eventBean);
            if (value is EventBean[]) {
                return value;
            }

            return BaseNestableEventUtil.GetBNFragmentArray(value, fragmentEventType, eventBeanTypedEventFactory);
        }

        private CodegenMethod GetFragmentCodegen(
            CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope)
        {
            var mSvc = codegenClassScope.AddOrGetFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
            var mType = codegenClassScope.AddFieldUnshared(
                true, typeof(EventType),
                EventTypeUtility.ResolveTypeCodegen(fragmentEventType, EPStatementInitServicesConstants.REF));
            return codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam(typeof(object[]), "oa").Block
                .DeclareVar(
                    typeof(object), "value", UnderlyingGetCodegen(Ref("oa"), codegenMethodScope, codegenClassScope))
                .IfInstanceOf("value", typeof(EventBean[]))
                .BlockReturn(Ref("value"))
                .MethodReturn(
                    StaticMethod(typeof(BaseNestableEventUtil), "getBNFragmentArray", Ref("value"), mType, mSvc));
        }

        public CodegenExpression EventBeanGetCodegen(
            CodegenExpression beanExpression, CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingGetCodegen(
                CastUnderlying(typeof(object[]), beanExpression), codegenMethodScope, codegenClassScope);
        }

        public CodegenExpression EventBeanExistsCodegen(
            CodegenExpression beanExpression, CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ConstantTrue();
        }

        public CodegenExpression EventBeanFragmentCodegen(
            CodegenExpression beanExpression, CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingFragmentCodegen(
                CastUnderlying(typeof(object[]), beanExpression), codegenMethodScope, codegenClassScope);
        }

        public CodegenExpression UnderlyingGetCodegen(
            CodegenExpression underlyingExpression, CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ArrayAtIndex(underlyingExpression, Constant(propertyIndex));
        }

        public CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression, CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ConstantTrue();
        }

        public CodegenExpression UnderlyingFragmentCodegen(
            CodegenExpression underlyingExpression, CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(GetFragmentCodegen(codegenMethodScope, codegenClassScope), underlyingExpression);
        }
    }
} // end of namespace