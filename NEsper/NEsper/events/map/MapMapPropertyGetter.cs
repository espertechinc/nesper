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

    /// <summary>
    ///     A getter that interrogates a given property in a map which may itself contain nested maps or indexed entries.
    /// </summary>
    public class MapMapPropertyGetter : MapEventPropertyGetter
    {
        private readonly MapEventPropertyGetter _getter;
        private readonly string _propertyMap;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="propertyMap">is the property returning the map to interrogate</param>
        /// <param name="getter">is the getter to use to interrogate the property in the map</param>
        public MapMapPropertyGetter(string propertyMap, MapEventPropertyGetter getter)
        {
            _propertyMap = propertyMap;
            _getter = getter ?? throw new ArgumentException("Getter is a required parameter");
        }

        public object GetMap(IDictionary<string, object> map)
        {
            var valueTopObj = map.Get(_propertyMap);
            if (!(valueTopObj is Map)) return null;
            return _getter.GetMap((Map) valueTopObj);
        }

        public bool IsMapExistsProperty(IDictionary<string, object> map)
        {
            var valueTopObj = map.Get(_propertyMap);
            if (!(valueTopObj is Map)) return false;
            return _getter.IsMapExistsProperty((Map) valueTopObj);
        }

        public object Get(EventBean eventBean)
        {
            return GetMap(BaseNestableEventUtil.CheckedCastUnderlyingMap(eventBean));
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return IsMapExistsProperty(BaseNestableEventUtil.CheckedCastUnderlyingMap(eventBean));
        }

        public object GetFragment(EventBean eventBean)
        {
            return null;
        }

        public ICodegenExpression CodegenEventBeanGet(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingGet(CastUnderlying(typeof(Map), beanExpression), context);
        }

        public ICodegenExpression CodegenEventBeanExists(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return CodegenUnderlyingExists(CastUnderlying(typeof(Map), beanExpression), context);
        }

        public ICodegenExpression CodegenEventBeanFragment(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return ConstantNull();
        }

        public ICodegenExpression CodegenUnderlyingGet(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return LocalMethod(GetMapMethodCodegen(context), underlyingExpression);
        }

        public ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression,
            ICodegenContext context)
        {
            return LocalMethod(IsMapExistsPropertyCodegen(context), underlyingExpression);
        }

        public ICodegenExpression CodegenUnderlyingFragment(ICodegenExpression underlyingExpression,
            ICodegenContext context)
        {
            return ConstantNull();
        }

        private string GetMapMethodCodegen(ICodegenContext context)
        {
            return context.AddMethod(typeof(object), typeof(Map), "map", GetType())
                .DeclareVar(typeof(object), "valueTopObj",
                    ExprDotMethod(Ref("map"), "get",
                        Constant(_propertyMap)))
                .IfRefNotTypeReturnConst("valueTopObj", typeof(Map), null)
                .DeclareVar(typeof(Map), "value", CastRef(typeof(Map), "valueTopObj"))
                .MethodReturn(_getter.CodegenUnderlyingGet(Ref("value"), context));
        }

        private string IsMapExistsPropertyCodegen(ICodegenContext context)
        {
            return context.AddMethod(typeof(bool), typeof(Map), "map", GetType())
                .DeclareVar(typeof(object), "valueTopObj",
                    ExprDotMethod(Ref("map"), "get",
                        Constant(_propertyMap)))
                .IfRefNotTypeReturnConst("valueTopObj", typeof(Map), false)
                .DeclareVar(typeof(Map), "value", CastRef(typeof(Map), "valueTopObj"))
                .MethodReturn(_getter.CodegenUnderlyingExists(Ref("value"), context));
        }
    }
} // end of namespace