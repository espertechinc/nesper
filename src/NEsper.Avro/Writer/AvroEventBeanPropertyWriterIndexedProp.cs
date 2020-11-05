///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Avro;
using Avro.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.magic;

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
            AvroWriteIndexedProp(value, record, index.Name, _indexTarget);
        }

        public override CodegenExpression WriteCodegen(
            CodegenExpression assigned,
            CodegenExpression underlying,
            CodegenExpression target,
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            return CodegenExpressionBuilder.StaticMethod(
                typeof(AvroEventBeanPropertyWriterIndexedProp),
                "AvroWriteIndexedProp",
                assigned,
                underlying,
                CodegenExpressionBuilder.Constant(index.Name),
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
            string index,
            int indexTarget)
        {
            var val = record.Get(index);
            if (val is Array valuesArray) {
                if (valuesArray.Length > indexTarget) {
                    valuesArray.SetValue(value, indexTarget);
                }
            }
            else if (val is IList<object> list) {
                if (list.Count > indexTarget) {
                    list[indexTarget] = value;
                }
            }
            else if (val.GetType().IsGenericList()) {
                list = MagicMarker.SingletonInstance.GetList(val);
                if (list.Count > indexTarget) {
                    list[indexTarget] = value;
                }
            }
        }
    }
} // end of namespace