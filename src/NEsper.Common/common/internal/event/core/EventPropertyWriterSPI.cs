///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

namespace com.espertech.esper.common.@internal.@event.core
{
    /// <summary>
    ///     Writes a single property value to an event.
    /// </summary>
    public interface EventPropertyWriterSPI : EventPropertyWriter
    {
        CodegenExpression WriteCodegen(
            CodegenExpression assigned,
            CodegenExpression underlying,
            CodegenExpression target,
            CodegenMethodScope parent,
            CodegenClassScope classScope);
    }

    public class ProxyEventPropertyWriterSPI : EventPropertyWriterSPI
    {
        public Action<object, EventBean> ProcWrite;

        public Func<
            CodegenExpression,
            CodegenExpression,
            CodegenExpression,
            CodegenMethodScope,
            CodegenClassScope,
            CodegenExpression> ProcWriteCodegen;

        public void Write(
            object value,
            EventBean target)
        {
            ProcWrite(value, target);
        }

        public CodegenExpression WriteCodegen(
            CodegenExpression assigned,
            CodegenExpression underlying,
            CodegenExpression target,
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            return ProcWriteCodegen(
                assigned,
                underlying,
                target,
                parent,
                classScope);
        }
    }
} // end of namespace