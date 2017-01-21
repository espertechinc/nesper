///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.util;

namespace com.espertech.esper.epl.approx
{
    /// <summary>
    /// <para />
    /// Count-min sketch (or CM sketch) is a probabilistic sub-linear space streaming algorithm
    /// (source: Wikipedia, see http://en.wikipedia.org/wiki/Count%E2%80%93min_sketch)
    /// <para />
    /// Count-min sketch computes an approximate frequency and thereby top-k or heavy-hitters.
    /// <para />
    /// Paper:
    /// Graham Cormode and S. Muthukrishnan. An improved data stream summary:
    /// The Count-Min sketch and its applications. 2004. 10.1016/j.jalgor.2003.12.001
    /// http://dl.acm.org/citation.cfm?id=1073718
    /// </summary>
    public class CountMinSketchStateHashes
    {
        private long _total;
        private long[][] _table;

        public static CountMinSketchStateHashes MakeState(CountMinSketchSpecHashes spec)
        {
            var width = (int) Math.Ceiling(2 / spec.EpsOfTotalCount);
            var depth = (int) Math.Ceiling(-Math.Log(1 - spec.Confidence) / Math.Log(2));
            var table = new long[depth][];
            for (var ii = 0; ii < depth; ii++)
                table[ii] = new long[width];

            var hash = new long[depth];
            var r = new Random(spec.Seed);
            for (var i = 0; i < depth; ++i)
            {
                hash[i] = r.Next(int.MaxValue);
            }
            return new CountMinSketchStateHashes(depth, width, table, hash, 0);
        }
    
        public CountMinSketchStateHashes(int depth, int width, long[][] table, long[] hash, long total)
        {
            this.Depth = depth;
            this.Width = width;
            this.Hash = hash;
            this._table = table;
            this._total = total;
        }

        public long[][] Table
        {
            get { return _table; }
        }

        public long[] Hash { get; private set; }

        public int Depth { get; private set; }

        public int Width { get; private set; }

        public void IncTotal(long count)
        {
            _total += count;
        }

        public long Total
        {
            get { return _total; }
        }

        public long EstimateCount(byte[] item)
        {
            var res = long.MaxValue;
            var buckets = GetHashBuckets(item, Depth, Width);
            for (var i = 0; i < Depth; ++i) {
                res = Math.Min(res, _table[i][buckets[i]]);
            }
            return res;
        }
    
        public void Add(byte[] item, long count)
        {
            if (count < 0) {
                throw new ArgumentException("Negative increments not implemented");
            }
            var buckets = GetHashBuckets(item, Depth, Width);
            for (var i = 0; i < Depth; ++i) {
                _table[i][buckets[i]] += count;
            }
            _total += count;
        }
    
        private int[] GetHashBuckets(byte[] b, int hashCount, int max)
        {
            var result = new int[hashCount];
            var hash1 = MurmurHash.Hash(b, 0, b.Length, 0);
            var hash2 = MurmurHash.Hash(b, 0, b.Length, hash1);
            for (var i = 0; i < hashCount; i++) {
                var tempMod = (int) (hash1 + i*hash2)%max;
                result[i] = Math.Abs(tempMod);
            }
            return result;
        }
    }
    
}
