///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.json.compiletime;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.json.writer
{
    public class JsonEventBeanPropertyWriterIndexedProp : JsonEventBeanPropertyWriter
    {
        private readonly int _index;

        public JsonEventBeanPropertyWriterIndexedProp(
            IJsonDelegate @delegate,
            JsonUnderlyingField propertyName,
            int index) : base(@delegate, propertyName)
        {
            _index = index;
        }

        public override void Write(
            object value,
            object und)
        {
            if (Delegate.TryGetProperty(Field.PropertyNumber, und, out var propertyValue)) {
                JsonWriteArrayProp(value, propertyValue, _index);
            }
        }

        public override CodegenExpression WriteCodegen(
            CodegenExpression assigned,
            CodegenExpression underlying,
            CodegenExpression target,
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            return StaticMethod(
                GetType(),
                "JsonWriteArrayProp",
                assigned, // value 
                ExprDotName(underlying, Field.FieldName), // arrayEntry
                Constant(_index) // index
            );
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
                array.SetValue(value, index);
            }
        }
    }
} // end of namespace