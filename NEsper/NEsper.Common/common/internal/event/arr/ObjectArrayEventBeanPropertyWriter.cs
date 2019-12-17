///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.arr
{
    public class ObjectArrayEventBeanPropertyWriter : EventPropertyWriterSPI
    {
        internal readonly int index;

        public ObjectArrayEventBeanPropertyWriter(int index)
        {
            this.index = index;
        }

        public void Write(
            object value,
            EventBean target)
        {
            var arrayEvent = (ObjectArrayBackedEventBean) target;
            Write(value, arrayEvent.Properties);
        }

        public virtual CodegenExpression WriteCodegen(
            CodegenExpression assigned,
            CodegenExpression und,
            CodegenExpression target,
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            return Assign(ArrayAtIndex(und, Constant(index)), assigned);
        }

        public virtual void Write(
            object value,
            object[] array)
        {
            array[index] = value;
        }
    }
} // end of namespace