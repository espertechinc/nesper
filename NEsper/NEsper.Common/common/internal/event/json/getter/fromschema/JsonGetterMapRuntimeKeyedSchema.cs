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
    public sealed class JsonGetterMapRuntimeKeyedSchema : EventPropertyGetterMappedSPI
    {
        private readonly JsonUnderlyingField _field;

        public JsonGetterMapRuntimeKeyedSchema(JsonUnderlyingField field)
        {
            this._field = field;
        }

        public CodegenExpression EventBeanGetMappedCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope,
            CodegenExpression beanExpression,
            CodegenExpression key)
        {
            return StaticMethod(
                typeof(JsonFieldGetterHelperSchema),
                "GetJsonMappedProp",
                ExprDotName(beanExpression, "Underlying"),
                Constant(_field.FieldName),
                key);
        }

        public object Get(
            EventBean eventBean,
            string mapKey)
        {
            return JsonFieldGetterHelperSchema.GetJsonMappedProp(eventBean.Underlying, _field.FieldName, mapKey);
        }
    }
} // end of namespace