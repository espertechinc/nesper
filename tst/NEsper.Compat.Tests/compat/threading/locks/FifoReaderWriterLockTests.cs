// Copyright (C) 2006-2024 Esper Team. All rights reserved.
// Subject to the terms of the GPL license (see license.txt).

using System;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.compat.threading.locks
{
    /// <summary>
    /// Tests for FifoReaderWriterLock.AcquireReaderLock(long) and AcquireWriterLock(long).
    ///
    /// Both methods compute a deadline (timeEnd) and pass it to
    /// SlimLock.SmartWait(iter, timeEnd), which returns false when the deadline
    /// expires, causing a TimeoutException to be thrown from the spin loop.
    ///
    /// Timeout tests  — verify that a timed-out acquisition faults with TimeoutException
    ///                   within the expected window.
    /// Correctness tests — verify basic lock/unlock and mutual-exclusion semantics.
    /// </summary>
    [TestFixture]
    public class FifoReaderWriterLockTests
    {
        // The timeout passed to the lock under test.
        private const int LockTimeoutMs = 200;

        // How long we wait for the task before concluding it's stuck.
        // 10× is generous enough for slow CI while staying far below "infinite".
        private const int OuterWaitMs = LockTimeoutMs * 10;

        // ── Timeout tests ───────────────────────────────────────────────────────────────

        [Test]
        public void AcquireReaderLock_WhenWriterHeld_TimesOutWithinExpectedWindow()
        {
            // Verifies that AcquireReaderLock honours the timeout argument by passing
            // timeEnd to SlimLock.SmartWait(iter, timeEnd), which returns false when the
            // deadline is exceeded, causing a TimeoutException to be thrown.

            var fifo = new FifoReaderWriterLock(LockConstants.DefaultTimeout);
            var writerAcquired = new ManualResetEventSlim(false);
            var releaseWriter = new ManualResetEventSlim(false);

            var writerTask = Task.Run(() =>
            {
                using var w = fifo.AcquireWriteLock();
                writerAcquired.Set();
                releaseWriter.Wait(TimeSpan.FromSeconds(30));
            });

            Assert.That(writerAcquired.Wait(TimeSpan.FromSeconds(5)), Is.True,
                "Writer failed to acquire lock during test setup.");

            // ReadLock.Acquire(ms) → CommonReadLock<Node>.Acquire(long) → AcquireReaderLock(long)
            var readerTask = Task.Run(() => fifo.ReadLock.Acquire(LockTimeoutMs));
            // Use WaitAny so a faulted task returns the index rather than throwing AggregateException.
            bool completed = Task.WaitAny(new Task[] { readerTask }, TimeSpan.FromMilliseconds(OuterWaitMs)) != -1;

            // Always release the writer so background tasks can drain.
            releaseWriter.Set();
            writerTask.Wait(TimeSpan.FromSeconds(5));

            Assert.That(completed, Is.True,
                $"Reader acquisition did not complete within {OuterWaitMs} ms — it spun indefinitely. " +
                "timeEnd is computed in AcquireReaderLock but SmartWait(iter) is called " +
                "instead of SmartWait(iter, timeEnd) (Task 1.5).");

            Assert.That(readerTask.IsFaulted, Is.True,
                "Timed-out acquisition must fault the task.");
            Assert.That(readerTask.Exception!.InnerException, Is.InstanceOf<TimeoutException>(),
                "The faulting exception must be TimeoutException.");
        }

        [Test]
        public void AcquireWriterLock_WhenReaderHeld_TimesOutWithinExpectedWindow()
        {
            // Verifies that AcquireWriterLock honours the timeout argument by passing
            // timeEnd to SlimLock.SmartWait(iter, timeEnd), which returns false when the
            // deadline is exceeded, causing a TimeoutException to be thrown.

            var fifo = new FifoReaderWriterLock(LockConstants.DefaultTimeout);
            var readerAcquired = new ManualResetEventSlim(false);
            var releaseReader = new ManualResetEventSlim(false);

            var readerTask = Task.Run(() =>
            {
                using var r = fifo.AcquireReadLock();
                readerAcquired.Set();
                releaseReader.Wait(TimeSpan.FromSeconds(30));
            });

            Assert.That(readerAcquired.Wait(TimeSpan.FromSeconds(5)), Is.True,
                "Reader failed to acquire lock during test setup.");

            // WriteLock.Acquire(ms) → CommonWriteLock<Node>.Acquire(long) → AcquireWriterLock(long)
            var contestedWriterTask = Task.Run(() => fifo.WriteLock.Acquire(LockTimeoutMs));
            // Use WaitAny so a faulted task returns the index rather than throwing AggregateException.
            bool completed = Task.WaitAny(new Task[] { contestedWriterTask }, TimeSpan.FromMilliseconds(OuterWaitMs)) != -1;

            releaseReader.Set();
            readerTask.Wait(TimeSpan.FromSeconds(5));

            Assert.That(completed, Is.True,
                $"Writer acquisition did not complete within {OuterWaitMs} ms — it spun indefinitely. " +
                "timeEnd is computed in AcquireWriterLock but SmartWait(iter) is called " +
                "instead of SmartWait(iter, timeEnd) (Task 1.5).");

            Assert.That(contestedWriterTask.IsFaulted, Is.True,
                "Timed-out acquisition must fault the task.");
            Assert.That(contestedWriterTask.Exception!.InnerException, Is.InstanceOf<TimeoutException>(),
                "The faulting exception must be TimeoutException.");
        }

        [Test]
        public void AcquireReaderLock_WhenWriterHeld_ThenWriterReleases_ReaderAcquires()
        {
            // Paired with the timeout test: once the writer releases, the reader must
            // proceed normally.  This verifies the spin loop logic is otherwise correct.

            var fifo = new FifoReaderWriterLock(LockConstants.DefaultTimeout);
            var writerAcquired = new ManualResetEventSlim(false);
            var releaseWriter = new ManualResetEventSlim(false);
            var readerAcquired = new ManualResetEventSlim(false);

            var writerTask = Task.Run(() =>
            {
                using var w = fifo.AcquireWriteLock();
                writerAcquired.Set();
                releaseWriter.Wait(TimeSpan.FromSeconds(10));
            });

            Assert.That(writerAcquired.Wait(TimeSpan.FromSeconds(5)), Is.True,
                "Writer failed to acquire lock during test setup.");

            // Reader waits for writer — uses default (long) timeout so it doesn't time out.
            var readerTask = Task.Run(() =>
            {
                using var r = fifo.AcquireReadLock();
                readerAcquired.Set();
            });

            // Let reader spin for a moment, then release writer.
            Thread.Sleep(50);
            releaseWriter.Set();

            Assert.That(readerAcquired.Wait(TimeSpan.FromSeconds(5)), Is.True,
                "Reader should acquire once writer releases.");

            writerTask.Wait(TimeSpan.FromSeconds(5));
            readerTask.Wait(TimeSpan.FromSeconds(5));
        }

        // ── Basic correctness tests (PASS both before and after fix) ─────────────────────

        [Test]
        public void AcquireReadLock_WithNoContention_Succeeds()
        {
            var fifo = new FifoReaderWriterLock(LockConstants.DefaultTimeout);
            Assert.DoesNotThrow(() =>
            {
                using var r = fifo.AcquireReadLock();
            });
        }

        [Test]
        public void AcquireWriteLock_WithNoContention_Succeeds()
        {
            var fifo = new FifoReaderWriterLock(LockConstants.DefaultTimeout);
            Assert.DoesNotThrow(() =>
            {
                using var w = fifo.AcquireWriteLock();
            });
        }

        [Test]
        public void AcquireReadLock_AfterWriterReleases_Succeeds()
        {
            var fifo = new FifoReaderWriterLock(LockConstants.DefaultTimeout);

            using (fifo.AcquireWriteLock())
            {
                // writer held
            }

            Assert.DoesNotThrow(() =>
            {
                using var r = fifo.AcquireReadLock();
            });
        }

        [Test]
        public void AcquireWriteLock_AfterReaderReleases_Succeeds()
        {
            var fifo = new FifoReaderWriterLock(LockConstants.DefaultTimeout);

            using (fifo.AcquireReadLock())
            {
                // reader held
            }

            Assert.DoesNotThrow(() =>
            {
                using var w = fifo.AcquireWriteLock();
            });
        }

        [Test]
        public void IsWriterLockHeld_ReflectsActualState()
        {
            var fifo = new FifoReaderWriterLock(LockConstants.DefaultTimeout);

            Assert.That(fifo.IsWriterLockHeld, Is.False, "No lock held initially.");

            // IsWriterLockHeld checks _rnode.Flags == Exclusive on the calling thread.
            // Since the write lock is acquired on this thread, it should return true.
            using (fifo.AcquireWriteLock())
            {
                Assert.That(fifo.IsWriterLockHeld, Is.True, "Write lock held.");
            }

            Assert.That(fifo.IsWriterLockHeld, Is.False, "Write lock released.");
        }

        [Test]
        public void WriterBlocksSubsequentWriter_UntilFirstReleases()
        {
            // FIFO ordering: second writer queues behind first; does not acquire concurrently.
            var fifo = new FifoReaderWriterLock(LockConstants.DefaultTimeout);
            var writer1Acquired = new ManualResetEventSlim(false);
            var releaseWriter1 = new ManualResetEventSlim(false);
            var writer2Acquired = new ManualResetEventSlim(false);

            var w1Task = Task.Run(() =>
            {
                using var w = fifo.AcquireWriteLock();
                writer1Acquired.Set();
                releaseWriter1.Wait(TimeSpan.FromSeconds(10));
            });

            Assert.That(writer1Acquired.Wait(TimeSpan.FromSeconds(5)), Is.True);

            var w2Task = Task.Run(() =>
            {
                using var w = fifo.AcquireWriteLock();
                writer2Acquired.Set();
            });

            // Writer 2 must NOT acquire while writer 1 is held.
            bool w2AcquiredEarly = writer2Acquired.Wait(TimeSpan.FromMilliseconds(150));
            Assert.That(w2AcquiredEarly, Is.False, "Writer 2 should not acquire while writer 1 holds.");

            // Release writer 1 — writer 2 should proceed.
            releaseWriter1.Set();
            Assert.That(writer2Acquired.Wait(TimeSpan.FromSeconds(5)), Is.True,
                "Writer 2 should acquire after writer 1 releases.");

            w1Task.Wait(TimeSpan.FromSeconds(5));
            w2Task.Wait(TimeSpan.FromSeconds(5));
        }
    }
}
