///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.io;

using static com.espertech.esper.common.@internal.epl.agg.core.AggregationSerdeUtil;

namespace com.espertech.esper.common.@internal.epl.agg.method.rate
{
    public class AggregatorRateEverSerde
    {
        public const short SERDE_VERSION = 1;

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="output">out</param>
        /// <param name="points">points</param>
        /// <throws>IOException io error</throws>
        public static void WritePoints(
            DataOutput output,
            Deque<long?> points)
        {
            WriteVersion(SERDE_VERSION, output);
            output.WriteInt(points.Count);
            foreach (long value in points) {
                output.WriteLong(value);
            }
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="input">input</param>
        /// <returns>points</returns>
        /// <throws>IOException io error</throws>
        public static Deque<long?> ReadPoints(DataInput input)
        {
            ReadVersionChecked(SERDE_VERSION, input);
            var points = new ArrayDeque<long?>();
            var size = input.ReadInt();
            for (var i = 0; i < size; i++) {
                points.Add(input.ReadLong());
            }

            return points;
        }
    }
} // end of namespace