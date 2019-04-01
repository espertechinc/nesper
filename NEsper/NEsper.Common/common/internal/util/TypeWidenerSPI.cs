///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

namespace com.espertech.esper.common.@internal.util
{
    /// <summary>
    ///     Interface for a type widener.
    /// </summary>
    public interface TypeWidenerSPI : TypeWidener
    {
        CodegenExpression WidenCodegen(
            CodegenExpression expression,
            CodegenMethodScope codegenMethodScope, 
            CodegenClassScope codegenClassScope);
    }

    public class ProxyTypeWidenerSPI : TypeWidenerSPI
    {
        public Func<object, object> ProcWiden;
        public Func<CodegenExpression, CodegenMethodScope, CodegenClassScope, CodegenExpression> ProcWidenCodegen;

        public object Widen(object input) => ProcWiden(input);

        public CodegenExpression WidenCodegen(
            CodegenExpression expression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope) => ProcWidenCodegen(expression, codegenMethodScope, codegenClassScope);
    }
} // end of namespace