///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.common.@internal.epl.approx.countminsketch;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.epl.agg.access.countminsketch
{
	public class AggregationStateSerdeCountMinSketch {

	    /// <summary>
	    /// NOTE: Code-generation-invoked method, method name and parameter order matters
	    /// </summary>
	    /// <param name="output">out</param>
	    /// <param name="state">state</param>
	    /// <throws>IOException when there is a write exception</throws>
	    public static void WriteCountMinSketch(DataOutput output, CountMinSketchAggState state) {
	        WriteState(output, state.State);
	    }

	    /// <summary>
	    /// NOTE: Code-generation-invoked method, method name and parameter order matters
	    /// </summary>
	    /// <param name="input">in</param>
	    /// <param name="spec">spec</param>
	    /// <returns>state</returns>
	    /// <throws>IOException when there is a read exception</throws>
	    public static CountMinSketchAggState ReadCountMinSketch(DataInput input, CountMinSketchSpec spec) {
	        CountMinSketchAggState state = spec.MakeAggState();
	        ReadState(input, state.State);
	        return state;
	    }

	    private static void WriteState(DataOutput output, CountMinSketchState state) {
	        CountMinSketchStateHashes hashes = state.Hashes;
	        output.WriteInt(hashes.Depth);
	        output.WriteInt(hashes.Width);

	        long[][] table = hashes.Table;
	        output.WriteInt(table.Length);
	        foreach (long[] row in table) {
	            output.WriteInt(row.Length);
	            foreach (long col in row) {
	                output.WriteLong(col);
	            }
	        }

	        long[] hash = hashes.Hash;
	        output.WriteInt(hash.Length);
	        foreach (long aHash in hash) {
	            output.WriteLong(aHash);
	        }

	        output.WriteLong(hashes.Total);

	        CountMinSketchStateTopk topk = state.Topk;
	        output.WriteBoolean(topk != null);
	        if (topk != null) {
	            output.WriteInt(topk.TopkMax);
	            OrderedDictionary<long, object> topMap = topk.Topk;
	            output.WriteInt(topMap.Count);
	            foreach (KeyValuePair<long?, object> entry in topMap) {
	                output.WriteLong(entry.Key);
	                if (entry.Value is ByteBuffer) {
	                    output.WriteInt(1);
	                    WriteBytes(output, (ByteBuffer) entry.Value);
	                } else {
	                    Deque<ByteBuffer> q = (Deque<ByteBuffer>) entry.Value;
	                    output.WriteInt(q.Count);
	                    foreach (ByteBuffer buf in q) {
	                        WriteBytes(output, buf);
	                    }
	                }
	            }
	        }
	    }

	    private static void ReadState(DataInput input, CountMinSketchState state) {
	        int depth = input.ReadInt();
	        int width = input.ReadInt();

	        int rowsTable = input.ReadInt();
	        long[][] table = new long[rowsTable][];
	        for (int i = 0; i < rowsTable; i++) {
	            int colsRows = input.ReadInt();
	            table[i] = new long[colsRows];
	            for (int j = 0; j < colsRows; j++) {
	                table[i][j] = input.ReadLong();
	            }
	        }

	        int rowsHash = input.ReadInt();
	        long[] hash = new long[rowsHash];
	        for (int i = 0; i < rowsTable; i++) {
	            hash[i] = input.ReadLong();
	        }

	        long total = input.ReadLong();
	        state.Hashes = new CountMinSketchStateHashes(depth, width, table, hash, total);

	        bool hasTopk = input.ReadBoolean();
	        state.Topk = null;
	        if (hasTopk) {
	            int topkMax = input.ReadInt();

	            OrderedDictionary<long, object> topMap = new OrderedDictionary<long, object>(Collections.ReverseOrder());
	            IDictionary<ByteBuffer, long?> refMap = new Dictionary<ByteBuffer, long?>();
	            int numRows = input.ReadInt();
	            for (int i = 0; i < numRows; i++) {
	                long freq = input.ReadLong();
	                int numEntries = input.ReadInt();
	                if (numEntries == 1) {
	                    ByteBuffer buf = ReadBytes(input);
	                    topMap.Put(freq, buf);
	                    refMap.Put(buf, freq);
	                } else {
	                    Deque<ByteBuffer> q = new ArrayDeque<ByteBuffer>(numEntries);
	                    for (int j = 0; j < numEntries; j++) {
	                        ByteBuffer buf = ReadBytes(input);
	                        q.Add(buf);
	                        refMap.Put(buf, freq);
	                    }
	                    topMap.Put(freq, q);
	                }
	            }
	            state.Topk = new CountMinSketchStateTopk(topkMax, topMap, refMap);
	        }
	    }

	    private static void WriteBytes(DataOutput output, ByteBuffer value) {
	        byte[] bytes = value.Array();
	        output.WriteInt(bytes.Length);
	        output.Write(bytes);
	    }

	    private static ByteBuffer ReadBytes(DataInput input) {
	        int byteSize = input.ReadInt();
	        byte[] bytes = new byte[byteSize];
	        input.ReadFully(bytes);
	        return ByteBuffer.Wrap(bytes);
	    }
	}
} // end of namespace