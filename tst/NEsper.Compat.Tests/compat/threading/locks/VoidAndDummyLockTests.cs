// Copyright (C) 2006-2024 Esper Team. All rights reserved.
// Subject to the terms of the GPL license (see license.txt).

using System;

using NUnit.Framework;

using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.compat.threading.locks
{
    [TestFixture]
    public class VoidAndDummyLockTests
    {
        // ── VoidLock ─────────────────────────────────────────────────────────────────────

        [Test]
        public void VoidLock_Acquire_ReturnsNonNullDisposable()
        {
            var vl = new VoidLock();
            var d = vl.Acquire();
            Assert.That(d, Is.Not.Null);
        }

        [Test]
        public void VoidLock_AcquireDispose_DoesNotThrow()
        {
            var vl = new VoidLock();
            Assert.DoesNotThrow(() =>
            {
                using var d = vl.Acquire();
            });
        }

        [Test]
        public void VoidLock_AcquireWithTimeout_ReturnsNonNullDisposable()
        {
            var vl = new VoidLock();
            var d = vl.Acquire(1000L);
            Assert.That(d, Is.Not.Null);
        }

        [Test]
        public void VoidLock_AcquireWithTimeout_DoesNotThrow()
        {
            var vl = new VoidLock();
            Assert.DoesNotThrow(() =>
            {
                using var d = vl.Acquire(500L);
            });
        }

        [Test]
        public void VoidLock_ReleaseAcquire_ReturnsNonNullDisposable()
        {
            var vl = new VoidLock();
            var d = vl.ReleaseAcquire();
            Assert.That(d, Is.Not.Null);
        }

        [Test]
        public void VoidLock_Release_DoesNotThrow()
        {
            var vl = new VoidLock();
            Assert.DoesNotThrow(() => vl.Release());
        }

        // ── VoidReaderWriterLock ──────────────────────────────────────────────────────────

        [Test]
        public void VoidReaderWriterLock_ReadLock_IsNotNull()
        {
            var vrwl = new VoidReaderWriterLock();
            Assert.That(vrwl.ReadLock, Is.Not.Null);
        }

        [Test]
        public void VoidReaderWriterLock_WriteLock_IsNotNull()
        {
            var vrwl = new VoidReaderWriterLock();
            Assert.That(vrwl.WriteLock, Is.Not.Null);
        }

        [Test]
        public void VoidReaderWriterLock_ReadLockAndWriteLock_AreSameInstance()
        {
            var vrwl = new VoidReaderWriterLock();
            Assert.That(vrwl.ReadLock, Is.SameAs(vrwl.WriteLock));
        }

        [Test]
        public void VoidReaderWriterLock_AcquireReadLock_ReturnsNonNull()
        {
            var vrwl = new VoidReaderWriterLock();
            using var d = vrwl.AcquireReadLock();
            Assert.That(d, Is.Not.Null);
        }

        [Test]
        public void VoidReaderWriterLock_AcquireWriteLock_ReturnsNonNull()
        {
            var vrwl = new VoidReaderWriterLock();
            using var d = vrwl.AcquireWriteLock();
            Assert.That(d, Is.Not.Null);
        }

        [Test]
        public void VoidReaderWriterLock_AcquireWriteLockWithTimeSpan_ReturnsNonNull()
        {
            var vrwl = new VoidReaderWriterLock();
            using var d = vrwl.AcquireWriteLock(TimeSpan.FromSeconds(1));
            Assert.That(d, Is.Not.Null);
        }

        [Test]
        public void VoidReaderWriterLock_ReleaseWriteLock_DoesNotThrow()
        {
            var vrwl = new VoidReaderWriterLock();
            Assert.DoesNotThrow(() => vrwl.ReleaseWriteLock());
        }

        [Test]
        public void VoidReaderWriterLock_IsWriterLockHeld_AlwaysFalse()
        {
            var vrwl = new VoidReaderWriterLock();
            Assert.That(vrwl.IsWriterLockHeld, Is.False);
            using (vrwl.AcquireWriteLock())
            {
                Assert.That(vrwl.IsWriterLockHeld, Is.False);
            }
        }

        // ── DummyReaderWriterLock ─────────────────────────────────────────────────────────

        [Test]
        public void DummyReaderWriterLock_ReadLock_IsNotNull()
        {
            var dummy = new DummyReaderWriterLock();
            Assert.That(dummy.ReadLock, Is.Not.Null);
        }

        [Test]
        public void DummyReaderWriterLock_WriteLock_IsNotNull()
        {
            var dummy = new DummyReaderWriterLock();
            Assert.That(dummy.WriteLock, Is.Not.Null);
        }

        [Test]
        public void DummyReaderWriterLock_ReadLockAndWriteLock_AreSameInstance()
        {
            var dummy = new DummyReaderWriterLock();
            Assert.That(dummy.ReadLock, Is.SameAs(dummy.WriteLock));
        }

        [Test]
        public void DummyReaderWriterLock_AcquireReadLock_ReturnsNonNull()
        {
            var dummy = new DummyReaderWriterLock();
            using var d = dummy.AcquireReadLock();
            Assert.That(d, Is.Not.Null);
        }

        [Test]
        public void DummyReaderWriterLock_AcquireWriteLock_ReturnsNonNull()
        {
            var dummy = new DummyReaderWriterLock();
            using var d = dummy.AcquireWriteLock();
            Assert.That(d, Is.Not.Null);
        }

        [Test]
        public void DummyReaderWriterLock_AcquireWriteLockWithTimeSpan_ReturnsNonNull()
        {
            var dummy = new DummyReaderWriterLock();
            using var d = dummy.AcquireWriteLock(TimeSpan.FromSeconds(1));
            Assert.That(d, Is.Not.Null);
        }

        [Test]
        public void DummyReaderWriterLock_ReleaseWriteLock_DoesNotThrow()
        {
            var dummy = new DummyReaderWriterLock();
            Assert.DoesNotThrow(() => dummy.ReleaseWriteLock());
        }

        [Test]
        public void DummyReaderWriterLock_IsWriterLockHeld_AlwaysFalse()
        {
            var dummy = new DummyReaderWriterLock();
            Assert.That(dummy.IsWriterLockHeld, Is.False);
        }
    }
}
