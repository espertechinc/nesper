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
using com.espertech.esper.compat.collections;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.map
{
    using Map = IDictionary<string, object>;

    /// <summary>Returns the event bean or the underlying array.</summary>
    public class MapEventBeanArrayPropertyGetter : MapEventPropertyGetter
    {
        private readonly string _propertyName;
        private readonly Type _underlyingType;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="propertyName">property to get</param>
        /// <param name="underlyingType">type of property</param>
        public MapEventBeanArrayPropertyGetter(string propertyName, Type underlyingType)
        {
            _propertyName = propertyName;
            _underlyingType = underlyingType;
        }

        public object GetMap(IDictionary<string, object> map)
        {
            var mapValue = map.Get(_propertyName);
            return BaseNestableEventUtil.GetArrayPropertyAsUnderlyingsArray(_underlyingType, (EventBean[]) mapValue);
        }

        public bool IsMapExistsProperty(IDictionary<string, object> map)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public object Get(EventBean obj)
        {
            return GetMap(BaseNestableEventUtil.CheckedCastUnderlyingMap(obj));
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public object GetFragment(EventBean obj)
        {
            var map = BaseNestableEventUtil.CheckedCastUnderlyingMap(obj);
            return map.Get(_propertyName);
        }

        public ICodegenExpression CodegenEventBeanGet(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingGet(CastUnderlying(typeof(Map), beanExpression), context);
        }

        public ICodegenExpression CodegenEventBeanExists(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return ConstantTrue();
        }

        public ICodegenExpression CodegenEventBeanFragment(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingFragment(CastUnderlying(typeof(Map), beanExpression), context);
        }

        public ICodegenExpression CodegenUnderlyingGet(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return LocalMethod(GetMapCodegen(context), underlyingExpression);
        }

        public ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression,
            ICodegenContext context)
        {
            return ConstantTrue();
        }

        public ICodegenExpression CodegenUnderlyingFragment(ICodegenExpression underlyingExpression,
            ICodegenContext context)
        {
            return ExprDotMethod(underlyingExpression, "get", Constant(_propertyName));
        }

        private string GetMapCodegen(ICodegenContext context)
        {
            return context.AddMethod(typeof(object), typeof(Map), "map", GetType())
                .DeclareVar(typeof(object), "mapValue",
                    ExprDotMethod(Ref("map"), "get",
                        Constant(_propertyName)))
                .MethodReturn(LocalMethod(
                    BaseNestableEventUtil.GetArrayPropertyAsUnderlyingsArrayCodegen(_underlyingType, context),
                    Cast(typeof(EventBean[]), Ref("mapValue"))));
        }
    }
} // end of namespace