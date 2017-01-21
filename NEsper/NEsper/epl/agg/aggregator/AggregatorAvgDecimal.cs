///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.epl.agg.aggregator
{
    /// <summary>
    /// Average that generates a BigDecimal numbers.
    /// </summary>
    public class AggregatorAvgDecimal : AggregationMethod
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private decimal _sum;
        private long _numDataPoints;
        private MathContext _optionalMathContext;
    
        /// <summary>Ctor. </summary>
        public AggregatorAvgDecimal(MathContext optionalMathContext)
        {
            _sum = 0.0m;
            _optionalMathContext = optionalMathContext;
        }

        public virtual void Clear()
        {
            _sum = 0.0m;
            _numDataPoints = 0;
        }
    
        public virtual void Enter(Object @object)
        {
            if (@object == null)
            {
                return;
            }
            _numDataPoints++;
            _sum += @object.AsDecimal();
        }

        public virtual void Leave(Object @object)
        {
            if (@object == null)
            {
                return;
            }
            _numDataPoints--;
            _sum -= @object.AsDecimal();
        }

        public virtual object Value
        {
            get
            {
                if (_numDataPoints == 0)
                {
                    return null;
                }
                try
                {
                    if (_optionalMathContext == null)
                    {
                        return _sum/_numDataPoints;
                    }
                    else
                    {
                        return Math.Round(
                            _sum/_numDataPoints, 
                            _optionalMathContext.Precision, 
                            _optionalMathContext.RoundingMode);
                    }
                }
                catch (ArithmeticException ex)
                {
                    Log.Error("Error computing avg aggregation result: " + ex.Message, ex);
                    return 0.0m;
                }
            }
        }
    }
}
