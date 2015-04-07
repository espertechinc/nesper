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
    /// For testing if a remove stream entry has been present.
    /// </summary>
    public class AggregatorLeaving : AggregationMethod
    {
        private bool _leaving = false;

        public virtual void Enter(Object value)
        {
        }
    
        public virtual void Leave(Object value) {
            _leaving = true;
        }

        public virtual Type ValueType
        {
            get { return typeof (bool?); }
        }

        public virtual object Value
        {
            get { return _leaving; }
        }

        public virtual void Clear() {
            _leaving = false;
        }
    }
}
