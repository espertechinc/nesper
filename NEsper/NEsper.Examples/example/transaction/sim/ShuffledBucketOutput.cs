///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


/*
 * Created on Apr 23, 2006
 *
 */

using System;
using System.Collections.Generic;
using com.espertech.esper.compat.collections;

namespace NEsper.Examples.Transaction.sim
{
    /// <summary> </summary>
    /// <author>Hans Gilde </author>
    public class ShuffledBucketOutput
    {
        private static readonly Random Random = RandomUtil.GetNewInstance();

        private readonly EventSource _eventSource;
        private readonly OutputStream _outputStream;
        private readonly int _bucketSize;

        /// <summary />
        /// <param name="eventSource"></param>
        /// <param name="outputStream"></param>
        /// <param name="bucketSize">how many events should be in the bucket when it's shuffled?</param>
        public ShuffledBucketOutput(EventSource eventSource, OutputStream outputStream, int bucketSize)
        {
            _eventSource = eventSource;
            _outputStream = outputStream;
            _bucketSize = bucketSize;
        }

        public void Output()
        {
            List<TxnEventBase> bucket = new List<TxnEventBase>(_bucketSize);

            foreach (TxnEventBase e in _eventSource)
            {
                bucket.Add(e);
                if (bucket.Count == _bucketSize)
                {
                    OutputBucket(bucket);
                }
            }

            if (bucket.Count > 0)
            {
                OutputBucket(bucket);
            }
        }

        private void OutputBucket(List<TxnEventBase> bucket)
        {
            Collections.Shuffle(bucket, Random);
            _outputStream.Output(bucket);
            bucket.Clear();
        }
    }
}
