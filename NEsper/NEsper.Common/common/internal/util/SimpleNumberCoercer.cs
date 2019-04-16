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
    ///     Interface for number coercion.
    /// </summary>
    public interface SimpleNumberCoercer
    {
        Type ReturnType { get; }

        /// <summary>
        ///     Coerce the given number to a previously determined type, assuming the type is a Boxed type. Allows coerce to lower
        ///     resultion number.
        ///     Doesnt coerce to primitive types.
        /// </summary>
        /// <param name="numToCoerce">is the number to coerce to the given type</param>
        /// <returns>the numToCoerce as a value in the given result type</returns>
        object CoerceBoxed(object numToCoerce);

        CodegenExpression CoerceCodegen(
            CodegenExpression value,
            Type valueType);

        CodegenExpression CoerceCodegenMayNullBoxed(
            CodegenExpression value,
            Type valueTypeMustNumeric,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope);
    }
} // end of namespace