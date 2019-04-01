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
    public class ObjectArrayArrayPONOBeanEntryIndexedPropertyGetter : BaseNativePropertyGetter,
        ObjectArrayEventPropertyGetter
    {
        private readonly int index;
        private readonly BeanEventPropertyGetter nestedGetter;

        private readonly int propertyIndex;

        public ObjectArrayArrayPONOBeanEntryIndexedPropertyGetter(
            int propertyIndex,
            int index,
            BeanEventPropertyGetter nestedGetter,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory,
            Type returnType)
            : base(eventBeanTypedEventFactory, beanEventTypeFactory, returnType, null)
        {
            this.propertyIndex = propertyIndex;
            this.index = index;
            this.nestedGetter = nestedGetter;
        }

        public override Type TargetType => typeof(object[]);

        public override Type BeanPropType => typeof(object);

        public object GetObjectArray(object[] array)
        {
            // If the map does not contain the key, this is allowed and represented as null
            var value = array[propertyIndex];
            return BaseNestableEventUtil.GetBeanArrayValue(nestedGetter, value, index);
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
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public override CodegenExpression EventBeanGetCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingGetCodegen(
                CastUnderlying(typeof(object[]), beanExpression), codegenMethodScope, codegenClassScope);
        }

        public override CodegenExpression EventBeanExistsCodegen(
            CodegenExpression beanExpression, 
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ConstantTrue();
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
            return ConstantTrue();
        }

        private CodegenMethod GetObjectArrayCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var method = codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam(typeof(object[]), "array");
            method.Block
                .DeclareVar(typeof(object), "value", ArrayAtIndex(Ref("array"), Constant(propertyIndex)))
                .MethodReturn(
                    LocalMethod(
                        BaseNestableEventUtil.GetBeanArrayValueCodegen(
                            codegenMethodScope, codegenClassScope, nestedGetter, index), Ref("value")));
            return method;
        }
    }
} // end of namespace