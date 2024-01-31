///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.util;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.magic;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.
    CodegenRelational;

namespace com.espertech.esper.common.@internal.@event.bean.getter
{
    /// <summary>
    ///     Getter for a list property backed by a field, identified by a given index, using vanilla reflection.
    /// </summary>
    public class ListFieldPropertyGetter : BaseNativePropertyGetter,
        BeanEventPropertyGetter,
        EventPropertyGetterAndIndexed
    {
        private readonly FieldInfo _field;
        private readonly int _index;

        public ListFieldPropertyGetter(
            FieldInfo field,
            int index,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
            : base(
                eventBeanTypedEventFactory,
                beanEventTypeFactory,
                TypeHelper.GetGenericFieldType(field, false))
        {
            _index = index;
            _field = field;

            if (index < 0) {
                throw new ArgumentException("Invalid negative index value");
            }
        }

        public object GetBeanProp(object @object)
        {
            return GetBeanPropInternal(@object, _index);
        }

        public bool IsBeanExistsProperty(object @object)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public override object Get(EventBean obj)
        {
            var underlying = obj.Underlying;
            return GetBeanProp(underlying);
        }

        public override bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        //public override Type BeanPropType => TypeHelper.GetGenericFieldType(_field, false);

        public override Type TargetType => _field.DeclaringType;

        public override CodegenExpression EventBeanGetCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingGetCodegen(
                CastUnderlying(TargetType, beanExpression),
                codegenMethodScope,
                codegenClassScope);
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
            return LocalMethod(
                GetBeanPropInternalCodegen(codegenMethodScope, codegenClassScope),
                underlyingExpression,
                Constant(_index));
        }

        public override CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ConstantTrue();
        }

        public object Get(
            EventBean eventBean,
            int index)
        {
            return GetBeanPropInternal(eventBean.Underlying, index);
        }

        private object GetBeanPropInternal(
            object @object,
            int index)
        {
            try {
                var value = _field.GetValue(@object);
                var valueList = value.AsObjectList(MagicMarker.SingletonInstance);
                if (valueList != null) {
                    if (valueList.Count <= index) {
                        return null;
                    }

                    return valueList[index];
                }

                return null;
            }
            catch (InvalidCastException e) {
                throw PropertyUtility.GetMismatchException(_field, @object, e);
            }
        }

        private CodegenMethod GetBeanPropInternalCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(BeanPropType, GetType(), codegenClassScope)
                .AddParam(TargetType, "@object")
                .AddParam<int>("index")
                .Block
                .DeclareVar<object>("value", ExprDotName(Ref("@object"), _field.Name))
                .DeclareVar<IList<object>>(
                    "l",
                    CodegenLegoCast.CastSafeFromObjectType(
                        typeof(IList<object>),
                        Ref("value")))
                .IfRefNullReturnNull("l")
                .IfConditionReturnConst(Relational(ExprDotName(Ref("l"), "Count"), LE, Ref("index")), null)
                .MethodReturn(
                    Cast(
                        BeanPropType,
                        ArrayAtIndex(Ref("l"), Ref("index"))));
        }

        public override string ToString()
        {
            return $"ListFieldPropertyGetter field={_field} index={_index}";
        }

        public CodegenExpression EventBeanGetIndexedCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope,
            CodegenExpression beanExpression,
            CodegenExpression key)
        {
            return LocalMethod(
                GetBeanPropInternalCodegen(codegenMethodScope, codegenClassScope),
                CastUnderlying(TargetType, beanExpression),
                key);
        }
    }
} // end of namespace