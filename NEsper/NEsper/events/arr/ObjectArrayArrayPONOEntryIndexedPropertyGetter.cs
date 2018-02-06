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
    /// A getter that works on arrays residing within a Map as an event property.
    /// </summary>
    public class ObjectArrayArrayPonoEntryIndexedPropertyGetter
        : BaseNativePropertyGetter
        , ObjectArrayEventPropertyGetterAndIndexed
    {
        private readonly int _index;
        private readonly int _propertyIndex;

        public static object GetArrayValue(object[] array, int propertyIndex, int index)
        {
            // If the oa does not contain the key, this is allowed and represented as null
            var value = array [propertyIndex];
            return BaseNestableEventUtil.GetBNArrayValueAtIndexWithNullCheck(value, index);
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="propertyIndex">MapIndex of the property.</param>
        /// <param name="index">the index to fetch the array element for</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        /// <param name="returnType">type of the entry returned</param>
        public ObjectArrayArrayPonoEntryIndexedPropertyGetter(
                int propertyIndex,
                int index,
                EventAdapterService eventAdapterService,
                Type returnType)
            : base(eventAdapterService, returnType, null)
        {
            _propertyIndex = propertyIndex;
            _index = index;
        }

        public object GetObjectArray(object[] array)
        {
            return GetArrayValue(array, _propertyIndex, _index);
        }

        public bool IsObjectArrayExistsProperty(object[] array)
        {
            return array.Length > _index;
        }

        public object Get(EventBean eventBean, int index)
        {
            object[] array = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(eventBean);
            return GetArrayValue(array, _propertyIndex, index);
        }

        public override object Get(EventBean eventBean)
        {
            object[] array = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(eventBean);
            return GetObjectArray(array);
        }

        public override bool IsExistsProperty(EventBean eventBean)
        {
            object[] array = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(eventBean);
            return array.Length > _index;
        }

        public override ICodegenExpression CodegenEventBeanGet(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingGet(CastUnderlying(typeof(object[]), beanExpression), context);
        }

        public override ICodegenExpression CodegenEventBeanExists(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingExists(CastUnderlying(typeof(object[]), beanExpression), context);
        }

        public override ICodegenExpression CodegenUnderlyingGet(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return StaticMethod(GetType(), "GetArrayValue", underlyingExpression,
                Constant(_propertyIndex),
                Constant(_index));
        }

        public override ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return Relational(
                ArrayLength(underlyingExpression), CodegenRelational.GT,
                Constant(_index));
        }

        public override Type TargetType => typeof(object[]);
        public override Type BeanPropType => typeof(object);
    }
}