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
    /// Aggregator for the very first value.
    /// </summary>
    public class AggregatorFirstEver : AggregationMethod
    {
        private readonly Type _type;

        /// <summary>Ctor. </summary>
        /// <param name="type">type of value returned</param>
        public AggregatorFirstEver(Type type) {
            _type = type;
        }
    
        public virtual void Clear()
        {
            FirstValue = null;
            IsSet = false;
        }
    
        public virtual void Enter(Object @object)
        {
            if (!IsSet)
            {
                IsSet = true;
                FirstValue = @object;
            }
        }
    
        public virtual void Leave(Object @object)
        {
        }

        public object Value
        {
            get { return FirstValue; }
        }

        public Type ValueType
        {
            get { return _type; }
        }

        public bool IsSet { get; set; }

        public object FirstValue { get; set; }
    }
}
