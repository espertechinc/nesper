///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.approx.countminsketch;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.epl.agg.access.countminsketch
{
    public class AggregationStateSerdeCountMinSketch
    {
        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="output">out</param>
        /// <param name="state">state</param>
        /// <throws>IOException when there is a write exception</throws>
        public static void WriteCountMinSketch(
            DataOutput output,
            CountMinSketchAggState state)
        {
            WriteState(output, state.State);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="input">in</param>
        /// <param name="spec">spec</param>
        /// <returns>state</returns>
        /// <throws>IOException when there is a read exception</throws>
        public static CountMinSketchAggState ReadCountMinSketch(
            DataInput input,
            CountMinSketchSpec spec)
        {
            var state = spec.MakeAggState();
            ReadState(input, state.State);
            return state;
        }

        private static void WriteState(
            DataOutput output,
            CountMinSketchState state)
        {
            var hashes = state.Hashes;
            output.WriteInt(hashes.Depth);
            output.WriteInt(hashes.Width);

            var table = hashes.Table;
            output.WriteInt(table.Length);
            foreach (var row in table) {
                output.WriteInt(row.Length);
                foreach (var col in row) {
                    output.WriteLong(col);
                }
            }

            var hash = hashes.Hash;
            output.WriteInt(hash.Length);
            foreach (var aHash in hash) {
                output.WriteLong(aHash);
            }

            output.WriteLong(hashes.Total);

            var topk = state.Topk;
            output.WriteBoolean(topk != null);
            if (topk != null) {
                output.WriteInt(topk.TopKMax);
                var topMap = topk.Topk;
                output.WriteInt(topMap.Count);
                foreach (var entry in topMap) {
                    output.WriteLong(entry.Key);
                    if (entry.Value is ByteBuffer) {
                        output.WriteInt(1);
                        WriteBytes(output, (ByteBuffer) entry.Value);
                    }
                    else {
                        var q = (Deque<ByteBuffer>) entry.Value;
                        output.WriteInt(q.Count);
                        foreach (var buf in q) {
                            WriteBytes(output, buf);
                        }
                    }
                }
            }
        }

        private static void ReadState(
            DataInput input,
            CountMinSketchState state)
        {
            var depth = input.ReadInt();
            var width = input.ReadInt();

            var rowsTable = input.ReadInt();
            var table = new long[rowsTable][];
            for (var i = 0; i < rowsTable; i++) {
                var colsRows = input.ReadInt();
                table[i] = new long[colsRows];
                for (var j = 0; j < colsRows; j++) {
                    table[i][j] = input.ReadLong();
                }
            }

            var rowsHash = input.ReadInt();
            var hash = new long[rowsHash];
            for (var i = 0; i < rowsTable; i++) {
                hash[i] = input.ReadLong();
            }

            var total = input.ReadLong();
            state.Hashes = new CountMinSketchStateHashes(depth, width, table, hash, total);

            var hasTopk = input.ReadBoolean();
            state.Topk = null;
            if (hasTopk) {
                var topkMax = input.ReadInt();

                var topMap = new OrderedListDictionary<long, object>(
                    Comparers.Default<long>().Inverse());
                var refMap = new Dictionary<ByteBuffer, long>();
                var numRows = input.ReadInt();
                for (var i = 0; i < numRows; i++) {
                    var freq = input.ReadLong();
                    var numEntries = input.ReadInt();
                    if (numEntries == 1) {
                        var buf = ReadBytes(input);
                        topMap.Put(freq, buf);
                        refMap.Put(buf, freq);
                    }
                    else {
                        Deque<ByteBuffer> q = new ArrayDeque<ByteBuffer>(numEntries);
                        for (var j = 0; j < numEntries; j++) {
                            var buf = ReadBytes(input);
                            q.AddLast(buf);
                            refMap.Put(buf, freq);
                        }

                        topMap.Put(freq, q);
                    }
                }

                state.Topk = new CountMinSketchStateTopk(topkMax, topMap, refMap);
            }
        }

        private static void WriteBytes(
            DataOutput output,
            ByteBuffer value)
        {
            var bytes = value.Array;
            output.WriteInt(bytes.Length);
            output.Write(bytes);
        }

        private static ByteBuffer ReadBytes(DataInput input)
        {
            var byteSize = input.ReadInt();
            var bytes = new byte[byteSize];
            input.ReadFully(bytes);
            return new ByteBuffer(bytes);
        }
    }
} // end of namespace