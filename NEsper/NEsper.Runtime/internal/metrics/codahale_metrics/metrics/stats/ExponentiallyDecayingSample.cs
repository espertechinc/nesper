///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading.locks;
using com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.core;

namespace com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.stats
{
    public class ExponentiallyDecayingSample : Sample
    {
        private static readonly long RESCALE_THRESHOLD = TimeUnit.HOURS.ToNanos(1);

        private readonly double alpha;
        private readonly Clock clock;
        private readonly AtomicLong count = new AtomicLong(0);
        private readonly IReaderWriterLock @lock;
        private readonly AtomicLong nextScaleTime = new AtomicLong(0);
        private readonly int reservoirSize;
        //private readonly ConcurrentSkipListMap<double, long> values;
        private readonly IDictionary<double, long> values;
        private long startTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExponentiallyDecayingSample"/> class.
        /// </summary>
        /// <param name="reservoirSize">the number of samples to keep in the sampling reservoir</param>
        /// <param name="alpha">the exponential decay factor; the higher this is, the more biased the sample will be towards newer </param>
        public ExponentiallyDecayingSample(
            int reservoirSize,
            double alpha) : this(reservoirSize, alpha, Clock.DefaultClock)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExponentiallyDecayingSample"/> class.
        /// </summary>
        /// <param name="reservoirSize">the number of samples to keep in the sampling reservoir</param>
        /// <param name="alpha">the exponential decay factor; the higher this is, the more biased the sample will be towards newer </param>
        /// <param name="clock">The clock.</param>
        public ExponentiallyDecayingSample(
            int reservoirSize,
            double alpha,
            Clock clock)
        {
            //values = new ConcurrentSkipListMap<double, long>();
            values = new SortedDictionary<double, long>();
            @lock = new SlimReaderWriterLock(LockConstants.DefaultTimeout);
            this.alpha = alpha;
            this.reservoirSize = reservoirSize;
            this.clock = clock;
            Clear();
        }

        public void Clear()
        {
            LockForRescale();
            try
            {
                values.Clear();
                count.Set(0);
                startTime = CurrentTimeInSeconds();
                nextScaleTime.Set(clock.Tick + RESCALE_THRESHOLD);
            }
            finally
            {
                UnlockForRescale();
            }
        }

        public int Count => (int) Math.Min(reservoirSize, count.Get());

        public void Update(long value)
        {
            Update(value, CurrentTimeInSeconds());
        }

        /// <summary>
        /// Adds an old value with a fixed timestamp to the sample.
        /// </summary>
        /// <param name="value">The value to be added.</param>
        /// <param name="timestamp">The epoch timestamp in seconds.</param>
        public void Update(
            long value,
            long timestamp)
        {
            RescaleIfNeeded();

            LockForRegularUsage();
            try
            {
                var priority = Weight(timestamp - startTime) / ThreadLocalRandom.Current.NextDouble();
                long newCount = count.IncrementAndGet();
                if (newCount <= reservoirSize)
                {
                    values.Put(priority, value);
                }
                else {
                    double first = values.Keys.First();
                    if (first < priority)
                    {
                        if (!values.TryPutIfAbsent(priority, value, out long outValue))
                        {
                            // ensure we always remove an item
                            // - while (values.Remove(first) == null)
                            while (values.Remove(first))
                            { 
                                first = values.Keys.First();
                            }
                        }
                    }
                }
            }
            finally
            {
                UnlockForRegularUsage();
            }
        }

        private void RescaleIfNeeded()
        {
            long now = clock.Tick;
            long next = nextScaleTime.Get();
            if (now >= next)
            {
                Rescale(now, next);
            }
        }

        public Snapshot Snapshot
        {
            get {
                LockForRegularUsage();
                try
                {
                    return new Snapshot(values.Values);
                }
                finally
                {
                    UnlockForRegularUsage();
                }
            }
        }

        private long CurrentTimeInSeconds()
        {
            return TimeUnit.MILLISECONDS.ToSeconds(clock.Time());
        }

        private double Weight(long t)
        {
            return Math.Exp(alpha * t);
        }

        private void Rescale(
            long now,
            long next)
        {
            if (nextScaleTime.CompareAndSet(next, now + RESCALE_THRESHOLD))
            {
                LockForRescale();
                try
                {
                    var oldStartTime = startTime;
                    startTime = CurrentTimeInSeconds();
                    var keys = new List<double>(values.Keys);
                    foreach (var key in keys) {
                        if (values.TryRemove(key, out long value)) {
                            values.Put(key * Math.Exp(-alpha * (startTime - oldStartTime)), value);
                        }
                    }

                    // make sure the counter is in sync with the number of stored samples.
                    count.Set(values.Count);
                }
                finally
                {
                    UnlockForRescale();
                }
            }
        }

        private void UnlockForRescale()
        {
            @lock.WriteLock.Release();
        }

        private void LockForRescale()
        {
            @lock.WriteLock.Acquire();
        }

        private void LockForRegularUsage()
        {
            @lock.ReadLock.Acquire();
        }

        private void UnlockForRegularUsage()
        {
            @lock.ReadLock.Release();
        }
    }
} // end of namespace