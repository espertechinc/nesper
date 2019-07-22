///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.bean.getter;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.arr
{
    /// <summary>
    ///     A getter that works on POJO events residing within a Map as an event property.
    /// </summary>
    public class ObjectArrayPOJOEntryPropertyGetter : BaseNativePropertyGetter,
        ObjectArrayEventPropertyGetter
    {
        private readonly BeanEventPropertyGetter entryGetter;
        private readonly int propertyIndex;

        public ObjectArrayPOJOEntryPropertyGetter(
            int propertyIndex,
            BeanEventPropertyGetter entryGetter,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory,
            Type returnType,
            Type nestedComponentType)
            : base(
                eventBeanTypedEventFactory,
                beanEventTypeFactory,
                returnType,
                nestedComponentType)
        {
            this.propertyIndex = propertyIndex;
            this.entryGetter = entryGetter;
        }

        public override Type TargetType => typeof(object[]);

        public override Type BeanPropType => typeof(object);

        public object GetObjectArray(object[] array)
        {
            // If the map does not contain the key, this is allowed and represented as null
            var value = array[propertyIndex];

            if (value == null) {
                return null;
            }

            // Object within the map
            if (value is EventBean) {
                return entryGetter.Get((EventBean) value);
            }

            return entryGetter.GetBeanProp(value);
        }

        public bool IsObjectArrayExistsProperty(object[] array)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public override object Get(EventBean obj)
        {
            var array = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(obj);
            return GetObjectArray(array);
        }

        public override bool IsExistsProperty(EventBean eventBean)
        {
            var array = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(eventBean);
            return IsExistsProperty(array);
        }

        public override CodegenExpression EventBeanGetCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingGetCodegen(
                CastUnderlying(typeof(object[]), beanExpression),
                codegenMethodScope,
                codegenClassScope);
        }

        public override CodegenExpression EventBeanExistsCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingExistsCodegen(
                CastUnderlying(typeof(object[]), beanExpression),
                codegenMethodScope,
                codegenClassScope);
        }

        public override CodegenExpression UnderlyingGetCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(GetObjectArrayCodegen(codegenMethodScope, codegenClassScope), underlyingExpression);
        }

        public override CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(IsExistsPropertyCodegen(codegenMethodScope, codegenClassScope), underlyingExpression);
        }

        private CodegenMethod GetObjectArrayCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam(typeof(object[]), "array")
                .Block
                .DeclareVar<object>("value", ArrayAtIndex(Ref("array"), Constant(propertyIndex)))
                .IfRefNullReturnNull("value")
                .IfInstanceOf("value", typeof(EventBean))
                .BlockReturn(
                    entryGetter.EventBeanGetCodegen(
                        CastRef(typeof(EventBean), "value"),
                        codegenMethodScope,
                        codegenClassScope))
                .MethodReturn(
                    entryGetter.UnderlyingGetCodegen(
                        Cast(entryGetter.TargetType, Ref("value")),
                        codegenMethodScope,
                        codegenClassScope));
        }

        private bool IsExistsProperty(object[] array)
        {
            var value = array[propertyIndex];

            if (value == null) {
                return false;
            }

            // Object within the map
            if (value is EventBean) {
                return entryGetter.IsExistsProperty((EventBean) value);
            }

            return entryGetter.IsBeanExistsProperty(value);
        }

        private CodegenMethod IsExistsPropertyCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(typeof(bool), GetType(), codegenClassScope)
                .AddParam(typeof(object[]), "array")
                .Block
                .DeclareVar<object>("value", ArrayAtIndex(Ref("array"), Constant(propertyIndex)))
                .IfRefNullReturnFalse("value")
                .IfInstanceOf("value", typeof(EventBean))
                .BlockReturn(
                    entryGetter.EventBeanExistsCodegen(
                        CastRef(typeof(EventBean), "value"),
                        codegenMethodScope,
                        codegenClassScope))
                .MethodReturn(
                    entryGetter.UnderlyingExistsCodegen(
                        Cast(entryGetter.TargetType, Ref("value")),
                        codegenMethodScope,
                        codegenClassScope));
        }
    }
} // end of namespace