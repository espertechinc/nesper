///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.json.getter.provided
{
    /// <summary>
    /// Property getter for Json underlying fields.
    /// </summary>
    public sealed class JsonGetterMapRuntimeKeyedProvided : EventPropertyGetterMappedSPI
    {
        private readonly FieldInfo _field;

        public JsonGetterMapRuntimeKeyedProvided(FieldInfo field)
        {
            _field = field;
        }

        public CodegenExpression EventBeanGetMappedCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope,
            CodegenExpression beanExpression,
            CodegenExpression key)
        {
            return StaticMethod(
                typeof(CollectionUtil),
                "GetMapValueChecked",
                ExprDotName(CastUnderlying(_field.DeclaringType, beanExpression), _field.Name),
                key);
        }

        public object Get(
            EventBean eventBean,
            string mapKey)
        {
            return JsonFieldGetterHelperProvided.GetJsonProvidedMappedProp(eventBean.Underlying, _field, mapKey);
        }
    }
} // end of namespace