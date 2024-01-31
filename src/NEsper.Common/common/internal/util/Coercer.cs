///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
    ///     Interface for coercion.
    /// </summary>
    public interface Coercer
    {
        Type GetReturnType(Type valueType);

        /// <summary>
        /// Coerce the given value to a pre-determined type.
        /// </summary>
        /// <param name="value">is the value to coerce</param>
        /// <returns>the value coerced to the given result type.</returns>
        object CoerceBoxed(object value);

        CodegenExpression CoerceCodegen(
            CodegenExpression value,
            Type valueType, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope);

        CodegenExpression CoerceCodegenMayNullBoxed(
            CodegenExpression value,
            Type valueType,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope);
    }
} // end of namespace