///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.parse
{
    /// <summary>
    /// Helper for walking a pattern match-until clause.
    /// </summary>
    public class ASTMatchUntilHelper
    {
        const string NumericMessage = "Match-until bounds expect a numeric or expression value";

        /// <summary>
        /// Validate.
        /// </summary>
        /// <param name="lowerBounds">is the lower bounds, or null if none supplied</param>
        /// <param name="upperBounds">is the upper bounds, or null if none supplied</param>
        /// <param name="isAllowLowerZero">true to allow zero value for lower range</param>
        /// <returns>
        /// true if closed range of constants and the constants are the same value
        /// </returns>
        /// <throws>ASTWalkException if the AST is incorrect</throws>
        public static bool Validate(ExprNode lowerBounds, ExprNode upperBounds, bool isAllowLowerZero)
        {
            var isConstants = true;

            Object constantLower = null;
            if (ExprNodeUtility.IsConstantValueExpr(lowerBounds)) {
                constantLower = lowerBounds.ExprEvaluator.Evaluate(new EvaluateParams(null, true, null));
                if (constantLower == null || !(constantLower.IsNumber())) 
                {
                    throw ASTWalkException.From(NumericMessage);
                }
            }
            else {
                isConstants = lowerBounds == null;
            }
    
            Object constantUpper = null;
            if (ExprNodeUtility.IsConstantValueExpr(upperBounds)) {
                constantUpper = upperBounds.ExprEvaluator.Evaluate(new EvaluateParams(null, true, null));
                if (constantUpper == null || !(constantUpper.IsNumber()))
                {
                    throw ASTWalkException.From(NumericMessage);
                }
            }
            else {
                isConstants = isConstants && upperBounds == null;
            }
    
            if (!isConstants) {
                return true;
            }
    
            if (constantLower != null && constantUpper != null) {
                int lower = constantLower.AsInt();
                int upper = constantUpper.AsInt();
                if (lower > upper) {
                    throw ASTWalkException.From(
                        "Incorrect range specification, lower bounds value '" + lower +
                        "' is higher then higher bounds '" + upper + "'");
                }
            }

            VerifyConstant(constantLower, isAllowLowerZero);
            VerifyConstant(constantUpper, false);
    
            return constantLower != null && constantUpper != null && constantLower.Equals(constantUpper);
        }
    
        private static void VerifyConstant(Object value, bool isAllowZero) {
            if (value != null) {
                int bound = value.AsInt();
                if (isAllowZero) {
                    if (bound < 0) {
                        throw ASTWalkException.From("Incorrect range specification, a bounds value of negative value is not allowed");
                    }
                }
                else {
                    if (bound <= 0) {
                        throw ASTWalkException.From("Incorrect range specification, a bounds value of zero or negative value is not allowed");
                    }
                }
            }
        }
    }
}
