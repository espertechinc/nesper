///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.ops
{
    /// <summary>Represents the between-clause function in an expression tree. </summary>
    public interface ExprBetweenNode : ExprNode, ExprEvaluator
    {
        /// <summary>Returns true if the low endpoint is included, false if not </summary>
        /// <value>indicator if endppoint is included</value>
        bool IsLowEndpointIncluded { get; }

        /// <summary>Returns true if the high endpoint is included, false if not </summary>
        /// <value>indicator if endppoint is included</value>
        bool IsHighEndpointIncluded { get; }

        /// <summary>Returns true for inverted range, or false for regular (openn/close/half-open/half-closed) ranges. </summary>
        /// <value>true for not betwene, false for between</value>
        bool IsNotBetween { get; }
    }
}
