///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using Avro;
using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.core;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

namespace NEsper.Avro.Writer
{
    public class AvroEventBeanPropertyWriter : EventPropertyWriterSPI
    {
        protected readonly Field index;

        public AvroEventBeanPropertyWriter(Field index)
        {
            this.index = index;
        }

        public void Write(
            object value,
            EventBean target)
        {
            AvroGenericDataBackedEventBean avroEvent = (AvroGenericDataBackedEventBean) target;
            Write(value, avroEvent.Properties);
        }

        public virtual void Write(
            object value,
            GenericRecord record)
        {
            record.Put(index, value);
        }

        public virtual CodegenExpression WriteCodegen(
            CodegenExpression assigned,
            CodegenExpression und,
            CodegenExpression target,
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            return CodegenExpressionBuilder.ExprDotMethod(und, "Put", CodegenExpressionBuilder.Constant(index), assigned);
        }
    }
} // end of namespace