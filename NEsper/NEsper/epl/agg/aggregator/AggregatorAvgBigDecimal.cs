///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using java.math;

namespace com.espertech.esper.epl.agg.aggregator
{
    /// <summary>Average that generates a BigDecimal numbers.</summary>
    public class AggregatorAvgBigDecimal : AggregationMethod {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        protected BigDecimal sum;
        protected long numDataPoints;
        protected MathContext optionalMathContext;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="optionalMathContext">math context</param>
        public AggregatorAvgBigDecimal(MathContext optionalMathContext) {
            sum = new BigDecimal(0.0);
            this.optionalMathContext = optionalMathContext;
        }
    
        public void Clear() {
            sum = new BigDecimal(0.0);
            numDataPoints = 0;
        }
    
        public void Enter(Object @object) {
            if (@object == null) {
                return;
            }
            numDataPoints++;
            if (object is BigInteger) {
                sum = sum.Add(new BigDecimal((BigInteger) object));
                return;
            }
            sum = sum.Add((BigDecimal) object);
        }
    
        public void Leave(Object @object) {
            if (@object == null) {
                return;
            }
    
            if (numDataPoints <= 1) {
                Clear();
            } else {
                numDataPoints--;
                if (object is BigInteger) {
                    sum = sum.Subtract(new BigDecimal((BigInteger) object));
                } else {
                    sum = sum.Subtract((BigDecimal) object);
                }
            }
        }
    
        public Object GetValue() {
            if (numDataPoints == 0) {
                return null;
            }
            try {
                if (optionalMathContext == null) {
                    return Sum.Divide(new BigDecimal(numDataPoints));
                }
                return Sum.Divide(new BigDecimal(numDataPoints), optionalMathContext);
            } catch (ArithmeticException ex) {
                Log.Error("Error computing avg aggregation result: " + ex.Message, ex);
                return new BigDecimal(0);
            }
        }
    
    }
} // end of namespace
