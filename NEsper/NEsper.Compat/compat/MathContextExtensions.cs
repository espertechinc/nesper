///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.compat.logging;

namespace com.espertech.esper.compat
{
    public static class MathContextExtensions
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        public static decimal? GetValueDivide(
            this MathContext optionalMathContext,
            decimal numerator,
            long denominator)
        {
            if (denominator == 0) {
                return null;
            }

            try {
                if (optionalMathContext == null) {
                    return numerator / denominator;
                }

                return optionalMathContext.Apply(decimal.Divide(numerator, denominator));
            }
            catch (ArithmeticException ex) {
                Log.Error("Error computing avg aggregation result: " + ex.Message, ex);
                return 0.0m;
            }
        }
        
        public static decimal? GetValueDivide(
            this MathContext optionalMathContext,
            decimal numerator,
            decimal denominator)
        {
            if (denominator == 0) {
                return null;
            }

            try {
                if (optionalMathContext == null) {
                    return numerator / denominator;
                }

                return optionalMathContext.Apply(decimal.Divide(numerator, denominator));
            }
            catch (ArithmeticException ex) {
                Log.Error("Error computing avg aggregation result: " + ex.Message, ex);
                return 0.0m;
            }
        }

    }
}