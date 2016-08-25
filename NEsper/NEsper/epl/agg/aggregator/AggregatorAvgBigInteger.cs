///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Numerics;

using com.espertech.esper.compat;

namespace com.espertech.esper.epl.agg.aggregator
{
    /// <summary>
    /// Average that generates double-typed numbers.
    /// </summary>
    public class AggregatorAvgBigInteger : AggregationMethod
    {
        protected BigInteger Sum;
        protected long NumDataPoints;

        public virtual void Clear()
        {
            Sum = 0;
            NumDataPoints = 0;
        }

        public virtual void Enter(Object @object)
        {
            if (@object == null) {
                return;
            }
            NumDataPoints++;
            Sum += (@object).AsBigInteger();
        }

        public virtual void Leave(Object @object)
        {
            if (@object == null) {
                return;
            }
            if (NumDataPoints <= 1) {
                Clear();
            }
            else {
                NumDataPoints--;
                Sum -= (@object).AsBigInteger();
            }
        }

        public virtual object Value
        {
            get
            {
                if (NumDataPoints == 0)
                {
                    return null;
                }
                return Sum / NumDataPoints;
            }
        }

        public virtual Type ValueType
        {
            get { return typeof (BigInteger?); }
        }
    }
}
