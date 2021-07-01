///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.map
{
    public class MapDynamicPropertyGetter : MapEventPropertyGetter
    {
        private readonly string propertyName;

        public MapDynamicPropertyGetter(string propertyName)
        {
            this.propertyName = propertyName;
        }

        public object GetMap(IDictionary<string, object> map)
        {
            return map.Get(propertyName);
        }

        public bool IsMapExistsProperty(IDictionary<string, object> map)
        {
            return map.ContainsKey(propertyName);
        }

        public object Get(EventBean eventBean)
        {
            if (eventBean.Underlying is IDictionary<object, object> objectMap) {
                return objectMap.Get(propertyName);
            }

            if (eventBean.Underlying is IDictionary<string, object> stringMap) {
                return stringMap.Get(propertyName);
            }

            return eventBean.Underlying
                .AsObjectDictionary()
                .Get(propertyName);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            if (eventBean.Underlying is IDictionary<object, object> objectMap) {
                return objectMap.ContainsKey(propertyName);
            }

            if (eventBean.Underlying is IDictionary<string, object> stringMap) {
                return stringMap.ContainsKey(propertyName);
            }

            return eventBean.Underlying
                .AsObjectDictionary()
                .ContainsKey(propertyName);
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
            return ExprDotMethod(
                CastUnderlying(typeof(IDictionary<string, object>), beanExpression),
                "Get",
                Constant(propertyName));
        }

        public CodegenExpression EventBeanExistsCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ExprDotMethod(
                CastUnderlying(typeof(IDictionary<string, object>), beanExpression),
                "CheckedContainsKey",
                Constant(propertyName));
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
            return ExprDotMethod(underlyingExpression, "Get", Constant(propertyName));
        }

        public CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ExprDotMethod(underlyingExpression, "CheckedContainsKey", Constant(propertyName));
        }

        public CodegenExpression UnderlyingFragmentCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }
    }
} // end of namespace