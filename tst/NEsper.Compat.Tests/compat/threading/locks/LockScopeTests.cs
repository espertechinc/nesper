// Copyright (C) 2006-2024 Esper Team. All rights reserved.
// Subject to the terms of the GPL license (see license.txt).

using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.compat.threading.locks
{
    [TestFixture]
    public class LockScopeTests
    {
        private const int LockTimeoutMs = 5000;

        // ── LockScope struct identity ─────────────────────────────────────────────────────

        [Test]
        public void LockScope_IsValueType()
        {
            Assert.That(typeof(LockScope).IsValueType, Is.True,
                "LockScope must be a struct to avoid heap allocation");
        }

        // ── ILockable.AcquireScope() through interface dispatch ───────────────────────────

        [Test]
        public void ILockable_AcquireScope_ThroughInterface_ReleasesLockOnDispose()
        {
            var rwl = new SlimReaderWriterLock(LockTimeoutMs);
            ILockable readLock = rwl.ReadLock;

            var scope = readLock.AcquireScope();
            scope.Dispose();

            Assert.DoesNotThrow(() => { using var _ = rwl.WriteLock.Acquire(); },
                "Write lock must be acquirable after scope.Dispose()");
        }

        [Test]
        public void ILockable_AcquireScopeWithTimeout_ThroughInterface_ReleasesLockOnDispose()
        {
            var rwl = new SlimReaderWriterLock(LockTimeoutMs);
            ILockable readLock = rwl.ReadLock;

            var scope = readLock.AcquireScope(2000L);
            scope.Dispose();

            Assert.DoesNotThrow(() => { using var _ = rwl.WriteLock.Acquire(); },
                "Write lock must be acquirable after timeout-scoped acquire disposes");
        }

        // ── CommonReadLock AcquireScope ───────────────────────────────────────────────────

        [Test]
        public void CommonReadLock_AcquireScope_ReleasesLockOnDispose()
        {
            var rwl = new SlimReaderWriterLock(LockTimeoutMs);
            var scope = rwl.ReadLock.AcquireScope();
            scope.Dispose();

            Assert.DoesNotThrow(() => { using var _ = rwl.WriteLock.Acquire(); },
                "Write lock must be acquirable after read scope disposes");
        }

        [Test]
        public void CommonReadLock_AcquireScope_UsingVar_ReleasesOnScopeExit()
        {
            var rwl = new SlimReaderWriterLock(LockTimeoutMs);
            using (rwl.ReadLock.AcquireScope()) { }

            Assert.DoesNotThrow(() => { using var _ = rwl.WriteLock.Acquire(); },
                "Write lock must be acquirable after using-scope exits");
        }

        [Test]
        public void CommonReadLock_AcquireScope_WhileHeld_PreventsWriteFromOtherThread()
        {
            var rwl = new SlimReaderWriterLock(200);
            using (rwl.ReadLock.AcquireScope())
            {
                bool writeSucceeded = false;
                var task = Task.Run(() =>
                {
                    try { using var _ = rwl.WriteLock.Acquire(); writeSucceeded = true; }
                    catch (System.TimeoutException) { }
                });
                task.Wait(System.TimeSpan.FromSeconds(2));
                Assert.That(writeSucceeded, Is.False,
                    "Write from another thread must not succeed while read scope is active");
            }
        }

        // ── CommonWriteLock AcquireScope ──────────────────────────────────────────────────

        [Test]
        public void CommonWriteLock_AcquireScope_ReleasesLockOnDispose()
        {
            var rwl = new SlimReaderWriterLock(LockTimeoutMs);
            var scope = rwl.WriteLock.AcquireScope();
            scope.Dispose();

            Assert.DoesNotThrow(() => { using var _ = rwl.ReadLock.Acquire(); },
                "Read lock must be acquirable after write scope disposes");
        }

        [Test]
        public void CommonWriteLock_AcquireScope_UsingVar_ReleasesOnScopeExit()
        {
            var rwl = new SlimReaderWriterLock(LockTimeoutMs);
            using (rwl.WriteLock.AcquireScope()) { }

            Assert.DoesNotThrow(() => { using var _ = rwl.ReadLock.Acquire(); },
                "Read lock must be acquirable after write using-scope exits");
        }

        // ── MonitorLock AcquireScope ──────────────────────────────────────────────────────

        [Test]
        public void MonitorLock_AcquireScope_IsHeldWhileActive_ReleasesOnDispose()
        {
            var ml = new MonitorLock();
            var scope = ml.AcquireScope();
            Assert.That(ml.IsHeldByCurrentThread, Is.True);
            scope.Dispose();
            Assert.That(ml.IsHeldByCurrentThread, Is.False);
        }

        [Test]
        public void MonitorLock_AcquireScope_UsingVar_ReleasesOnScopeExit()
        {
            var ml = new MonitorLock();
            using (ml.AcquireScope()) { }
            Assert.That(ml.IsHeldByCurrentThread, Is.False);
        }

        // ── MonitorSlimLock AcquireScope ──────────────────────────────────────────────────

        [Test]
        public void MonitorSlimLock_AcquireScope_IsHeldWhileActive_ReleasesOnDispose()
        {
            var ml = new MonitorSlimLock();
            var scope = ml.AcquireScope();
            Assert.That(ml.IsHeldByCurrentThread, Is.True);
            scope.Dispose();
            Assert.That(ml.IsHeldByCurrentThread, Is.False);
        }

        [Test]
        public void MonitorSlimLock_AcquireScope_UsingVar_ReleasesOnScopeExit()
        {
            var ml = new MonitorSlimLock();
            using (ml.AcquireScope()) { }
            Assert.That(ml.IsHeldByCurrentThread, Is.False);
        }

        [Test]
        public void MonitorSlimLock_AcquireScope_BlocksOtherThread_UntilDisposed()
        {
            var ml = new MonitorSlimLock(LockTimeoutMs);
            var otherAcquired = new ManualResetEventSlim(false);

            var scope = ml.AcquireScope();
            var task = Task.Run(() =>
            {
                using (ml.AcquireScope())
                {
                    otherAcquired.Set();
                }
            });

            // other thread should NOT have acquired yet
            Assert.That(otherAcquired.Wait(50), Is.False,
                "Other thread must not acquire while scope is held");

            scope.Dispose();

            Assert.That(otherAcquired.Wait(System.TimeSpan.FromSeconds(5)), Is.True,
                "Other thread must acquire after scope is disposed");
            task.Wait(System.TimeSpan.FromSeconds(5));
        }

        // ── VoidLock AcquireScope ─────────────────────────────────────────────────────────

        [Test]
        public void VoidLock_AcquireScope_DoesNotThrow()
        {
            var vl = new VoidLock();
            Assert.DoesNotThrow(() =>
            {
                using (vl.AcquireScope()) { }
            });
        }

    }
}
