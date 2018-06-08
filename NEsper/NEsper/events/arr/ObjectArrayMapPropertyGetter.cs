///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.codegen.core;
using com.espertech.esper.codegen.model.expression;
using com.espertech.esper.events.map;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.arr
{
    using Map = IDictionary<string, object>;

    public class ObjectArrayMapPropertyGetter : ObjectArrayEventPropertyGetter
    {
        private readonly int _index;
        private readonly MapEventPropertyGetter _getter;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="getter">is the getter to use to interrogate the property in the map</param>
        /// <param name="index">index</param>
        public ObjectArrayMapPropertyGetter(int index, MapEventPropertyGetter getter)
        {
            _index = index;
            _getter = getter ?? throw new ArgumentException("getter is a required parameter");
        }

        public Object GetObjectArray(Object[] array)
        {
            var valueTopObj = array[_index] as Map;
            if (valueTopObj != null)
            {
                return _getter.GetMap(valueTopObj);
            }

            return null;
        }

        private string GetObjectArrayCodegen(ICodegenContext context)
        {
            return context.AddMethod(typeof(Object), typeof(Object[]), "array", this.GetType())
                .DeclareVar(typeof(Object), "valueTopObj", ArrayAtIndex(Ref("array"), Constant(_index)))
                .IfRefNotTypeReturnConst("valueTopObj", typeof(Map), null)
                .MethodReturn(_getter.CodegenUnderlyingGet(Cast(typeof(Map), Ref("valueTopObj")), context));
        }

        public bool IsObjectArrayExistsProperty(Object[] array)
        {
            Object valueTopObj = array[_index];
            if (!(valueTopObj is Map))
            {
                return false;
            }
            return _getter.IsMapExistsProperty((Map)valueTopObj);
        }

        private string IsObjectArrayExistsPropertyCodegen(ICodegenContext context)
        {
            return context.AddMethod(typeof(bool), typeof(Object[]), "array", this.GetType())
                .DeclareVar(typeof(Object), "valueTopObj", ArrayAtIndex(Ref("array"), Constant(_index)))
                .IfRefNotTypeReturnConst("valueTopObj", typeof(Map), false)
                .MethodReturn(_getter.CodegenUnderlyingExists(Cast(typeof(Map), Ref("valueTopObj")), context));
        }

        public Object Get(EventBean eventBean)
        {
            Object[] array = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(eventBean);
            return GetObjectArray(array);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            Object[] array = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(eventBean);
            return IsObjectArrayExistsProperty(array);
        }

        public Object GetFragment(EventBean eventBean)
        {
            return null;
        }

        public ICodegenExpression CodegenEventBeanGet(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingGet(CastUnderlying(typeof(Object[]), beanExpression), context);
        }

        public ICodegenExpression CodegenEventBeanExists(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingExists(CastUnderlying(typeof(Object[]), beanExpression), context);
        }

        public ICodegenExpression CodegenEventBeanFragment(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return ConstantNull();
        }

        public ICodegenExpression CodegenUnderlyingGet(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return LocalMethod(GetObjectArrayCodegen(context), underlyingExpression);
        }

        public ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return LocalMethod(IsObjectArrayExistsPropertyCodegen(context), underlyingExpression);
        }

        public ICodegenExpression CodegenUnderlyingFragment(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return ConstantNull();
        }
    }
} // end of namespace