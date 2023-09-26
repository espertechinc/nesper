///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.collection;

namespace com.espertech.esper.common.@internal.epl.join.querygraph
{
    /// <summary>
    ///     Key consisting of 2 integer stream numbers, for use by <seealso cref="QueryGraphForge" />.
    /// </summary>
    public class QueryGraphKey
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="streamOne">from stream</param>
        /// <param name="streamTwo">to stream</param>
        public QueryGraphKey(
            int streamOne,
            int streamTwo)
        {
            Streams = new UniformPair<int>(streamOne, streamTwo);
        }

        public UniformPair<int> Streams { get; }

        public override bool Equals(object obj)
        {
            if (this == obj) {
                return true;
            }

            if (!(obj is QueryGraphKey other)) {
                return false;
            }

            return other.Streams.Equals(Streams);
        }

        public override int GetHashCode()
        {
            return Streams.GetHashCode();
        }

        public override string ToString()
        {
            return "QueryGraphKey " + Streams.First + " and " + Streams.Second;
        }
    }
} // end of namespace