///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.util;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.
    CodegenRelational;

namespace com.espertech.esper.common.@internal.@event.bean.getter
{
    /// <summary>
    ///     Getter for an array property backed by a property, identified by a given index, using vanilla reflection.
    /// </summary>
    public class ArrayPropertyPropertyGetter : BaseNativePropertyGetter,
        BeanEventPropertyGetter,
        EventPropertyGetterAndIndexed
    {
        private readonly PropertyInfo _prop;
        private readonly int _index;

        public ArrayPropertyPropertyGetter(
            PropertyInfo prop,
            int index,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
            : base(
                eventBeanTypedEventFactory,
                beanEventTypeFactory,
                prop.PropertyType.GetElementType())
        {
            _index = index;
            _prop = prop;

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
            return GetBeanProp(obj.Underlying);
        }

        public override bool IsExistsProperty(EventBean eventBean)
        {
            var underlying = eventBean.Underlying;
            return GetBeanPropInternalExists(underlying, _index);
        }

        public override Type BeanPropType => _prop.PropertyType.GetElementType().GetBoxedType();

        public override Type TargetType => _prop.DeclaringType;

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
            return UnderlyingExistsCodegen(
                CastUnderlying(TargetType, beanExpression),
                codegenMethodScope,
                codegenClassScope);
        }

        public override CodegenExpression UnderlyingGetCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(
                GetBeanPropInternalCode(codegenMethodScope, _prop, codegenClassScope),
                underlyingExpression,
                Constant(_index));
        }

        public override CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(GetBeanPropInternalExistsCode(codegenMethodScope, _prop, codegenClassScope), underlyingExpression, Constant(_index));
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
                var value = (Array) _prop.GetValue(@object);
                return CollectionUtil.ArrayValueAtIndex(value, index);
            }
            catch (InvalidCastException e) {
                throw PropertyUtility.GetMismatchException(_prop, @object, e);
            }
            catch (TargetException e) {
                throw PropertyUtility.GetTargetException(_prop, e);
            }
            catch (TargetInvocationException e) {
                throw PropertyUtility.GetTargetException(_prop, e);
            }
            catch (MemberAccessException e) {
                throw PropertyUtility.GetMemberAccessException(_prop, e);
            }
            catch (ArgumentException e) {
                throw PropertyUtility.GetArgumentException(_prop, e);
            }
        }
        
        private bool GetBeanPropInternalExists(
            object @object,
            int index)
        {
            try {
                var value = (Array) _prop.GetValue(@object);
                return CollectionUtil.ArrayExistsAtIndex(value, index);
            }
            catch (InvalidCastException e) {
                throw PropertyUtility.GetMismatchException(_prop, @object, e);
            }
            catch (TargetException e) {
                throw PropertyUtility.GetTargetException(_prop, e);
            }
            catch (TargetInvocationException e) {
                throw PropertyUtility.GetTargetException(_prop, e);
            }
            catch (MemberAccessException e) {
                throw PropertyUtility.GetMemberAccessException(_prop, e);
            }
            catch (ArgumentException e) {
                throw PropertyUtility.GetArgumentException(_prop, e);
            }
        }

        internal static CodegenMethod GetBeanPropInternalCode(
            CodegenMethodScope codegenMethodScope,
            PropertyInfo property,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope
                .MakeChild(property.PropertyType.GetElementType().GetBoxedType(), typeof(ArrayMethodPropertyGetter), codegenClassScope)
                .AddParam(property.DeclaringType, "obj")
                .AddParam(typeof(int), "index")
                .Block
                .DeclareVar(property.PropertyType, "array", ExprDotName(Ref("obj"), property.Name))
                .IfRefNullReturnNull("array")
                .IfConditionReturnConst(
                    Relational(
                        ArrayLength(Ref("array")),
                        LE,
                        Ref("index")),
                    null)
                .MethodReturn(ArrayAtIndex(Ref("array"), Ref("index")));
        }
        
        internal static CodegenMethod GetBeanPropInternalExistsCode(
            CodegenMethodScope codegenMethodScope,
            PropertyInfo property,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope
                .MakeChild(typeof(bool), typeof(ArrayMethodPropertyGetter), codegenClassScope)
                .AddParam(property.DeclaringType, "obj")
                .AddParam(typeof(int), "index")
                .Block
                .DeclareVar(property.PropertyType, "array", ExprDotName(Ref("obj"), property.Name))
                .IfRefNullReturnFalse("array")
                .IfConditionReturnConst(
                    Relational(
                        ArrayLength(Ref("array")),
                        LE,
                        Ref("index")),
                    false)
                .MethodReturn(ConstantTrue());
        }
        
        public override string ToString()
        {
            return $"ArrayPropertyPropertyGetter property={_prop} index={_index}";
        }

        public CodegenExpression EventBeanGetIndexedCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope,
            CodegenExpression beanExpression,
            CodegenExpression key)
        {
            return LocalMethod(
                GetBeanPropInternalCode(codegenMethodScope, _prop, codegenClassScope),
                CastUnderlying(TargetType, beanExpression),
                key);
        }
    }
} // end of namespace