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
        private readonly JsonEventPropertyGetter innerGetter;
        private readonly string propertyName;
        private readonly string underlyingClassName;

        public JsonGetterDynamicNestedSchema(
            string propertyName,
            JsonEventPropertyGetter innerGetter,
            string underlyingClassName)
        {
            this.propertyName = propertyName;
            this.innerGetter = innerGetter;
            this.underlyingClassName = underlyingClassName;
        }

        public object Get(EventBean eventBean)
        {
            return GetJsonProp(eventBean.Underlying);
        }

        public object GetJsonProp(object @object)
        {
            var inner = ((IDictionary<string, object>) @object).Get(propertyName);
            return inner != null ? innerGetter.GetJsonProp(inner) : null;
        }

        public CodegenExpression EventBeanGetCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingGetCodegen(CastUnderlying(underlyingClassName, beanExpression), codegenMethodScope, codegenClassScope);
        }

        public CodegenExpression UnderlyingGetCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var method = codegenMethodScope
                .MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam(underlyingClassName, "und");
            method.Block
                .DeclareVar(typeof(object), "inner", ExprDotMethod(Ref("und"), "get", Constant(propertyName)))
                .IfNotInstanceOf("inner", typeof(IDictionary<string, object>))
                .BlockReturn(ConstantNull())
                .MethodReturn(innerGetter.UnderlyingGetCodegen(Cast(typeof(IDictionary<string, object>), Ref("inner")), method, codegenClassScope));
            return LocalMethod(method, underlyingExpression);
        }

        public CodegenExpression EventBeanExistsCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingExistsCodegen(CastUnderlying(underlyingClassName, beanExpression), codegenMethodScope, codegenClassScope);
        }

        public CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var method = codegenMethodScope
                .MakeChild(typeof(bool), GetType(), codegenClassScope)
                .AddParam(underlyingClassName, "und");
            method.Block
                .DeclareVar(typeof(object), "inner", ExprDotMethod(Ref("und"), "Get", Constant(propertyName)))
                .IfNotInstanceOf("inner", typeof(IDictionary<string, object>))
                .BlockReturn(ConstantFalse())
                .MethodReturn(innerGetter.UnderlyingExistsCodegen(Cast(typeof(IDictionary<string, object>), Ref("inner")), method, codegenClassScope));
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
            var inner = ((JsonEventObjectBase) @object).Get(propertyName);
            if (!(inner is IDictionary<string, object>)) {
                return false;
            }

            return innerGetter.GetJsonExists(inner);
        }
    }
} // end of namespace