///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat.logging;
using com.espertech.esper.type;

namespace com.espertech.esper.epl.agg.aggregator
{
    /// <summary>
    /// Min/max aggregator for all values, not considering events leaving the aggregation (i.e. ever).
    /// </summary>
    public class AggregatorMinMaxEver : AggregationMethod
    {
        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly MinMaxTypeEnum _minMaxTypeEnum;

        private IComparable _currentMinMax;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="minMaxTypeEnum">- enum indicating to return minimum or maximum values</param>
        public AggregatorMinMaxEver(MinMaxTypeEnum minMaxTypeEnum)
        {
            _minMaxTypeEnum = minMaxTypeEnum;
        }

        public void Clear()
        {
            _currentMinMax = null;
        }

        public virtual void Enter(Object @object)
        {
            if (@object == null)
            {
                return;
            }
            if (_currentMinMax == null)
            {
                _currentMinMax = (IComparable) @object;
                return;
            }
            if (_minMaxTypeEnum == MinMaxTypeEnum.MAX)
            {
                if (_currentMinMax.CompareTo(@object) < 0)
                {
                    _currentMinMax = (IComparable) @object;
                }
            }
            else
            {
                if (_currentMinMax.CompareTo(@object) > 0)
                {
                    _currentMinMax = (IComparable) @object;
                }
            }
        }

        public virtual void Leave(Object @object)
        {
            // no-op, this is designed to handle min-max ever
            Log.Warn(".leave Received remove stream, none was expected");
        }

        public object Value
        {
            get { return _currentMinMax; }
        }

        public IComparable CurrentMinMax
        {
            get { return _currentMinMax; }
            set { this._currentMinMax = value; }
        }
    }
} // end of namespace
