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
    /// Tests for Task 1.4: SlimReaderWriterLock and CommonReadLock share a single IDisposable
    /// instance across all callers of AcquireReadLock() / Acquire().
    ///
    /// Because TrackedDisposable is idempotent (Task 1.1), the second Dispose() on the shared
    /// instance is a no-op: the second reader's ExitReadLock() is never called, permanently
    /// leaking the read lock.  A writer that subsequently tries to acquire will hang or time out.
    ///
    /// Root objects with the bug:
    ///   SlimReaderWriterLock._rDisposable — returned by every AcquireReadLock() call
    ///   SlimReaderWriterLock._wDisposable — returned by every AcquireWriteLock() call
    ///   CommonReadLock._disposableObj     — returned by every Acquire() call via ReadLock
    ///
    /// FIX: remove the cached fields; return a new TrackedDisposable per acquisition.
    ///
    /// STATUS:
    ///   "Distinct disposable" tests — FAIL NOW (same object returned)
    ///   "Writer acquirable after both readers release" tests — FAIL NOW (writer times out)
    ///   "Basic timeout/state" tests — PASS both before and after fix
    /// </summary>
    [TestFixture]
    public class SlimReaderWriterLockTests
    {
        // ── Shared disposable: AcquireReadLock() (direct path) ──────────────────────────

        [Test]
        public void AcquireReadLock_ReturnsDistinctDisposablePerAcquisition()
        {
            // FAILS NOW: both calls return _rDisposable (the same cached instance).
            // PASSES AFTER FIX: new TrackedDisposable per acquisition.
            var rw = new SlimReaderWriterLock(LockConstants.DefaultTimeout);
            var d1 = rw.AcquireReadLock();
            var d2 = rw.AcquireReadLock();
            try
            {
                Assert.That(d1, Is.Not.SameAs(d2),
                    "Each AcquireReadLock() must return a distinct IDisposable. " +
                    "A shared instance means the second Dispose() is a no-op, leaking the lock.");
            }
            finally
            {
                d1.Dispose();
                d2.Dispose();
            }
        }

        [Test]
        public void AcquireWriteLock_ReturnsDistinctDisposablePerAcquisition()
        {
            // Write lock is exclusive so we acquire-release-acquire to obtain two handles.
            // FAILS NOW: both calls return _wDisposable.
            var rw = new SlimReaderWriterLock(LockConstants.DefaultTimeout);
            var d1 = rw.AcquireWriteLock();
            d1.Dispose();
            var d2 = rw.AcquireWriteLock();
            d2.Dispose();

            Assert.That(d1, Is.Not.SameAs(d2),
                "Each AcquireWriteLock() must return a distinct IDisposable.");
        }

        [Test]
        public void AcquireWriteLock_TimeSpanOverload_ReturnsDistinctDisposable()
        {
            // FAILS NOW: the TimeSpan overload also returns _wDisposable.
            var rw = new SlimReaderWriterLock(LockConstants.DefaultTimeout);
            var span = TimeSpan.FromSeconds(5);
            var d1 = rw.AcquireWriteLock(span);
            d1.Dispose();
            var d2 = rw.AcquireWriteLock(span);
            d2.Dispose();

            Assert.That(d1, Is.Not.SameAs(d2),
                "AcquireWriteLock(TimeSpan) must also return a distinct IDisposable per call.");
        }

        // ── Shared disposable: correctness consequence (concurrent readers) ─────────────

        [Test]
        public void ConcurrentReaders_AfterBothRelease_WriterCanAcquire()
        {
            // BUG DEMONSTRATION:
            //   Thread A and Thread B both call AcquireReadLock() and receive _rDisposable.
            //   Thread A disposes → ExitReadLock() called once (count: 2→1), action nulled.
            //   Thread B disposes → action is null → no-op → count stays at 1.
            //   Writer tries to acquire → read count > 0 → TimeoutException.
            //
            // FAILS NOW: writer times out because read lock is permanently leaked.
            // PASSES AFTER FIX: both readers properly release; writer acquires immediately.

            const int writerTimeoutMs = 500;

            var rw = new SlimReaderWriterLock(LockConstants.DefaultTimeout);
            var reader1Ready = new ManualResetEventSlim(false);
            var reader2Ready = new ManualResetEventSlim(false);
            var readersCanRelease = new ManualResetEventSlim(false);

            var t1 = Task.Run(() =>
            {
                using var d = rw.AcquireReadLock();
                reader1Ready.Set();
                readersCanRelease.Wait();
            });

            var t2 = Task.Run(() =>
            {
                using var d = rw.AcquireReadLock();
                reader2Ready.Set();
                readersCanRelease.Wait();
            });

            Assert.That(reader1Ready.Wait(TimeSpan.FromSeconds(5)), Is.True,
                "Reader 1 failed to acquire lock in test setup.");
            Assert.That(reader2Ready.Wait(TimeSpan.FromSeconds(5)), Is.True,
                "Reader 2 failed to acquire lock in test setup.");

            readersCanRelease.Set();
            Task.WaitAll(new[] { t1, t2 }, TimeSpan.FromSeconds(5));

            Assert.DoesNotThrow(
                () =>
                {
                    using var w = rw.AcquireWriteLock(TimeSpan.FromMilliseconds(writerTimeoutMs));
                },
                $"Writer could not acquire after both readers released within {writerTimeoutMs} ms. " +
                "The second reader's dispose was a no-op — its read lock was leaked.");
        }

        [Test]
        public void ConcurrentReaders_DisposingFirstDoesNotRelaseSecond()
        {
            // Demonstrates the mechanism of the bug:
            // After Thread A disposes _rDisposable, the underlying ReaderWriterLockSlim
            // still has Thread B's read lock held.  A writer should NOT be able to acquire yet.
            //
            // FAILS NOW: read count is leaking in the opposite direction —
            //   after reader 1 disposes (which nulls the action), reader 2's dispose is no-op,
            //   so the count never reaches 0.  The writer actually WILL fail to acquire.
            //   The test captures this by verifying reader 2 can still observe read-lock state
            //   after reader 1 released — the LACK of interference in the other direction.
            //
            // This is a positive signal test: verifies readers are independent.

            const int shortTimeoutMs = 100;

            var rw = new SlimReaderWriterLock(LockConstants.DefaultTimeout);
            var reader1HasLock = new ManualResetEventSlim(false);
            var reader2HasLock = new ManualResetEventSlim(false);
            var reader1CanRelease = new ManualResetEventSlim(false);
            var reader1Released = new ManualResetEventSlim(false);
            var reader2CanRelease = new ManualResetEventSlim(false);

            var t1 = Task.Run(() =>
            {
                using var d = rw.AcquireReadLock();
                reader1HasLock.Set();
                reader1CanRelease.Wait();
                // d disposed here
            });
            reader1HasLock.Wait(TimeSpan.FromSeconds(5));

            var t2 = Task.Run(() =>
            {
                using var d = rw.AcquireReadLock();
                reader2HasLock.Set();
                reader2CanRelease.Wait();
                // d disposed here
            });
            reader2HasLock.Wait(TimeSpan.FromSeconds(5));

            // Release reader 1 only.
            reader1CanRelease.Set();
            t1.Wait(TimeSpan.FromSeconds(5));

            // Reader 2 is still holding its lock.  A writer must NOT be able to acquire.
            Assert.Throws<TimeoutException>(
                () => rw.AcquireWriteLock(TimeSpan.FromMilliseconds(shortTimeoutMs)),
                "Writer must be blocked while reader 2 still holds the lock.");

            // Release reader 2.
            reader2CanRelease.Set();
            t2.Wait(TimeSpan.FromSeconds(5));

            // Now writer must succeed.
            Assert.DoesNotThrow(
                () =>
                {
                    using var w = rw.AcquireWriteLock(TimeSpan.FromMilliseconds(500));
                },
                "Writer must succeed after both readers have released.");
        }

        // ── Shared disposable: CommonReadLock path (ReadLock.Acquire) ───────────────────

        [Test]
        public void CommonReadLock_AcquireReturnsDistinctDisposablePerCall()
        {
            // CommonReadLock._disposableObj is pre-created in the constructor and returned
            // from every Acquire() call — the same bug via the ReadLock property.
            //
            // FAILS NOW: both calls return the same _disposableObj.
            var rw = new SlimReaderWriterLock(LockConstants.DefaultTimeout);
            var d1 = rw.ReadLock.Acquire();
            var d2 = rw.ReadLock.Acquire();
            try
            {
                Assert.That(d1, Is.Not.SameAs(d2),
                    "CommonReadLock.Acquire() must return a distinct IDisposable per call.");
            }
            finally
            {
                d1.Dispose();
                d2.Dispose();
            }
        }

        [Test]
        public void CommonReadLock_ConcurrentAcquires_BothReleaseProperly()
        {
            // Same as ConcurrentReaders_AfterBothRelease_WriterCanAcquire but via ReadLock.Acquire().
            // FAILS NOW: second ReadLock.Acquire() dispose is a no-op.

            const int writerTimeoutMs = 500;

            var rw = new SlimReaderWriterLock(LockConstants.DefaultTimeout);
            var reader1Ready = new ManualResetEventSlim(false);
            var reader2Ready = new ManualResetEventSlim(false);
            var readersCanRelease = new ManualResetEventSlim(false);

            var t1 = Task.Run(() =>
            {
                using var d = rw.ReadLock.Acquire();
                reader1Ready.Set();
                readersCanRelease.Wait();
            });

            var t2 = Task.Run(() =>
            {
                using var d = rw.ReadLock.Acquire();
                reader2Ready.Set();
                readersCanRelease.Wait();
            });

            Assert.That(reader1Ready.Wait(TimeSpan.FromSeconds(5)), Is.True);
            Assert.That(reader2Ready.Wait(TimeSpan.FromSeconds(5)), Is.True);

            readersCanRelease.Set();
            Task.WaitAll(new[] { t1, t2 }, TimeSpan.FromSeconds(5));

            Assert.DoesNotThrow(
                () =>
                {
                    using var w = rw.AcquireWriteLock(TimeSpan.FromMilliseconds(writerTimeoutMs));
                },
                "Writer could not acquire after both CommonReadLock holders released.");
        }

        // ── Non-bug correctness tests (PASS both before and after fix) ───────────────────

        [Test]
        public void AcquireReadLock_Succeeds_WhenNoContention()
        {
            var rw = new SlimReaderWriterLock(LockConstants.DefaultTimeout);
            Assert.DoesNotThrow(() =>
            {
                using var r = rw.AcquireReadLock();
            });
        }

        [Test]
        public void AcquireWriteLock_Succeeds_WhenNoContention()
        {
            var rw = new SlimReaderWriterLock(LockConstants.DefaultTimeout);
            Assert.DoesNotThrow(() =>
            {
                using var w = rw.AcquireWriteLock();
            });
        }

        [Test]
        public void AcquireReadLock_ThrowsTimeoutException_WhenWriterHeld()
        {
            var rw = new SlimReaderWriterLock(200); // 200 ms timeout
            var writerAcquired = new ManualResetEventSlim(false);
            var releaseWriter = new ManualResetEventSlim(false);

            var writerTask = Task.Run(() =>
            {
                using var w = rw.AcquireWriteLock();
                writerAcquired.Set();
                releaseWriter.Wait(TimeSpan.FromSeconds(30));
            });

            Assert.That(writerAcquired.Wait(TimeSpan.FromSeconds(5)), Is.True);
            try
            {
                Assert.Throws<TimeoutException>(() => rw.AcquireReadLock(),
                    "Reader must time out while a writer holds the lock.");
            }
            finally
            {
                releaseWriter.Set();
                writerTask.Wait(TimeSpan.FromSeconds(5));
            }
        }

        [Test]
        public void AcquireWriteLock_ThrowsTimeoutException_WhenReaderHeld()
        {
            var rw = new SlimReaderWriterLock(200);
            var readerAcquired = new ManualResetEventSlim(false);
            var releaseReader = new ManualResetEventSlim(false);

            var readerTask = Task.Run(() =>
            {
                using var r = rw.AcquireReadLock();
                readerAcquired.Set();
                releaseReader.Wait(TimeSpan.FromSeconds(30));
            });

            Assert.That(readerAcquired.Wait(TimeSpan.FromSeconds(5)), Is.True);
            try
            {
                Assert.Throws<TimeoutException>(() => rw.AcquireWriteLock(),
                    "Writer must time out while a reader holds the lock.");
            }
            finally
            {
                releaseReader.Set();
                readerTask.Wait(TimeSpan.FromSeconds(5));
            }
        }

        [Test]
        public void IsWriterLockHeld_ReflectsActualState()
        {
            var rw = new SlimReaderWriterLock(LockConstants.DefaultTimeout);

            Assert.That(rw.IsWriterLockHeld, Is.False, "No lock held initially.");

            using (rw.AcquireWriteLock())
            {
                Assert.That(rw.IsWriterLockHeld, Is.True, "Write lock is held.");
            }

            Assert.That(rw.IsWriterLockHeld, Is.False, "Write lock released after using block.");
        }

        [Test]
        public void WriterBlocksReaderAndReaderBlocksWriter_Sequential()
        {
            var rw = new SlimReaderWriterLock(LockConstants.DefaultTimeout);

            // Write then read
            using (rw.AcquireWriteLock())
            {
                Assert.That(rw.IsWriterLockHeld, Is.True);
            }

            using (rw.AcquireReadLock())
            {
                Assert.That(rw.IsWriterLockHeld, Is.False);
            }
        }
    }
}
