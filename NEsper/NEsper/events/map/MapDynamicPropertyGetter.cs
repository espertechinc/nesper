///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.codegen.core;
using com.espertech.esper.codegen.model.expression;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.map
{
    using Map = IDictionary<string, object>;

    public class MapDynamicPropertyGetter : MapEventPropertyGetter
    {
        private readonly string _propertyName;

        public MapDynamicPropertyGetter(string propertyName)
        {
            this._propertyName = propertyName;
        }

        public object GetMap(IDictionary<string, object> map)
        {
            return map.Get(_propertyName);
        }

        public bool IsMapExistsProperty(IDictionary<string, object> map)
        {
            return map.ContainsKey(_propertyName);
        }

        public object Get(EventBean eventBean)
        {
            var map = (Map) eventBean.Underlying;
            return map.Get(_propertyName);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            var map = (Map) eventBean.Underlying;
            return map.ContainsKey(_propertyName);
        }

        public object GetFragment(EventBean eventBean)
        {
            return null;
        }

        public ICodegenExpression CodegenEventBeanGet(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return ExprDotMethod(CastUnderlying(typeof(Map), beanExpression), "get",
                Constant(_propertyName));
        }

        public ICodegenExpression CodegenEventBeanExists(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return ExprDotMethod(CastUnderlying(typeof(Map), beanExpression),
                "containsKey", Constant(_propertyName));
        }

        public ICodegenExpression CodegenEventBeanFragment(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return ConstantNull();
        }

        public ICodegenExpression CodegenUnderlyingGet(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            return ExprDotMethod(underlyingExpression, "Get", Constant(_propertyName));
        }

        public ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression,
            ICodegenContext context)
        {
            return ExprDotMethod(underlyingExpression, "containsKey",
                Constant(_propertyName));
        }

        public ICodegenExpression CodegenUnderlyingFragment(ICodegenExpression underlyingExpression,
            ICodegenContext context)
        {
            return ConstantNull();
        }
    }
} // end of namespace