///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.json.compiletime;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.json.getter.fromschema
{
    /// <summary>
    ///     Property getter for Json underlying fields.
    /// </summary>
    public sealed class JsonGetterIndexedRuntimeIndexSchema : EventPropertyGetterIndexedSPI
    {
        private readonly JsonUnderlyingField _field;

        public JsonGetterIndexedRuntimeIndexSchema(JsonUnderlyingField field)
        {
            _field = field;
        }

        public CodegenExpression EventBeanGetIndexedCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope,
            CodegenExpression beanExpression,
            CodegenExpression key)
        {
            return StaticMethod(
                typeof(JsonFieldGetterHelperSchema),
                "GetJsonIndexedProp",
                ExprDotName(beanExpression, "Underlying"),
                Constant(_field.PropertyNumber),
                key);
        }

        public object Get(
            EventBean eventBean,
            int index)
        {
            return JsonFieldGetterHelperSchema.GetJsonIndexedProp(
                eventBean.Underlying,
                _field.PropertyNumber,
                index);
        }
    }
} // end of namespace