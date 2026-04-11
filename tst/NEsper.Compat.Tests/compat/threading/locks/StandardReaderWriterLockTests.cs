// Copyright (C) 2006-2024 Esper Team. All rights reserved.
// Subject to the terms of the GPL license (see license.txt).

#pragma warning disable CS0618

using System;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.compat.threading.locks
{
    [TestFixture]
    public class StandardReaderWriterLockTests
    {
        private const int LockTimeoutMs = 5000;
        private const int SmallTimeoutMs = 150;

        [Test]
        public void AcquireReadLock_NoContention_Succeeds()
        {
            var rwl = new StandardReaderWriterLock(LockTimeoutMs);
            Assert.DoesNotThrow(() =>
            {
                using var d = rwl.AcquireReadLock();
            });
        }

        [Test]
        public void AcquireWriteLock_NoContention_Succeeds()
        {
            var rwl = new StandardReaderWriterLock(LockTimeoutMs);
            Assert.DoesNotThrow(() =>
            {
                using var d = rwl.AcquireWriteLock();
            });
        }

        [Test]
        public void AcquireWriteLock_WithTimeSpan_NoContention_Succeeds()
        {
            var rwl = new StandardReaderWriterLock(LockTimeoutMs);
            Assert.DoesNotThrow(() =>
            {
                using var d = rwl.AcquireWriteLock(TimeSpan.FromSeconds(5));
            });
        }

        [Test]
        public void IsWriterLockHeld_FalseInitially()
        {
            var rwl = new StandardReaderWriterLock(LockTimeoutMs);
            Assert.That(rwl.IsWriterLockHeld, Is.False);
        }

        [Test]
        public void IsWriterLockHeld_TrueWhileWriteLockHeld_FalseAfterRelease()
        {
            var rwl = new StandardReaderWriterLock(LockTimeoutMs);
            using (rwl.AcquireWriteLock())
            {
                Assert.That(rwl.IsWriterLockHeld, Is.True);
            }
            Assert.That(rwl.IsWriterLockHeld, Is.False);
        }

        [Test]
        public void ReleaseWriteLock_ReleasesLock_SubsequentReaderSucceeds()
        {
            var rwl = new StandardReaderWriterLock(LockTimeoutMs);
            rwl.WriteLock.Acquire();
            Assert.DoesNotThrow(() => rwl.ReleaseWriteLock());
            Assert.DoesNotThrow(() =>
            {
                using var r = rwl.AcquireReadLock();
            });
        }

        [Test]
        public void AcquireReadLock_WhileWriterHeld_ThrowsTimeoutException()
        {
            var rwl = new StandardReaderWriterLock(SmallTimeoutMs);
            var writerReady = new ManualResetEventSlim(false);
            var releaseWriter = new ManualResetEventSlim(false);

            var writer = Task.Run(() =>
            {
                using (rwl.AcquireWriteLock())
                {
                    writerReady.Set();
                    releaseWriter.Wait(TimeSpan.FromSeconds(10));
                }
            });

            Assert.That(writerReady.Wait(TimeSpan.FromSeconds(5)), Is.True);
            try
            {
                Assert.Throws<TimeoutException>(() => rwl.ReadLock.Acquire());
            }
            finally
            {
                releaseWriter.Set();
                writer.Wait(TimeSpan.FromSeconds(5));
            }
        }

        [Test]
        public void AcquireWriteLock_WhileReaderHeld_ThrowsTimeoutException()
        {
            var rwl = new StandardReaderWriterLock(SmallTimeoutMs);
            var readerReady = new ManualResetEventSlim(false);
            var releaseReader = new ManualResetEventSlim(false);

            var reader = Task.Run(() =>
            {
                using (rwl.AcquireReadLock())
                {
                    readerReady.Set();
                    releaseReader.Wait(TimeSpan.FromSeconds(10));
                }
            });

            Assert.That(readerReady.Wait(TimeSpan.FromSeconds(5)), Is.True);
            try
            {
                Assert.Throws<TimeoutException>(() => rwl.WriteLock.Acquire());
            }
            finally
            {
                releaseReader.Set();
                reader.Wait(TimeSpan.FromSeconds(5));
            }
        }
    }
}
