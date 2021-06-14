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
    /// Widerner that coerces to a widened boxed number.
    /// </summary>
    public class TypeWidenerBoxedNumeric : TypeWidenerSPI
    {
        private readonly Coercer _coercer;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="coercer">the coercer</param>
        public TypeWidenerBoxedNumeric(Coercer coercer)
        {
            _coercer = coercer;
        }

        public Type WidenResultType {
            get => _coercer.ReturnType;
        }

        public object Widen(object input)
        {
            return _coercer.CoerceBoxed(input);
        }

        public CodegenExpression WidenCodegen(
            CodegenExpression expression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return _coercer.CoerceCodegen(expression, typeof(object));
        }
    }
} // end of namespace