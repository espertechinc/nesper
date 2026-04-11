// Copyright (C) 2006-2024 Esper Team. All rights reserved.
// Subject to the terms of the GPL license (see license.txt).

using System;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.compat.threading.locks
{
    [TestFixture]
    public class CommonLockTests
    {
        private const int LockTimeoutMs = 5000;

        // ── CommonReadLock (non-generic, via SlimReaderWriterLock) ────────────────────────

        [Test]
        public void CommonReadLock_Acquire_ReturnsNonNullDisposable()
        {
            var rwl = new SlimReaderWriterLock(LockTimeoutMs);
            using var d = rwl.ReadLock.Acquire();
            Assert.That(d, Is.Not.Null);
        }

        [Test]
        public void CommonReadLock_AcquireWithTimeout_Succeeds()
        {
            var rwl = new SlimReaderWriterLock(LockTimeoutMs);
            Assert.DoesNotThrow(() =>
            {
                using var d = rwl.ReadLock.Acquire(2000L);
            });
        }

        [Test]
        public void CommonReadLock_Release_ReleasesLock()
        {
            var rwl = new SlimReaderWriterLock(LockTimeoutMs);
            rwl.ReadLock.Acquire();
            Assert.DoesNotThrow(() => rwl.ReadLock.Release());
            Assert.DoesNotThrow(() =>
            {
                using var w = rwl.WriteLock.Acquire();
            });
        }

        [Test]
        public void CommonReadLock_ReleaseAcquire_ReleasesAndReacquires()
        {
            var rwl = new SlimReaderWriterLock(LockTimeoutMs);
            var writerAcquiredDuringRelease = new ManualResetEventSlim(false);

            using (rwl.ReadLock.Acquire())
            {
                using (rwl.ReadLock.ReleaseAcquire())
                {
                    var task = Task.Run(() =>
                    {
                        using (rwl.WriteLock.Acquire())
                        {
                            writerAcquiredDuringRelease.Set();
                        }
                    });
                    Assert.That(writerAcquiredDuringRelease.Wait(TimeSpan.FromSeconds(5)), Is.True,
                        "Writer should acquire during read ReleaseAcquire.");
                    task.Wait(TimeSpan.FromSeconds(5));
                }
            }
        }

        // ── CommonWriteLock (non-generic, via SlimReaderWriterLock) ──────────────────────

        [Test]
        public void CommonWriteLock_Acquire_ReturnsNonNullDisposable()
        {
            var rwl = new SlimReaderWriterLock(LockTimeoutMs);
            using var d = rwl.WriteLock.Acquire();
            Assert.That(d, Is.Not.Null);
        }

        [Test]
        public void CommonWriteLock_AcquireWithTimeout_Succeeds()
        {
            var rwl = new SlimReaderWriterLock(LockTimeoutMs);
            Assert.DoesNotThrow(() =>
            {
                using var d = rwl.WriteLock.Acquire(2000L);
            });
        }

        [Test]
        public void CommonWriteLock_Release_ReleasesLock()
        {
            var rwl = new SlimReaderWriterLock(LockTimeoutMs);
            rwl.WriteLock.Acquire();
            Assert.DoesNotThrow(() => rwl.WriteLock.Release());
            Assert.DoesNotThrow(() =>
            {
                using var r = rwl.ReadLock.Acquire();
            });
        }

        [Test]
        public void CommonWriteLock_ReleaseAcquire_ReleasesAndReacquires()
        {
            var rwl = new SlimReaderWriterLock(LockTimeoutMs);
            var readerAcquiredDuringRelease = new ManualResetEventSlim(false);

            using (rwl.WriteLock.Acquire())
            {
                using (rwl.WriteLock.ReleaseAcquire())
                {
                    var task = Task.Run(() =>
                    {
                        using (rwl.ReadLock.Acquire())
                        {
                            readerAcquiredDuringRelease.Set();
                        }
                    });
                    Assert.That(readerAcquiredDuringRelease.Wait(TimeSpan.FromSeconds(5)), Is.True,
                        "Reader should acquire during write ReleaseAcquire.");
                    task.Wait(TimeSpan.FromSeconds(5));
                }
            }
        }

        // ── CommonReadLock<T> (generic, via FifoReaderWriterLock) ────────────────────────

        [Test]
        public void CommonReadLockGeneric_Acquire_ReturnsNonNullDisposable()
        {
            var rwl = new FifoReaderWriterLock(LockTimeoutMs);
            using var d = rwl.ReadLock.Acquire();
            Assert.That(d, Is.Not.Null);
        }

        [Test]
        public void CommonReadLockGeneric_AcquireWithTimeout_Succeeds()
        {
            var rwl = new FifoReaderWriterLock(LockTimeoutMs);
            Assert.DoesNotThrow(() =>
            {
                using var d = rwl.ReadLock.Acquire(2000L);
            });
        }

        [Test]
        public void CommonReadLockGeneric_Release_ReleasesLock()
        {
            var rwl = new FifoReaderWriterLock(LockTimeoutMs);
            rwl.ReadLock.Acquire();
            Assert.DoesNotThrow(() => rwl.ReadLock.Release());
            Assert.DoesNotThrow(() =>
            {
                using var w = rwl.WriteLock.Acquire();
            });
        }

        [Test]
        public void CommonReadLockGeneric_ReleaseAcquire_ReleasesAndReacquires()
        {
            var rwl = new FifoReaderWriterLock(LockTimeoutMs);
            var writerAcquiredDuringRelease = new ManualResetEventSlim(false);

            using (rwl.ReadLock.Acquire())
            {
                using (rwl.ReadLock.ReleaseAcquire())
                {
                    var task = Task.Run(() =>
                    {
                        using (rwl.WriteLock.Acquire())
                        {
                            writerAcquiredDuringRelease.Set();
                        }
                    });
                    Assert.That(writerAcquiredDuringRelease.Wait(TimeSpan.FromSeconds(5)), Is.True,
                        "Writer should acquire during generic read ReleaseAcquire.");
                    task.Wait(TimeSpan.FromSeconds(5));
                }
            }
        }

        // ── CommonWriteLock<T> (generic, via FifoReaderWriterLock) ───────────────────────

        [Test]
        public void CommonWriteLockGeneric_Acquire_ReturnsNonNullDisposable()
        {
            var rwl = new FifoReaderWriterLock(LockTimeoutMs);
            using var d = rwl.WriteLock.Acquire();
            Assert.That(d, Is.Not.Null);
        }

        [Test]
        public void CommonWriteLockGeneric_AcquireWithTimeout_Succeeds()
        {
            var rwl = new FifoReaderWriterLock(LockTimeoutMs);
            Assert.DoesNotThrow(() =>
            {
                using var d = rwl.WriteLock.Acquire(2000L);
            });
        }

        [Test]
        public void CommonWriteLockGeneric_Release_ReleasesLock()
        {
            var rwl = new FifoReaderWriterLock(LockTimeoutMs);
            rwl.WriteLock.Acquire();
            Assert.DoesNotThrow(() => rwl.WriteLock.Release());
            Assert.DoesNotThrow(() =>
            {
                using var r = rwl.ReadLock.Acquire();
            });
        }

        [Test]
        public void CommonWriteLockGeneric_ReleaseAcquire_ReleasesAndReacquires()
        {
            var rwl = new FifoReaderWriterLock(LockTimeoutMs);
            var readerAcquiredDuringRelease = new ManualResetEventSlim(false);

            using (rwl.WriteLock.Acquire())
            {
                using (rwl.WriteLock.ReleaseAcquire())
                {
                    var task = Task.Run(() =>
                    {
                        using (rwl.ReadLock.Acquire())
                        {
                            readerAcquiredDuringRelease.Set();
                        }
                    });
                    Assert.That(readerAcquiredDuringRelease.Wait(TimeSpan.FromSeconds(5)), Is.True,
                        "Reader should acquire during generic write ReleaseAcquire.");
                    task.Wait(TimeSpan.FromSeconds(5));
                }
            }
        }
    }
}
