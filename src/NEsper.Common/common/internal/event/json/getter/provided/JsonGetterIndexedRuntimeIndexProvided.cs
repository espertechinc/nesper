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
    public sealed class JsonGetterIndexedRuntimeIndexProvided : EventPropertyGetterIndexedSPI
    {
        private readonly FieldInfo field;

        public JsonGetterIndexedRuntimeIndexProvided(FieldInfo field)
        {
            this.field = field;
        }

        public CodegenExpression EventBeanGetIndexedCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope,
            CodegenExpression beanExpression,
            CodegenExpression key)
        {
            return StaticMethod(
                typeof(CollectionUtil),
                "ArrayValueAtIndex",
                ExprDotName(CastUnderlying(field.DeclaringType, beanExpression), field.Name),
                key);
        }

        public object Get(
            EventBean eventBean,
            int index)
        {
            return JsonFieldGetterHelperProvided.GetJsonProvidedIndexedProp(eventBean.Underlying, field, index);
        }
    }
} // end of namespace