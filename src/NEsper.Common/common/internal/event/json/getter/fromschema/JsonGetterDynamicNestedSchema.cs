///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.common.@internal.@event.json.getter.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.json.getter.fromschema
{
    /// <summary>
    ///     Property getter for Json underlying fields.
    /// </summary>
    public sealed class JsonGetterDynamicNestedSchema : JsonEventPropertyGetter
    {
        private readonly JsonEventPropertyGetter _innerGetter;
        private readonly string _propertyName;
        private readonly string _underlyingClassName;

        public JsonGetterDynamicNestedSchema(
            string propertyName,
            JsonEventPropertyGetter innerGetter,
            string underlyingClassName)
        {
            this._propertyName = propertyName;
            this._innerGetter = innerGetter;
            this._underlyingClassName = underlyingClassName;
        }

        public object Get(EventBean eventBean)
        {
            return GetJsonProp(eventBean.Underlying);
        }

        public object GetJsonProp(object @object)
        {
            var inner = ((IDictionary<string, object>) @object).Get(_propertyName);
            return inner != null ? _innerGetter.GetJsonProp(inner) : null;
        }

        public CodegenExpression EventBeanGetCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingGetCodegen(CastUnderlying(_underlyingClassName, beanExpression), codegenMethodScope, codegenClassScope);
        }

        public CodegenExpression UnderlyingGetCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var method = codegenMethodScope
                .MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam(_underlyingClassName, "und");
            method.Block
                .DeclareVar<object>("inner", ExprDotMethod(Ref("und"), "Get", Constant(_propertyName)))
                .IfNotInstanceOf("inner", typeof(IDictionary<string, object>))
                .BlockReturn(ConstantNull())
                .MethodReturn(_innerGetter.UnderlyingGetCodegen(Cast(typeof(IDictionary<string, object>), Ref("inner")), method, codegenClassScope));
            return LocalMethod(method, underlyingExpression);
        }

        public CodegenExpression EventBeanExistsCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingExistsCodegen(CastUnderlying(_underlyingClassName, beanExpression), codegenMethodScope, codegenClassScope);
        }

        public CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var method = codegenMethodScope
                .MakeChild(typeof(bool), GetType(), codegenClassScope)
                .AddParam(_underlyingClassName, "und");
            method.Block
                .DeclareVar<object>("inner", ExprDotMethod(Ref("und"), "Get", Constant(_propertyName)))
                .IfNotInstanceOf("inner", typeof(IDictionary<string, object>))
                .BlockReturn(ConstantFalse())
                .MethodReturn(_innerGetter.UnderlyingExistsCodegen(Cast(typeof(IDictionary<string, object>), Ref("inner")), method, codegenClassScope));
            return LocalMethod(method, underlyingExpression);
        }

        public CodegenExpression EventBeanFragmentCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        public CodegenExpression UnderlyingFragmentCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return GetJsonExists(eventBean.Underlying);
        }

        public object GetFragment(EventBean eventBean)
        {
            return null;
        }

        public object GetJsonFragment(object @object)
        {
            return null;
        }

        public bool GetJsonExists(object @object)
        {
            var inner = ((JsonEventObjectBase) @object).Get(_propertyName);
            if (!(inner is IDictionary<string, object>)) {
                return false;
            }

            return _innerGetter.GetJsonExists(inner);
        }
    }
} // end of namespace