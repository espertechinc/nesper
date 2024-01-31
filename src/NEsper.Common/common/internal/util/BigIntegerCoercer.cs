///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Numerics;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

namespace com.espertech.esper.common.@internal.util
{
    /// <summary>
    ///     Interface for number coercion resulting in BigInteger.
    /// </summary>
    public interface BigIntegerCoercer
    {
        /// <summary>
        ///     Widen the number to BigInteger, if widening is required.
        /// </summary>
        /// <param name="numToCoerce">number to widen</param>
        /// <returns>widened number</returns>
        BigInteger CoerceBoxedBigInt(object numToCoerce);

        CodegenExpression CoerceBoxedBigIntCodegen(
            CodegenExpression expr,
            Type type, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope);
    }
} // end of namespace