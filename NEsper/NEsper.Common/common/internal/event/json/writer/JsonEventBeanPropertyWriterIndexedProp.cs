///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.json.compiletime;
using com.espertech.esper.common.@internal.@event.json.parser.core;
using com.espertech.esper.common.@internal.@event.json.serde;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.json.writer
{
    public class JsonEventBeanPropertyWriterIndexedProp : JsonEventBeanPropertyWriter
    {
        private readonly int _index;

        public JsonEventBeanPropertyWriterIndexedProp(
            JsonUnderlyingField propertyName,
            int index) : base(propertyName)
        {
            _index = index;
        }

        public override void Write(
            object value,
            object und)
        {
            var lookup = (ILookup<string, object>) und;
            //SerializationContext.GetValue(_field.FieldName, und),
            JsonWriteArrayProp(value, lookup[Field.FieldName], _index);
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