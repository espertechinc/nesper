///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.json.getter.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.json.getter.fromschema
{
    /// <summary>
    ///     Property getter for Json underlying fields.
    /// </summary>
    public sealed class JsonGetterDynamicNestedChain : JsonEventPropertyGetter
    {
        private readonly JsonEventPropertyGetter[] _getters;
        private readonly string _underlyingClassName;

        public JsonGetterDynamicNestedChain(
            string underlyingClassName,
            JsonEventPropertyGetter[] getters)
        {
            _underlyingClassName = underlyingClassName;
            _getters = getters;
        }

        public CodegenExpression EventBeanGetCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingGetCodegen(
                CastUnderlying(_underlyingClassName, beanExpression),
                codegenMethodScope,
                codegenClassScope);
        }

        public CodegenExpression UnderlyingGetCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var resultExpression =
                _getters[0].UnderlyingGetCodegen(underlyingExpression, codegenMethodScope, codegenClassScope);
            return LocalMethod(
                HandleGetterTrailingChainCodegen(codegenMethodScope, codegenClassScope),
                resultExpression);
        }

        public CodegenExpression EventBeanExistsCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingExistsCodegen(
                CastUnderlying(_underlyingClassName, beanExpression),
                codegenMethodScope,
                codegenClassScope);
        }

        public CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var resultExpression =
                _getters[0].UnderlyingGetCodegen(underlyingExpression, codegenMethodScope, codegenClassScope);
            return LocalMethod(
                HandleGetterTrailingExistsCodegen(codegenMethodScope, codegenClassScope),
                resultExpression);
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

        public object Get(EventBean eventBean)
        {
            return GetJsonProp(eventBean.Underlying);
        }

        public object GetJsonProp(object @object)
        {
            var result = _getters[0].GetJsonProp(@object);
            for (var i = 1; i < _getters.Length; i++) {
                if (!(result is IDictionary<string, object>)) {
                    return null;
                }

                var getter = _getters[i];
                result = getter.GetJsonProp(result);
            }

            return result;
        }

        public bool GetJsonExists(object @object)
        {
            var result = _getters[0].GetJsonProp(@object);
            for (var i = 1; i < _getters.Length - 1; i++) {
                if (!(result is IDictionary<string, object>)) {
                    return false;
                }

                var getter = _getters[i];
                result = getter.GetJsonProp(result);
            }

            if (!(result is IDictionary<string, object>)) {
                return false;
            }

            return _getters[^1].GetJsonExists(result);
        }

        private CodegenMethod HandleGetterTrailingChainCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var method = codegenMethodScope
                .MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam<object>("result");

            for (var i = 1; i < _getters.Length; i++) {
                var getter = _getters[i];
                method.Block
                    .IfRefNullReturnNull("result")
                    .IfNotInstanceOf("result", typeof(IDictionary<string, object>))
                    .BlockReturn(ConstantNull())
                    .AssignRef(
                        "result",
                        getter.UnderlyingGetCodegen(
                            CastRef(typeof(IDictionary<string, object>), "result"),
                            codegenMethodScope,
                            codegenClassScope));
            }

            method.Block.MethodReturn(Ref("result"));
            return method;
        }

        private CodegenMethod HandleGetterTrailingExistsCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var method = codegenMethodScope
                .MakeChild(typeof(bool), GetType(), codegenClassScope)
                .AddParam<object>("result");

            for (var i = 1; i < _getters.Length - 1; i++) {
                var getter = _getters[i];
                method.Block
                    .IfRefNull("result")
                    .BlockReturn(ConstantFalse())
                    .IfNotInstanceOf("result", typeof(IDictionary<string, object>))
                    .BlockReturn(ConstantFalse())
                    .AssignRef(
                        "result",
                        getter.UnderlyingGetCodegen(
                            CastRef(typeof(IDictionary<string, object>), "result"),
                            codegenMethodScope,
                            codegenClassScope));
            }

            method.Block
                .IfRefNull("result")
                .BlockReturn(ConstantFalse())
                .IfNotInstanceOf("result", typeof(IDictionary<string, object>))
                .BlockReturn(ConstantFalse())
                .MethodReturn(
                    _getters[^1]
                        .UnderlyingExistsCodegen(
                            CastRef(typeof(IDictionary<string, object>), "result"),
                            codegenMethodScope,
                            codegenClassScope));
            return method;
        }
    }
} // end of namespace