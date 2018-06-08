using System;
using System.Threading;

namespace com.espertech.esper.compat.threading
{
    public class CountDownLatch
    {
        private long _latchCount;

        public CountDownLatch(long latchCount)
        {
            _latchCount = latchCount;
        }

        /// <summary>
        /// Returns the number of outstanding latches that have not been
        /// removed.
        /// </summary>
        /// <value>The count.</value>
        public long Count => Interlocked.Read(ref _latchCount);

        /// <summary>
        /// Decrements the latch count.
        /// </summary>
        public void CountDown()
        {
            Interlocked.Decrement(ref _latchCount);
        }

        /// <summary>
        /// Waits for the latch to be released for up to the specified amount of time.
        /// If the timeout expires a TimeoutException is thrown.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        public bool Await(TimeSpan timeout)
        {
            var timeCur = DateTimeHelper.CurrentTimeMillis;
            var timeEnd = timeCur + (long) timeout.TotalMilliseconds;
            var iteration = 0;

            while(Interlocked.Read(ref _latchCount) > 0)
            {
                if (!SlimLock.SmartWait(++iteration, timeEnd))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Awaits this instance.
        /// </summary>
        /// <returns></returns>
        public bool Await()
        {
            return Await(TimeSpan.MaxValue);
        }

        public bool Await(int units, TimeUnit timeUnits)
        {
            return Await(TimeUnitHelper.ToTimeSpan(units, timeUnits));
        }
    }
}
