///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.serde;
using com.espertech.esper.compat.io;

using static com.espertech.esper.common.@internal.epl.agg.core.AggregationSerdeUtil;

namespace com.espertech.esper.common.@internal.epl.agg.method.nth
{
    public class AggregatorNthSerde
    {
        public const short SERDE_VERSION = 1;

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="output">output</param>
        /// <param name="unitKey">unit key</param>
        /// <param name="writer">writer</param>
        /// <param name="serdeNullable">binding</param>
        /// <param name="circularBuffer">buffer</param>
        /// <param name="numDataPoints">points</param>
        /// <param name="currentBufferElementPointer">pointer</param>
        /// <param name="sizeBuf">size</param>
        /// <throws>IOException io error</throws>
        public static void Write(
            DataOutput output,
            byte[] unitKey,
            EventBeanCollatedWriter writer,
            DataInputOutputSerde serdeNullable,
            object[] circularBuffer,
            long numDataPoints,
            int currentBufferElementPointer,
            int sizeBuf)
        {
            WriteVersion(SERDE_VERSION, output);
            output.WriteBoolean(circularBuffer != null);
            if (circularBuffer != null) {
                output.WriteLong(numDataPoints);
                output.WriteInt(currentBufferElementPointer);
                for (var i = 0; i < sizeBuf; i++) {
                    serdeNullable.Write(circularBuffer[i], output, unitKey, writer);
                }
            }
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="input">input</param>
        /// <param name="unitKey">unit key</param>
        /// <param name="serdeNullable">binding</param>
        /// <param name="sizeBuf">size</param>
        /// <returns>state</returns>
        /// <throws>IOException ioerror</throws>
        public static AggregationNthState Read(
            DataInput input,
            byte[] unitKey,
            DataInputOutputSerde serdeNullable,
            int sizeBuf)
        {
            ReadVersionChecked(SERDE_VERSION, input);
            var filled = input.ReadBoolean();
            var state = new AggregationNthState();
            if (!filled) {
                return state;
            }

            var circularBuffer = new object[sizeBuf];
            state.CircularBuffer = circularBuffer;
            state.NumDataPoints = input.ReadLong();
            state.CurrentBufferElementPointer = input.ReadInt();
            for (var i = 0; i < sizeBuf; i++) {
                circularBuffer[i] = serdeNullable.Read(input, unitKey);
            }

            return state;
        }
    }
} // end of namespace