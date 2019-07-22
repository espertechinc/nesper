///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.util
{
    /// <summary>
    /// Type widener that coerces from string to char if required.
    /// </summary>
    public class TypeWidenerObjectArrayToCollectionCoercer : TypeWidenerSPI
    {
        /// <summary>
        /// Widen input value.
        /// </summary>
        /// <param name="input">the object to widen.</param>
        /// <returns>
        /// widened object.
        /// </returns>
        public object Widen(object input)
        {
            return WidenInput(input);
        }

        /// <summary>
        /// Generates code to widen an input.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="codegenMethodScope">The codegen method scope.</param>
        /// <param name="codegenClassScope">The codegen class scope.</param>
        /// <returns></returns>
        public CodegenExpression WidenCodegen(
            CodegenExpression expression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return CodegenExpressionBuilder.StaticMethod(
                typeof(TypeWidenerObjectArrayToCollectionCoercer),
                "WidenInput",
                expression);
        }

        /// <summary>
        /// Widens the input value.
        /// </summary>
        /// <param name="input">the object to widen.</param>
        /// <returns>
        /// widened object.
        /// </returns>
        public static object WidenInput(object input)
        {
            return input.Unwrap<object>(true);
        }
    }
} // end of namespace