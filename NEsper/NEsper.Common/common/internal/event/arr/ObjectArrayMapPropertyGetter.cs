///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.map;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.arr
{
    using Map = IDictionary<string, object>;

    public class ObjectArrayMapPropertyGetter : ObjectArrayEventPropertyGetter
    {
        private readonly MapEventPropertyGetter getter;
        private readonly int index;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="getter">is the getter to use to interrogate the property in the map</param>
        /// <param name="index">index</param>
        public ObjectArrayMapPropertyGetter(
            int index,
            MapEventPropertyGetter getter)
        {
            if (getter == null) {
                throw new ArgumentException("Getter is a required parameter");
            }

            this.index = index;
            this.getter = getter;
        }

        public object GetObjectArray(object[] array)
        {
            var valueTopObj = array[index];
            if (!(valueTopObj is Map)) {
                return null;
            }

            return getter.GetMap((Map) valueTopObj);
        }

        public bool IsObjectArrayExistsProperty(object[] array)
        {
            var valueTopObj = array[index];
            if (!(valueTopObj is Map)) {
                return false;
            }

            return getter.IsMapExistsProperty((Map) valueTopObj);
        }

        public object Get(EventBean eventBean)
        {
            var array = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(eventBean);
            return GetObjectArray(array);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            var array = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(eventBean);
            return IsObjectArrayExistsProperty(array);
        }

        public object GetFragment(EventBean eventBean)
        {
            return null;
        }

        public CodegenExpression EventBeanGetCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingGetCodegen(
                CastUnderlying(typeof(object[]), beanExpression),
                codegenMethodScope,
                codegenClassScope);
        }

        public CodegenExpression EventBeanExistsCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingExistsCodegen(
                CastUnderlying(typeof(object[]), beanExpression),
                codegenMethodScope,
                codegenClassScope);
        }

        public CodegenExpression EventBeanFragmentCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        public CodegenExpression UnderlyingGetCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(GetObjectArrayCodegen(codegenMethodScope, codegenClassScope), underlyingExpression);
        }

        public CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(
                IsObjectArrayExistsPropertyCodegen(codegenMethodScope, codegenClassScope),
                underlyingExpression);
        }

        public CodegenExpression UnderlyingFragmentCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        private CodegenMethod GetObjectArrayCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam(typeof(object[]), "array")
                .Block
                .DeclareVar<object>("valueTopObj", ArrayAtIndex(Ref("array"), Constant(index)))
                .IfRefNotTypeReturnConst("valueTopObj", typeof(Map), null)
                .MethodReturn(
                    getter.UnderlyingGetCodegen(
                        Cast(typeof(Map), Ref("valueTopObj")),
                        codegenMethodScope,
                        codegenClassScope));
        }

        private CodegenMethod IsObjectArrayExistsPropertyCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(typeof(bool), GetType(), codegenClassScope)
                .AddParam(typeof(object[]), "array")
                .Block
                .DeclareVar<object>("valueTopObj", ArrayAtIndex(Ref("array"), Constant(index)))
                .IfRefNotTypeReturnConst("valueTopObj", typeof(Map), false)
                .MethodReturn(
                    getter.UnderlyingExistsCodegen(
                        Cast(typeof(Map), Ref("valueTopObj")),
                        codegenMethodScope,
                        codegenClassScope));
        }
    }
} // end of namespace