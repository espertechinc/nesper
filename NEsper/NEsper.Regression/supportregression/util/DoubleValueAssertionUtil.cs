///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

using com.espertech.esper.compat.logging;

namespace com.espertech.esper.supportregression.util
{
    /// <summary>
    /// Utility class for comparing double values up to a given precision
    /// </summary>
    public class DoubleValueAssertionUtil
    {
        public static bool Equals(double valueActual, double valueExpected, int precision)
        {
            if (precision < 1)
            {
                throw new ArgumentException("Invalid precision value of " + precision + " supplied");
            }
    
            if ((Double.IsNaN(valueActual) && Double.IsNaN(valueExpected)))
            {
                return true;
            }
            if (((Double.IsNaN(valueActual)) && (!Double.IsNaN(valueExpected))) ||
                 ((!Double.IsNaN(valueActual)) && (Double.IsNaN(valueExpected))))
            {
                Log.Debug(".equals Compare failed, " +
                        "  valueActual=" + valueActual +
                        "  valueExpected=" + valueExpected);
                return false;
            }
    
            double factor = Math.Pow(10, precision);
            double val1 = valueActual * factor;
            double val2 = valueExpected * factor;
    
            // Round to closest integer
            double d1 = Math.Round(val1);
            double d2 = Math.Round(val2);
    
            if (d1 != d2)
            {
                Log.Debug(".equals Compare failed, " +
                        "  valueActual=" + valueActual +
                        "  valueExpected=" + valueExpected +
                        "  precision=" + precision
                        );
                return false;
            }
    
            return true;
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
