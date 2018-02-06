///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.collection;
using com.espertech.esper.type;

namespace com.espertech.esper.epl.agg.aggregator
{
    /// <summary>Min/max aggregator for all values. </summary>
    public class AggregatorMinMax : AggregationMethod
    {
        private readonly MinMaxTypeEnum _minMaxTypeEnum;
    
        private readonly SortedRefCountedSet<Object> _refSet;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="minMaxTypeEnum">enum indicating to return minimum or maximum values</param>
        public AggregatorMinMax(MinMaxTypeEnum minMaxTypeEnum)
        {
            _minMaxTypeEnum = minMaxTypeEnum;
            _refSet = new SortedRefCountedSet<Object>();
        }
    
        public virtual void Clear()
        {
            _refSet.Clear();
        }

        public virtual void Enter(Object @object)
        {
            if (@object == null)
            {
                return;
            }
            _refSet.Add(@object);
        }
    
        public virtual void Leave(Object @object)
        {
            if (@object == null)
            {
                return;
            }
            _refSet.Remove(@object);
        }

        public virtual object Value
        {
            get
            {
                if (_minMaxTypeEnum == MinMaxTypeEnum.MAX)
                {
                    return _refSet.MaxValue;
                }
                else
                {
                    return _refSet.MinValue;
                }
            }
        }

        public SortedRefCountedSet<object> RefSet
        {
            get { return _refSet; }
        }
    }
}
