///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.util
{
	/// <summary>
	/// Interface for number coercion resulting in BigInteger.
	/// </summary>
	public interface SimpleNumberDecimalCoercer {
	    /// <summary>
	    /// Widen the number to decimal, if widening is required.
	    /// </summary>
	    /// <param name="numToCoerce">number to widen</param>
	    /// <returns>widened number</returns>
	    decimal? CoerceBoxedDecimal(object numToCoerce);

	    CodegenExpression CoerceBoxedDecimalCodegen(CodegenExpression expr, Type type);
	}
} // end of namespace