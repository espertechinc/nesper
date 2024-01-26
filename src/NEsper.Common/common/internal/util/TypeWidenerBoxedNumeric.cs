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
        private readonly Type _fromType;
        private readonly Coercer _coercer;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="fromType"></param>
        /// <param name="coercer">the coercer</param>
        public TypeWidenerBoxedNumeric(
            Type fromType,
            Coercer coercer)
        {
            _fromType = fromType;
            _coercer = coercer;
        }

        public Type WidenResultType => _coercer.ReturnType;

        public object Widen(object input)
        {
            return _coercer.CoerceBoxed(input);
        }

        public CodegenExpression WidenCodegen(
            CodegenExpression expression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return _coercer.CoerceCodegen(expression, _fromType, codegenMethodScope, codegenClassScope);
        }
    }
} // end of namespace