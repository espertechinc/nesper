///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.approx
{
    /// <summary>
    /// <para>
    /// Count-min sketch (or CM sketch) is a probabilistic sub-linear space streaming algorithm
    /// (source: Wikipedia, see http://en.wikipedia.org/wiki/Count%E2%80%93min_sketch)
    /// </para>
    /// <para>
    /// Count-min sketch computes an approximate frequency and thereby top-k or heavy-hitters.
    /// </para>
    /// <para>
    /// Paper:
    /// Graham Cormode and S. Muthukrishnan. An improved data stream summary:
    /// The Count-Min sketch and its applications. 2004. 10.1016/j.jalgor.2003.12.001
    /// http://dl.acm.org/citation.cfm?id=1073718
    /// </para>
    /// </summary>
    public class CountMinSketchStateHashes {
    
        private int depth;
        private int width;
        private long[][] table;
        private long[] hash;
        private long total;
    
        public CountMinSketchStateHashes(int depth, int width, long[][] table, long[] hash, long total) {
            this.depth = depth;
            this.width = width;
            this.table = table;
            this.hash = hash;
            this.total = total;
        }
    
        public static CountMinSketchStateHashes MakeState(CountMinSketchSpecHashes spec) {
            int width = (int) Math.Ceil(2 / spec.EpsOfTotalCount);
            int depth = (int) Math.Ceil(-Math.Log(1 - spec.Confidence) / Math.Log(2));
            var table = new long[depth][width];
            var hash = new long[depth];
            var r = new Random(spec.Seed);
            for (int i = 0; i < depth; ++i) {
                hash[i] = r.NextInt(Int32.MaxValue);
            }
            return new CountMinSketchStateHashes(depth, width, table, hash, 0);
        }
    
        public long[][] GetTable() {
            return table;
        }
    
        public long[] GetHash() {
            return hash;
        }
    
        public int GetDepth() {
            return depth;
        }
    
        public int GetWidth() {
            return width;
        }
    
        public void IncTotal(long count) {
            total += count;
        }
    
        public long GetTotal() {
            return total;
        }
    
        public long EstimateCount(byte[] item) {
            long res = long.MAX_VALUE;
            int[] buckets = GetHashBuckets(item, depth, width);
            for (int i = 0; i < depth; ++i) {
                res = Math.Min(res, table[i][buckets[i]]);
            }
            return res;
        }
    
        public void Add(byte[] item, long count) {
            if (count < 0) {
                throw new ArgumentException("Negative increments not implemented");
            }
            int[] buckets = GetHashBuckets(item, depth, width);
            for (int i = 0; i < depth; ++i) {
                table[i][buckets[i]] += count;
            }
            total += count;
        }
    
        private int[] GetHashBuckets(byte[] b, int hashCount, int max) {
            var result = new int[hashCount];
            int hash1 = MurmurHash.Hash(b, 0, b.Length, 0);
            int hash2 = MurmurHash.Hash(b, 0, b.Length, hash1);
            for (int i = 0; i < hashCount; i++) {
                result[i] = Math.Abs((hash1 + i * hash2) % max);
            }
            return result;
        }
    }
} // end of namespace
