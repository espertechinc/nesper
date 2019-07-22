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
    ///     Interface for casting.
    /// </summary>
    public interface SimpleTypeCaster
    {
        /// <summary>
        ///     Casts an object to another type, typically for numeric types.
        ///     <para />
        ///     May performs a compatibility check and returns null if not compatible.
        /// </summary>
        /// <param name="object">to cast</param>
        /// <returns>casted or transformed object, possibly the same, or null if the cast cannot be made</returns>
        object Cast(object @object);

        /// <summary>
        ///     Returns true to indicate that the cast target type is numeric.
        /// </summary>
        /// <value>true for numeric cast</value>
        bool IsNumericCast { get; }

        CodegenExpression Codegen(
            CodegenExpression input,
            Type inputType,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope);
    }

    public class ProxyTypeCaster : SimpleTypeCaster
    {
        public Func<object, object> ProcCast;

        public object Cast(object @object)
            => ProcCast?.Invoke(@object);

        public bool IsNumericCast { get; set; }

        public Func<CodegenExpression, Type, CodegenMethodScope, CodegenClassScope, CodegenExpression> ProcCodegen;
        public Func<CodegenExpression, CodegenExpression> ProcCodegenInput;

        public CodegenExpression Codegen(
            CodegenExpression input,
            Type inputType,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            if (ProcCodegen != null) {
                return ProcCodegen.Invoke(input, inputType, codegenMethodScope, codegenClassScope);
            }

            if (ProcCodegenInput != null) {
                return ProcCodegenInput.Invoke(input);
            }

            throw new NotImplementedException();
        }
    }
} // end of namespace