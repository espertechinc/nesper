///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.epl.agg.aggregator
{
    /// <summary>
    /// Aggregator for count-ever value.
    /// </summary>
    public class AggregatorCountEver : AggregationMethod
    {
        private long _count;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        public AggregatorCountEver()
        {
        }
    
        public void Clear()
        {
            _count = 0;
        }
    
        public virtual void Enter(object @object)
        {
            _count++;
        }
    
        public virtual void Leave(object @object)
        {
        }

        public virtual object Value
        {
            get { return _count; }
        }

        public virtual Type ValueType
        {
            get { return typeof (long); }
        }

        public virtual long Count
        {
            get { return _count; }
            set { this._count = value; }
        }
    }
}
