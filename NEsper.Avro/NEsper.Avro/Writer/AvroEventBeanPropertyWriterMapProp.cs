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
using com.espertech.esper.compat.collections;

using NEsper.Avro.Extensions;

namespace NEsper.Avro.Writer
{
    public class AvroEventBeanPropertyWriterMapProp : AvroEventBeanPropertyWriter
    {
        private readonly string _key;

        public AvroEventBeanPropertyWriterMapProp(
            Field propertyIndex,
            string key)
            : base(propertyIndex)
        {
            _key = key;
        }

        public override void Write(
            object value,
            GenericRecord record)
        {
            AvroWriteMapProp(value, record, _key, index.Name);
        }

        public override CodegenExpression WriteCodegen(
            CodegenExpression assigned,
            CodegenExpression und,
            CodegenExpression target,
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            return CodegenExpressionBuilder.StaticMethod(
                typeof(AvroEventBeanPropertyWriterMapProp),
                "AvroWriteMapProp",
                assigned,
                und,
                CodegenExpressionBuilder.Constant(_key),
                CodegenExpressionBuilder.Constant(index.Name));
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="record">record</param>
        /// <param name="key">key</param>
        /// <param name="index">index</param>
        public static void AvroWriteMapProp(
            object value,
            GenericRecord record,
            string key,
            string index)
        {
            object val = record.Get(index);
            if (val != null && val is IDictionary<string, object> map) {
                map.Put(key, value);
            }
        }
    }
} // end of namespace