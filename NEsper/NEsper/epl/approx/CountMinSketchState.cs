///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.epl.approx
{
    public class CountMinSketchState
    {
        public static CountMinSketchState MakeState(CountMinSketchSpec spec)
        {
            CountMinSketchStateHashes hashes = CountMinSketchStateHashes.MakeState(spec.HashesSpec);
            CountMinSketchStateTopk topk = null;
            if (spec.TopkSpec != null && spec.TopkSpec > 0) {
                topk = new CountMinSketchStateTopk(spec.TopkSpec.Value);
            }
            return new CountMinSketchState(hashes, topk);
        }
    
        public CountMinSketchState(CountMinSketchStateHashes hashes, CountMinSketchStateTopk topk)
        {
            Hashes = hashes;
            Topk = topk;
        }
    
        public void Add(byte[] bytes, int count)
        {
            Hashes.Add(bytes, count);
            if (Topk != null)
            {
                long frequency = Hashes.EstimateCount(bytes);
                Topk.UpdateExpectIncreasing(bytes, frequency);
            }
        }
    
        public long Frequency(byte[] bytes)
        {
            return Hashes.EstimateCount(bytes);
        }

        public ICollection<Blob> TopKValues
        {
            get
            {
                if (Topk == null)
                {
                    return Collections.GetEmptyList<Blob>();
                }
                return Topk.TopKValues;
            }
        }

        public CountMinSketchStateHashes Hashes { get; set; }

        public CountMinSketchStateTopk Topk { get; set; }
    }
    
}
