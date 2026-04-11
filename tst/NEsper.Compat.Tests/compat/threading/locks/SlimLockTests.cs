// Copyright (C) 2006-2024 Esper Team. All rights reserved.
// Subject to the terms of the GPL license (see license.txt).

using System;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using com.espertech.esper.compat;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.compat.threading.locks
{
    [TestFixture]
    public class SlimLockTests
    {
        // ── Enter() / Release() ────────────────────────────────────────────────────────────

        [Test]
        public void Enter_Uncontested_AcquiresAndReleasesSuccessfully()
        {
            var sl = new SlimLock();
            Assert.DoesNotThrow(() =>
            {
                sl.Enter();
                sl.Release();
            });
        }

        [Test]
        public void Enter_Reentrant_SameThread_DepthTrackedCorrectly()
        {
            // SlimLock._myLockDepth is private; verify depth tracking behaviourally.
            // Enter(timeout) from the SAME thread always succeeds (reentrancy path), so
            // the contention check must come from a separate thread.
            //
            // Expected sequence:
            //   depth 0 → Enter() → 1 → Enter() → 2
            //   Release() → depth 1  — second thread still blocked
            //   Release() → depth 0  — second thread can now acquire
            var sl = new SlimLock();

            sl.Enter(); // depth = 1
            sl.Enter(); // depth = 2

            sl.Release(); // depth = 1 — lock still owned by this thread

            // From another thread: should NOT acquire while depth > 0
            bool acquiredEarly = Task.Run(() => sl.Enter(50)).GetAwaiter().GetResult();
            Assert.That(acquiredEarly, Is.False,
                "Lock should still be held after only one Release() of two nested Enter() calls.");

            sl.Release(); // depth = 0 — lock is now free

            // From another thread: should now acquire and release from the same thread.
            bool acquiredAfter = Task.Run(() =>
            {
                bool r = sl.Enter(1000);
                if (r) sl.Release();
                return r;
            }).GetAwaiter().GetResult();
            Assert.That(acquiredAfter, Is.True,
                "Lock should be free after the second Release() brings depth to zero.");
        }

        // ── Enter(int timeout) ─────────────────────────────────────────────────────────────

        [Test]
        public void Enter_WithTimeout_Uncontested_ReturnsTrue()
        {
            var sl = new SlimLock();
            bool acquired = sl.Enter(1000);
            Assert.That(acquired, Is.True);
            sl.Release();
        }

        [Test]
        public void Enter_WithTimeout_ReentrantSameThread_ReturnsTrue()
        {
            var sl = new SlimLock();
            bool first = sl.Enter(1000);
            Assert.That(first, Is.True);
            bool second = sl.Enter(1000);
            Assert.That(second, Is.True);
            sl.Release();
            sl.Release();
        }

        [Test]
        public void Enter_WithTimeout_UnderContention_AcquiresAfterHolderReleases()
        {
            var sl = new SlimLock();
            var holderAcquired = new ManualResetEventSlim(false);
            var releaseHolder = new ManualResetEventSlim(false);

            var holderTask = Task.Run(() =>
            {
                sl.Enter();
                holderAcquired.Set();
                releaseHolder.Wait(TimeSpan.FromSeconds(10));
                sl.Release();
            });

            Assert.That(holderAcquired.Wait(TimeSpan.FromSeconds(5)), Is.True);

            bool acquired = false;
            var waiterTask = Task.Run(() =>
            {
                acquired = sl.Enter(5000);
                if (acquired) sl.Release();
            });

            Thread.Sleep(30);
            releaseHolder.Set();
            waiterTask.Wait(TimeSpan.FromSeconds(10));
            holderTask.Wait(TimeSpan.FromSeconds(5));

            Assert.That(acquired, Is.True, "Waiter should acquire the lock after holder releases.");
        }

        [Test]
        public void Enter_WithTimeout_UnderContention_ReturnsFalseOnTimeout()
        {
            var sl = new SlimLock();
            var holderAcquired = new ManualResetEventSlim(false);
            var releaseHolder = new ManualResetEventSlim(false);

            var holderTask = Task.Run(() =>
            {
                sl.Enter();
                holderAcquired.Set();
                releaseHolder.Wait(TimeSpan.FromSeconds(30));
                sl.Release();
            });

            Assert.That(holderAcquired.Wait(TimeSpan.FromSeconds(5)), Is.True);

            bool acquired = true;
            try
            {
                acquired = sl.Enter(50);
            }
            finally
            {
                releaseHolder.Set();
                holderTask.Wait(TimeSpan.FromSeconds(5));
            }

            Assert.That(acquired, Is.False, "Should return false when the lock cannot be acquired within timeout.");
        }

        // ── SmartWait(int) ─────────────────────────────────────────────────────────────────

        [Test]
        public void SmartWait_Iter1_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => SlimLock.SmartWait(1));
        }

        [Test]
        public void SmartWait_Iter15_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => SlimLock.SmartWait(15));
        }

        [Test]
        public void SmartWait_Iter45_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => SlimLock.SmartWait(45));
        }

        [Test]
        public void SmartWait_Iter75_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => SlimLock.SmartWait(75));
        }

        [Test]
        public void SmartWait_Iter110_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => SlimLock.SmartWait(110));
        }

        // ── SmartWait(int, long) ───────────────────────────────────────────────────────────

        [Test]
        public void SmartWait_WithFutureTimeEnd_Iter1_ReturnsTrue()
        {
            long futureEnd = DateTimeHelper.CurrentTimeMillis + 60000L;
            bool result = SlimLock.SmartWait(1, futureEnd);
            Assert.That(result, Is.True);
        }

        [Test]
        public void SmartWait_WithFutureTimeEnd_Iter15_ReturnsTrue()
        {
            long futureEnd = DateTimeHelper.CurrentTimeMillis + 60000L;
            bool result = SlimLock.SmartWait(15, futureEnd);
            Assert.That(result, Is.True);
        }

        [Test]
        public void SmartWait_WithFutureTimeEnd_Iter45_ReturnsTrue()
        {
            long futureEnd = DateTimeHelper.CurrentTimeMillis + 60000L;
            bool result = SlimLock.SmartWait(45, futureEnd);
            Assert.That(result, Is.True);
        }

        [Test]
        public void SmartWait_WithFutureTimeEnd_Iter75_ReturnsTrue()
        {
            long futureEnd = DateTimeHelper.CurrentTimeMillis + 60000L;
            bool result = SlimLock.SmartWait(75, futureEnd);
            Assert.That(result, Is.True);
        }

        [Test]
        public void SmartWait_WithFutureTimeEnd_Iter110_ReturnsTrue()
        {
            long futureEnd = DateTimeHelper.CurrentTimeMillis + 60000L;
            bool result = SlimLock.SmartWait(110, futureEnd);
            Assert.That(result, Is.True);
        }

        [Test]
        public void SmartWait_WithExpiredTimeEnd_MultipleOf10GreaterThan60_ReturnsFalse()
        {
            // iter=70: multiple of 10, > 60 — triggers the deadline check
            // timeEnd=0L is always in the past (epoch millis for the current time is ~1.7 trillion)
            bool result = SlimLock.SmartWait(70, 0L);
            Assert.That(result, Is.False);
        }
    }
}
