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
    ///     A getter that works on PONO events residing within a Map as an event property.
    /// </summary>
    public class ObjectArrayPONOEntryPropertyGetter : BaseNativePropertyGetter,
        ObjectArrayEventPropertyGetter
    {
        private readonly BeanEventPropertyGetter _entryGetter;
        private readonly int _propertyIndex;

        public ObjectArrayPONOEntryPropertyGetter(
            int propertyIndex,
            BeanEventPropertyGetter entryGetter,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory,
            Type returnType)
            : base(
                eventBeanTypedEventFactory,
                beanEventTypeFactory,
                returnType)
        {
            _propertyIndex = propertyIndex;
            _entryGetter = entryGetter;
        }

        public override Type TargetType => typeof(object[]);

        //public override Type BeanPropType => typeof(object);

        public object GetObjectArray(object[] array)
        {
            // If the map does not contain the key, this is allowed and represented as null
            var value = array[_propertyIndex];

            if (value == null) {
                return null;
            }

            // Object within the map
            if (value is EventBean bean) {
                return _entryGetter.Get(bean);
            }

            return _entryGetter.GetBeanProp(value);
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
            var itemType = BeanPropType;
            return codegenMethodScope
                .MakeChild(itemType, GetType(), codegenClassScope)
                .AddParam<object[]>("array")
                .Block
                .DeclareVar<object>("value", ArrayAtIndex(Ref("array"), Constant(_propertyIndex)))
                .IfRefNullReturnDefault("value")
                .IfInstanceOf("value", typeof(EventBean))
                .BlockReturn(
                    _entryGetter.EventBeanGetCodegen(
                        CastRef(typeof(EventBean), "value"),
                        codegenMethodScope,
                        codegenClassScope))
                .MethodReturn(
                    _entryGetter.UnderlyingGetCodegen(
                        Cast(_entryGetter.TargetType, Ref("value")),
                        codegenMethodScope,
                        codegenClassScope));
        }

        private bool IsExistsProperty(object[] array)
        {
            var value = array[_propertyIndex];

            if (value == null) {
                return false;
            }

            // Object within the map
            if (value is EventBean bean) {
                return _entryGetter.IsExistsProperty(bean);
            }

            return _entryGetter.IsBeanExistsProperty(value);
        }

        private CodegenMethod IsExistsPropertyCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(typeof(bool), GetType(), codegenClassScope)
                .AddParam<object[]>("array")
                .Block
                .DeclareVar<object>("value", ArrayAtIndex(Ref("array"), Constant(_propertyIndex)))
                .IfRefNullReturnFalse("value")
                .IfInstanceOf("value", typeof(EventBean))
                .BlockReturn(
                    _entryGetter.EventBeanExistsCodegen(
                        CastRef(typeof(EventBean), "value"),
                        codegenMethodScope,
                        codegenClassScope))
                .MethodReturn(
                    _entryGetter.UnderlyingExistsCodegen(
                        Cast(_entryGetter.TargetType, Ref("value")),
                        codegenMethodScope,
                        codegenClassScope));
        }
    }
} // end of namespace