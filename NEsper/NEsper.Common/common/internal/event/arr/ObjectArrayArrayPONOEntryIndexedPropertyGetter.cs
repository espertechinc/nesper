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
using com.espertech.esper.common.@internal.@event.bean.getter;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.
    CodegenRelational;

namespace com.espertech.esper.common.@internal.@event.arr
{
    /// <summary>
    ///     A getter that works on arrays residing within a Map as an event property.
    /// </summary>
    public class ObjectArrayArrayPONOEntryIndexedPropertyGetter : BaseNativePropertyGetter,
        ObjectArrayEventPropertyGetterAndIndexed
    {
        private readonly int index;
        private readonly int propertyIndex;

        public ObjectArrayArrayPONOEntryIndexedPropertyGetter(
            int propertyIndex,
            int index,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory,
            Type returnType)
            : base(eventBeanTypedEventFactory, beanEventTypeFactory, returnType, null)
        {
            this.propertyIndex = propertyIndex;
            this.index = index;
        }

        public override Type TargetType => typeof(object[]);

        public override Type BeanPropType => typeof(object);

        public object GetObjectArray(object[] array)
        {
            return GetArrayValue(array, propertyIndex, index);
        }

        public bool IsObjectArrayExistsProperty(object[] array)
        {
            return array.Length > index;
        }

        public object Get(
            EventBean eventBean,
            int index)
        {
            var array = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(eventBean);
            return GetArrayValue(array, propertyIndex, index);
        }

        public override object Get(EventBean obj)
        {
            var array = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(obj);
            return GetObjectArray(array);
        }

        public override bool IsExistsProperty(EventBean eventBean)
        {
            var array = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(eventBean);
            return CollectionUtil.ArrayExistsAtIndex(array[propertyIndex], index);
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
            return StaticMethod(
                typeof(CollectionUtil),
                "ArrayExistsAtIndex",
                ArrayAtIndex(underlyingExpression, Constant(propertyIndex)),
                Constant(index));
        }

        public override CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return Relational(ArrayLength(underlyingExpression), GT, Constant(index));
        }

        public CodegenExpression EventBeanGetIndexedCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope,
            CodegenExpression beanExpression,
            CodegenExpression key)
        {
            return StaticMethod(
                GetType(),
                "GetArrayValue",
                CastUnderlying(typeof(object[]), beanExpression),
                Constant(propertyIndex),
                key);
        }

        public static object GetArrayValue(
            object[] array,
            int propertyIndex,
            int index)
        {
            // If the oa does not contain the key, this is allowed and represented as null
            var value = array[propertyIndex];
            return BaseNestableEventUtil.GetBNArrayValueAtIndexWithNullCheck(value, index);
        }
    }
} // end of namespace