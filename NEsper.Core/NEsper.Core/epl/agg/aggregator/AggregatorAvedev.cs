///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.collection;
using com.espertech.esper.compat;

namespace com.espertech.esper.epl.agg.aggregator
{
    /// <summary>
    /// Standard deviation always generates double-types numbers.
    /// </summary>
    public class AggregatorAvedev : AggregationMethod
    {
        /// <summary>Ctor. </summary>
        public AggregatorAvedev()
        {
            ValueSet = new RefCountedSet<Double>();
        }

        public virtual void Enter(Object @object)
        {
            if (@object == null)
            {
                return;
            }

            var value = @object.AsDouble();
            ValueSet.Add(value);
            Sum += value;
        }

        public virtual void Leave(Object @object)
        {
            if (@object == null)
            {
                return;
            }
    
            var value = @object.AsDouble();
            ValueSet.Remove(value);
            Sum -= value;
        }

        public virtual void Clear()
        {
            Sum = 0;
            ValueSet.Clear();
        }
    
        public object Value
        {
            get
            {
                int datapoints = ValueSet.Count;

                if (datapoints == 0)
                {
                    return null;
                }

                double total = 0;
                double avg = Sum/datapoints;

                foreach(var entry in ValueSet)
                {
                    total += entry.Value*Math.Abs(entry.Key - avg);
                }

                return total/datapoints;
            }
        }

        public RefCountedSet<double> ValueSet { get; set; }

        public double Sum { get; set; }
    }
}
