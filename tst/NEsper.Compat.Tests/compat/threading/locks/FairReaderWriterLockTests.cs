// Copyright (C) 2006-2024 Esper Team. All rights reserved.
// Subject to the terms of the GPL license (see license.txt).

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.compat.threading.locks
{
    /// <summary>
    /// Tests for Task 1.6: FairReaderWriterLock outer loop does not check the total timeout
    /// after SmartWait returns.
    ///
    /// NATURE OF BUG (different from 1.5 — NOT an infinite spin):
    ///   The outer loop calls WithMainLock(timeCur, timeEnd, action).  When the lock state
    ///   doesn't allow entry (action returns false), WithMainLock returns normally.  The loop
    ///   then calls SmartWait(++ii) and updates timeCur — but does not immediately check
    ///   whether timeCur has passed timeEnd.  It instead calls WithMainLock again, which
    ///   itself will throw TimeoutException once timeOut = timeEnd - timeCur &lt;= 0.
    ///
    ///   Effect: the timeout is honoured, but with a potential overshoot of one SmartWait
    ///   call.  For higher iteration counts SmartWait can sleep up to ~10 ms extra per cycle.
    ///
    /// FIX: after updating timeCur, add an explicit check:
    ///   if (timeCur >= timeEnd)
    ///       throw new TimeoutException("FairReaderWriterLock timeout expired");
    ///
    /// STATUS:
    ///   "Does time out" tests — PASS both before and after fix (lock does eventually time out).
    ///   "Times out within tight window" test — documents expected precision; may show overshoot.
    ///   All other correctness tests — PASS both before and after fix.
    ///
    /// The value of these tests: they form the regression baseline that the fix must not break,
    /// and they catch any future regression that turns the minor overshoot into a true hang.
    /// </summary>
    [TestFixture]
    public class FairReaderWriterLockTests
    {
        private const int LockTimeoutMs = 300;

        // ── Task 1.6: outer timeout check ───────────────────────────────────────────────

        [Test]
        public void AcquireReaderLock_WhenWriterHeld_ThrowsTimeoutException()
        {
            // AcquireReaderLock does time out eventually (via WithMainLock's own check),
            // but the explicit outer guard is missing — verify the timeout fires at all.
            //
            // PASSES NOW (lock does time out, just possibly with small overshoot).
            // PASSES AFTER FIX (timeout fires at the explicit check, no overshoot).

            var fair = new FairReaderWriterLock(LockTimeoutMs);
            var writerAcquired = new ManualResetEventSlim(false);
            var releaseWriter = new ManualResetEventSlim(false);

            var writerTask = Task.Run(() =>
            {
                using var w = fair.AcquireWriteLock();
                writerAcquired.Set();
                releaseWriter.Wait(TimeSpan.FromSeconds(30));
            });

            Assert.That(writerAcquired.Wait(TimeSpan.FromSeconds(5)), Is.True,
                "Writer failed to acquire lock during test setup.");

            try
            {
                // CommonReadLock.Acquire() → AcquireReaderLock(timeout)
                Assert.Throws<TimeoutException>(
                    () => fair.ReadLock.Acquire(),
                    "Reader must throw TimeoutException while writer holds the lock.");
            }
            finally
            {
                releaseWriter.Set();
                writerTask.Wait(TimeSpan.FromSeconds(5));
            }
        }

        [Test]
        public void AcquireWriterLock_WhenReaderHeld_ThrowsTimeoutException()
        {
            var fair = new FairReaderWriterLock(LockTimeoutMs);
            var readerAcquired = new ManualResetEventSlim(false);
            var releaseReader = new ManualResetEventSlim(false);

            var readerTask = Task.Run(() =>
            {
                using var r = fair.AcquireReadLock();
                readerAcquired.Set();
                releaseReader.Wait(TimeSpan.FromSeconds(30));
            });

            Assert.That(readerAcquired.Wait(TimeSpan.FromSeconds(5)), Is.True,
                "Reader failed to acquire lock during test setup.");

            try
            {
                Assert.Throws<TimeoutException>(
                    () => fair.WriteLock.Acquire(),
                    "Writer must throw TimeoutException while reader holds the lock.");
            }
            finally
            {
                releaseReader.Set();
                readerTask.Wait(TimeSpan.FromSeconds(5));
            }
        }

        [Test]
        public void AcquireReaderLock_WhenWriterHeld_TimesOutWithinReasonableWindow()
        {
            // Measures wall-clock duration of a timed-out reader acquisition.
            //
            // Without fix: one extra SmartWait cycle may run after timeout — small overshoot.
            // With fix: times out as soon as timeCur >= timeEnd, no extra wait.
            //
            // The bound is deliberately loose (4×) to avoid flakiness on slow CI.
            // The lower bound (> 0.5×) guards against spurious early returns.

            const double lowerBoundFactor = 0.5;
            const double upperBoundFactor = 4.0;

            var fair = new FairReaderWriterLock(LockTimeoutMs);
            var writerAcquired = new ManualResetEventSlim(false);
            var releaseWriter = new ManualResetEventSlim(false);

            var writerTask = Task.Run(() =>
            {
                using var w = fair.AcquireWriteLock();
                writerAcquired.Set();
                releaseWriter.Wait(TimeSpan.FromSeconds(30));
            });

            Assert.That(writerAcquired.Wait(TimeSpan.FromSeconds(5)), Is.True);

            var sw = Stopwatch.StartNew();
            try
            {
                Assert.Throws<TimeoutException>(() => fair.ReadLock.Acquire());
            }
            finally
            {
                releaseWriter.Set();
                writerTask.Wait(TimeSpan.FromSeconds(5));
            }
            sw.Stop();

            long elapsedMs = sw.ElapsedMilliseconds;
            Assert.That(elapsedMs, Is.GreaterThan((long)(LockTimeoutMs * lowerBoundFactor)),
                $"Timed out too quickly ({elapsedMs} ms); expected at least {LockTimeoutMs * lowerBoundFactor} ms.");
            Assert.That(elapsedMs, Is.LessThan((long)(LockTimeoutMs * upperBoundFactor)),
                $"Timed out too slowly ({elapsedMs} ms); should fire within {LockTimeoutMs * upperBoundFactor} ms.");
        }

        [Test]
        public void AcquireWriterLock_WhenReaderHeld_TimesOutWithinReasonableWindow()
        {
            const double lowerBoundFactor = 0.5;
            const double upperBoundFactor = 4.0;

            var fair = new FairReaderWriterLock(LockTimeoutMs);
            var readerAcquired = new ManualResetEventSlim(false);
            var releaseReader = new ManualResetEventSlim(false);

            var readerTask = Task.Run(() =>
            {
                using var r = fair.AcquireReadLock();
                readerAcquired.Set();
                releaseReader.Wait(TimeSpan.FromSeconds(30));
            });

            Assert.That(readerAcquired.Wait(TimeSpan.FromSeconds(5)), Is.True);

            var sw = Stopwatch.StartNew();
            try
            {
                Assert.Throws<TimeoutException>(() => fair.WriteLock.Acquire());
            }
            finally
            {
                releaseReader.Set();
                readerTask.Wait(TimeSpan.FromSeconds(5));
            }
            sw.Stop();

            long elapsedMs = sw.ElapsedMilliseconds;
            Assert.That(elapsedMs, Is.GreaterThan((long)(LockTimeoutMs * lowerBoundFactor)),
                $"Timed out too quickly ({elapsedMs} ms).");
            Assert.That(elapsedMs, Is.LessThan((long)(LockTimeoutMs * upperBoundFactor)),
                $"Timed out too slowly ({elapsedMs} ms).");
        }

        // ── IsWriterLockHeld ─────────────────────────────────────────────────────────────

        [Test]
        public void IsWriterLockHeld_FalseInitially()
        {
            var fair = new FairReaderWriterLock(LockConstants.DefaultTimeout);
            Assert.That(fair.IsWriterLockHeld, Is.False);
        }

        [Test]
        public void IsWriterLockHeld_TrueWhileWriteLockHeld_FalseAfterRelease()
        {
            var fair = new FairReaderWriterLock(LockConstants.DefaultTimeout);

            using (fair.AcquireWriteLock())
            {
                Assert.That(fair.IsWriterLockHeld, Is.True,
                    "IsWriterLockHeld must be true while write lock is held by current thread.");
            }

            Assert.That(fair.IsWriterLockHeld, Is.False,
                "IsWriterLockHeld must be false after write lock is released.");
        }

        [Test]
        public void IsWriterLockHeld_FalseWhileReadLockHeld()
        {
            var fair = new FairReaderWriterLock(LockConstants.DefaultTimeout);

            using (fair.AcquireReadLock())
            {
                Assert.That(fair.IsWriterLockHeld, Is.False,
                    "IsWriterLockHeld must be false when only a read lock is held.");
            }
        }

        [Test]
        public void IsWriterLockHeld_FalseOnNonOwnerThread_WhileOwnerHoldsWriteLock()
        {
            var fair = new FairReaderWriterLock(LockConstants.DefaultTimeout);
            var writerAcquired = new ManualResetEventSlim(false);
            var releaseWriter = new ManualResetEventSlim(false);
            bool? observedOnOtherThread = null;

            var writerTask = Task.Run(() =>
            {
                using var w = fair.AcquireWriteLock();
                writerAcquired.Set();
                releaseWriter.Wait(TimeSpan.FromSeconds(10));
            });

            Assert.That(writerAcquired.Wait(TimeSpan.FromSeconds(5)), Is.True);

            // Check IsWriterLockHeld from a different thread — must be false (it's owner-thread-specific).
            var observerTask = Task.Run(() => observedOnOtherThread = fair.IsWriterLockHeld);
            observerTask.Wait(TimeSpan.FromSeconds(5));

            releaseWriter.Set();
            writerTask.Wait(TimeSpan.FromSeconds(5));

            Assert.That(observedOnOtherThread, Is.False,
                "IsWriterLockHeld must return false on a thread that does not own the write lock.");
        }

        // ── Basic correctness tests (PASS both before and after fix) ─────────────────────

        [Test]
        public void AcquireReadLock_WithNoContention_Succeeds()
        {
            var fair = new FairReaderWriterLock(LockConstants.DefaultTimeout);
            Assert.DoesNotThrow(() =>
            {
                using var r = fair.AcquireReadLock();
            });
        }

        [Test]
        public void AcquireWriteLock_WithNoContention_Succeeds()
        {
            var fair = new FairReaderWriterLock(LockConstants.DefaultTimeout);
            Assert.DoesNotThrow(() =>
            {
                using var w = fair.AcquireWriteLock();
            });
        }

        [Test]
        public void MultipleReaders_CanHoldLockConcurrently()
        {
            var fair = new FairReaderWriterLock(LockConstants.DefaultTimeout);
            var reader1Ready = new ManualResetEventSlim(false);
            var reader2Ready = new ManualResetEventSlim(false);
            var readersCanRelease = new ManualResetEventSlim(false);
            var errors = new System.Collections.Concurrent.ConcurrentBag<Exception>();

            void Reader(ManualResetEventSlim ready)
            {
                try
                {
                    using var r = fair.AcquireReadLock();
                    ready.Set();
                    readersCanRelease.Wait(TimeSpan.FromSeconds(10));
                }
                catch (Exception ex) { errors.Add(ex); }
            }

            var t1 = Task.Run(() => Reader(reader1Ready));
            var t2 = Task.Run(() => Reader(reader2Ready));

            Assert.That(reader1Ready.Wait(TimeSpan.FromSeconds(5)), Is.True,
                "Reader 1 should acquire lock.");
            Assert.That(reader2Ready.Wait(TimeSpan.FromSeconds(5)), Is.True,
                "Reader 2 should acquire lock concurrently with reader 1.");

            readersCanRelease.Set();
            Task.WaitAll(new[] { t1, t2 }, TimeSpan.FromSeconds(5));

            Assert.That(errors, Is.Empty, "Concurrent readers must not throw.");
        }

        [Test]
        public void WriterBlocksReader_AndReaderRelease_AllowsWriter()
        {
            var fair = new FairReaderWriterLock(LockConstants.DefaultTimeout);
            var readerAcquired = new ManualResetEventSlim(false);
            var releaseReader = new ManualResetEventSlim(false);
            var writerAcquired = new ManualResetEventSlim(false);

            var readerTask = Task.Run(() =>
            {
                using var r = fair.AcquireReadLock();
                readerAcquired.Set();
                releaseReader.Wait(TimeSpan.FromSeconds(10));
            });

            Assert.That(readerAcquired.Wait(TimeSpan.FromSeconds(5)), Is.True);

            var writerTask = Task.Run(() =>
            {
                using var w = fair.AcquireWriteLock();
                writerAcquired.Set();
            });

            // Writer must not acquire while reader holds.
            bool writerAcquiredEarly = writerAcquired.Wait(TimeSpan.FromMilliseconds(100));
            Assert.That(writerAcquiredEarly, Is.False, "Writer must not acquire while reader holds.");

            // Release reader.
            releaseReader.Set();
            Assert.That(writerAcquired.Wait(TimeSpan.FromSeconds(5)), Is.True,
                "Writer must acquire once reader releases.");

            readerTask.Wait(TimeSpan.FromSeconds(5));
            writerTask.Wait(TimeSpan.FromSeconds(5));
        }

        [Test]
        public void AcquireReadLock_AfterWriterReleases_Succeeds()
        {
            var fair = new FairReaderWriterLock(LockConstants.DefaultTimeout);

            using (fair.AcquireWriteLock())
            {
                // writer briefly held
            }

            Assert.DoesNotThrow(() =>
            {
                using var r = fair.AcquireReadLock();
            });
        }
    }
}
