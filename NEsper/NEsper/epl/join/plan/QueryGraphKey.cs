///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.collection;


namespace com.espertech.esper.epl.join.plan
{
    /// <summary>
    /// Value consisting of 2 integer stream numbers, for use by <seealso cref="QueryGraph"/>.
    /// </summary>
    public class QueryGraphKey
    {
        private readonly UniformPair<int> _streams;
    
        /// <summary>Ctor. </summary>
        /// <param name="streamOne">from stream</param>
        /// <param name="streamTwo">to stream</param>
        public QueryGraphKey(int streamOne, int streamTwo)
        {
            _streams = new UniformPair<int>(streamOne, streamTwo);
        }
    
        public override bool Equals(Object obj)
        {
            if (this == obj)
            {
                return true;
            }
    
            if (!(obj is QueryGraphKey))
            {
                return false;
            }
    
            var other = (QueryGraphKey) obj;
            return other._streams.Equals(_streams);
        }
    
        public override int GetHashCode()
        {
            return _streams.GetHashCode();
        }
    
        public override String ToString()
        {
            return "QueryGraphKey " + _streams.First + " and " + _streams.Second;
        }
    }
    
}
