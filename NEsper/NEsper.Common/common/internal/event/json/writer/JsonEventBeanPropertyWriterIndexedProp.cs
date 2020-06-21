///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.json.compiletime;
using com.espertech.esper.common.@internal.@event.json.parser.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.json.writer
{
    public class JsonEventBeanPropertyWriterIndexedProp : JsonEventBeanPropertyWriter
    {
        private readonly int index;

        public JsonEventBeanPropertyWriterIndexedProp(
            JsonDelegateFactory delegateFactory,
            JsonUnderlyingField propertyName,
            int index) : base(delegateFactory, propertyName)
        {
            this.index = index;
        }

        public override void Write(
            object value,
            object und)
        {
            JsonWriteArrayProp(value, delegateFactory.GetValue(field.PropertyNumber, und), index);
        }

        public override CodegenExpression WriteCodegen(
            CodegenExpression assigned,
            CodegenExpression und,
            CodegenExpression target,
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            return StaticMethod(GetType(), "jsonWriteArrayProp", assigned, ExprDotName(und, field.FieldName), Constant(index));
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="arrayEntry">array</param>
        /// <param name="index">index</param>
        public static void JsonWriteArrayProp(
            object value,
            object arrayEntry,
            int index)
        {
            if (arrayEntry is Array array &&
                array.Length > index) {
                array.SetValue(arrayEntry, index);
            }
        }
    }
} // end of namespace