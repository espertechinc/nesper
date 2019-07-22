///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using Avro;
using Avro.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using NEsper.Avro.Extensions;

namespace NEsper.Avro.Writer
{
    public class AvroEventBeanPropertyWriterIndexedProp : AvroEventBeanPropertyWriter
    {
        private readonly int _indexTarget;

        public AvroEventBeanPropertyWriterIndexedProp(
            Field propertyIndex,
            int indexTarget)
            : base(propertyIndex)
        {
            _indexTarget = indexTarget;
        }

        public override void Write(
            object value,
            GenericRecord record)
        {
            AvroWriteIndexedProp(value, record, index, _indexTarget);
        }

        public override CodegenExpression WriteCodegen(
            CodegenExpression assigned,
            CodegenExpression und,
            CodegenExpression target,
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            return CodegenExpressionBuilder.StaticMethod(
                typeof(AvroEventBeanPropertyWriterIndexedProp),
                "avroWriteIndexedProp",
                assigned,
                und,
                CodegenExpressionBuilder.Constant(index),
                CodegenExpressionBuilder.Constant(_indexTarget));
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="record">record</param>
        /// <param name="index">index</param>
        /// <param name="indexTarget">index to write to</param>
        public static void AvroWriteIndexedProp(
            object value,
            GenericRecord record,
            Field index,
            int indexTarget)
        {
            var val = record.Get(index);
            if (val != null && val is IList<object>) {
                var list = (IList<object>) val;
                if (list.Count > indexTarget) {
                    list[indexTarget] = value;
                }
            }
        }
    }
} // end of namespace