///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.@internal.epl.approx.countminsketch
{
    public class CountMinSketchState
    {
        public CountMinSketchState(
            CountMinSketchStateHashes hashes,
            CountMinSketchStateTopk topk)
        {
            Hashes = hashes;
            Topk = topk;
        }

        public ICollection<ByteBuffer> TopKValues {
            get {
                if (Topk == null) {
                    return Collections.GetEmptyList<ByteBuffer>();
                }

                return Topk.TopKValues;
            }
        }

        public CountMinSketchStateHashes Hashes { get; set; }

        public CountMinSketchStateTopk Topk { get; set; }

        public static CountMinSketchState MakeState(CountMinSketchSpec spec)
        {
            var hashes = CountMinSketchStateHashes.MakeState(spec.HashesSpec);
            CountMinSketchStateTopk topk = null;
            if (spec.TopkSpec != null && spec.TopkSpec > 0) {
                topk = new CountMinSketchStateTopk(spec.TopkSpec.Value);
            }

            return new CountMinSketchState(hashes, topk);
        }

        public void Add(
            byte[] bytes,
            int count)
        {
            Hashes.Add(bytes, count);
            if (Topk != null) {
                var frequency = Hashes.EstimateCount(bytes);
                Topk.UpdateExpectIncreasing(bytes, frequency);
            }
        }

        public long Frequency(byte[] bytes)
        {
            return Hashes.EstimateCount(bytes);
        }
    }
} // end of namespace