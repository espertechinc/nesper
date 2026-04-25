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
    public class MonitorLockTests
    {
        private const int SmallTimeoutMs = 150;

        // ── MonitorLock ───────────────────────────────────────────────────────────────────

        [Test]
        public void MonitorLock_BasicAcquireRelease_DoesNotThrow()
        {
            var ml = new MonitorLock();
            Assert.DoesNotThrow(() =>
            {
                using var d = ml.Acquire();
            });
        }

        [Test]
        public void MonitorLock_AcquireWithTimeout_Succeeds()
        {
            var ml = new MonitorLock();
            Assert.DoesNotThrow(() =>
            {
                using var d = ml.Acquire(1000L);
            });
        }

        [Test]
        public void MonitorLock_LockTimeout_ReflectsConstructorValue()
        {
            var ml = new MonitorLock(12345);
            Assert.That(ml.LockTimeout, Is.EqualTo(12345));
        }

        [Test]
        public void MonitorLock_LockDepth_IsOneWhileHeld_ZeroAfterRelease()
        {
            var ml = new MonitorLock();
            using (ml.Acquire())
            {
                Assert.That(ml.LockDepth, Is.EqualTo(1));
            }
            Assert.That(ml.LockDepth, Is.EqualTo(0));
        }

        [Test]
        public void MonitorLock_IsHeldByCurrentThread_TrueInsideFalseOutside()
        {
            var ml = new MonitorLock();
            Assert.That(ml.IsHeldByCurrentThread, Is.False);
            using (ml.Acquire())
            {
                Assert.That(ml.IsHeldByCurrentThread, Is.True);
            }
            Assert.That(ml.IsHeldByCurrentThread, Is.False);
        }

        [Test]
        public void MonitorLock_Reentrant_SameThread_DoesNotDeadlock()
        {
            var ml = new MonitorLock();
            using (ml.Acquire())
            {
                Assert.That(ml.LockDepth, Is.EqualTo(1));
                using (ml.Acquire())
                {
                    Assert.That(ml.LockDepth, Is.EqualTo(2));
                }
                Assert.That(ml.LockDepth, Is.EqualTo(1));
            }
            Assert.That(ml.LockDepth, Is.EqualTo(0));
        }

        [Test]
        public void MonitorLock_Release_DoesNotThrow()
        {
            var ml = new MonitorLock();
            using (ml.Acquire())
            {
                Assert.DoesNotThrow(() => ml.Release());
            }
        }

        [Test]
        public void MonitorLock_ReleaseAcquire_TemporarilyReleasesLock()
        {
            var ml = new MonitorLock();
            var otherThreadAcquired = new ManualResetEventSlim(false);

            using (ml.Acquire())
            {
                using (ml.ReleaseAcquire())
                {
                    var task = Task.Run(() =>
                    {
                        using (ml.Acquire())
                        {
                            otherThreadAcquired.Set();
                        }
                    });
                    Assert.That(otherThreadAcquired.Wait(TimeSpan.FromSeconds(5)), Is.True,
                        "Other thread should acquire the lock during ReleaseAcquire.");
                    task.Wait(TimeSpan.FromSeconds(5));
                }
                Assert.That(ml.IsHeldByCurrentThread, Is.True,
                    "Lock should be re-acquired after ReleaseAcquire disposable is disposed.");
            }
        }

        [Test]
        public void MonitorLock_TimeoutUnderContention_ThrowsTimeoutException()
        {
            var ml = new MonitorLock(SmallTimeoutMs);
            var holderReady = new ManualResetEventSlim(false);
            var releaseHolder = new ManualResetEventSlim(false);

            var holder = Task.Run(() =>
            {
                using (ml.Acquire())
                {
                    holderReady.Set();
                    releaseHolder.Wait(TimeSpan.FromSeconds(10));
                }
            });

            Assert.That(holderReady.Wait(TimeSpan.FromSeconds(5)), Is.True);
            try
            {
                Assert.Throws<TimeoutException>(() => ml.Acquire());
            }
            finally
            {
                releaseHolder.Set();
                holder.Wait(TimeSpan.FromSeconds(5));
            }
        }

        // ── MonitorSlimLock ───────────────────────────────────────────────────────────────

        [Test]
        public void MonitorSlimLock_BasicAcquireRelease_DoesNotThrow()
        {
            var ml = new MonitorSlimLock();
            Assert.DoesNotThrow(() =>
            {
                using var d = ml.Acquire();
            });
        }

        [Test]
        public void MonitorSlimLock_AcquireWithTimeout_Succeeds()
        {
            var ml = new MonitorSlimLock();
            Assert.DoesNotThrow(() =>
            {
                using var d = ml.Acquire(1000L);
            });
        }

        [Test]
        public void MonitorSlimLock_LockTimeout_ReflectsConstructorValue()
        {
            var ml = new MonitorSlimLock(54321);
            Assert.That(ml.LockTimeout, Is.EqualTo(54321));
        }

        [Test]
        public void MonitorSlimLock_LockDepth_IsOneWhileHeld_ZeroAfterRelease()
        {
            var ml = new MonitorSlimLock();
            using (ml.Acquire())
            {
                Assert.That(ml.LockDepth, Is.EqualTo(1));
            }
            Assert.That(ml.LockDepth, Is.EqualTo(0));
        }

        [Test]
        public void MonitorSlimLock_IsHeldByCurrentThread_TrueInsideFalseOutside()
        {
            var ml = new MonitorSlimLock();
            Assert.That(ml.IsHeldByCurrentThread, Is.False);
            using (ml.Acquire())
            {
                Assert.That(ml.IsHeldByCurrentThread, Is.True);
            }
            Assert.That(ml.IsHeldByCurrentThread, Is.False);
        }

        [Test]
        public void MonitorSlimLock_Reentrant_SameThread_DoesNotDeadlock()
        {
            var ml = new MonitorSlimLock();
            using (ml.Acquire())
            {
                Assert.That(ml.LockDepth, Is.EqualTo(1));
                using (ml.Acquire())
                {
                    Assert.That(ml.LockDepth, Is.EqualTo(2));
                }
                Assert.That(ml.LockDepth, Is.EqualTo(1));
            }
            Assert.That(ml.LockDepth, Is.EqualTo(0));
        }

        [Test]
        public void MonitorSlimLock_Release_DoesNotThrow()
        {
            var ml = new MonitorSlimLock();
            using (ml.Acquire())
            {
                Assert.DoesNotThrow(() => ml.Release());
            }
        }

        [Test]
        public void MonitorSlimLock_ReleaseAcquire_TemporarilyReleasesLock()
        {
            var ml = new MonitorSlimLock();
            var otherThreadAcquired = new ManualResetEventSlim(false);

            using (ml.Acquire())
            {
                using (ml.ReleaseAcquire())
                {
                    var task = Task.Run(() =>
                    {
                        using (ml.Acquire())
                        {
                            otherThreadAcquired.Set();
                        }
                    });
                    Assert.That(otherThreadAcquired.Wait(TimeSpan.FromSeconds(5)), Is.True,
                        "Other thread should acquire the lock during ReleaseAcquire.");
                    task.Wait(TimeSpan.FromSeconds(5));
                }
                Assert.That(ml.IsHeldByCurrentThread, Is.True,
                    "Lock should be re-acquired after ReleaseAcquire disposable is disposed.");
            }
        }

        [Test]
        public void MonitorSlimLock_TimeoutUnderContention_ThrowsTimeoutException()
        {
            var ml = new MonitorSlimLock(SmallTimeoutMs);
            var holderReady = new ManualResetEventSlim(false);
            var releaseHolder = new ManualResetEventSlim(false);

            var holder = Task.Run(() =>
            {
                using (ml.Acquire())
                {
                    holderReady.Set();
                    releaseHolder.Wait(TimeSpan.FromSeconds(10));
                }
            });

            Assert.That(holderReady.Wait(TimeSpan.FromSeconds(5)), Is.True);
            try
            {
                Assert.Throws<TimeoutException>(() => ml.Acquire());
            }
            finally
            {
                releaseHolder.Set();
                holder.Wait(TimeSpan.FromSeconds(5));
            }
        }

        // ── MonitorSpinLock (deprecated — smoke tests) ────────────────────────────────────

        [Test]
        public void MonitorSpinLock_BasicAcquireRelease_DoesNotThrow()
        {
            var ml = new MonitorSpinLock();
            Assert.DoesNotThrow(() =>
            {
                using var d = ml.Acquire();
            });
        }

        [Test]
        public void MonitorSpinLock_AcquireWithTimeout_Succeeds()
        {
            var ml = new MonitorSpinLock();
            Assert.DoesNotThrow(() =>
            {
                using var d = ml.Acquire(1000L);
            });
        }

        [Test]
        public void MonitorSpinLock_LockDepth_IsOneWhileHeld_ZeroAfterRelease()
        {
            var ml = new MonitorSpinLock();
            using (ml.Acquire())
            {
                Assert.That(ml.LockDepth, Is.EqualTo(1));
            }
            Assert.That(ml.LockDepth, Is.EqualTo(0));
        }

        [Test]
        public void MonitorSpinLock_IsHeldByCurrentThread_TrueInsideFalseOutside()
        {
            var ml = new MonitorSpinLock();
            Assert.That(ml.IsHeldByCurrentThread, Is.False);
            using (ml.Acquire())
            {
                Assert.That(ml.IsHeldByCurrentThread, Is.True);
            }
            Assert.That(ml.IsHeldByCurrentThread, Is.False);
        }

        [Test]
        public void MonitorSpinLock_Release_DoesNotThrow()
        {
            var ml = new MonitorSpinLock();
            using (ml.Acquire())
            {
                Assert.DoesNotThrow(() => ml.Release());
            }
        }

        [Test]
        public void MonitorSpinLock_ReleaseAcquire_TemporarilyReleasesLock()
        {
            var ml = new MonitorSpinLock();
            var otherThreadAcquired = new ManualResetEventSlim(false);

            using (ml.Acquire())
            {
                using (ml.ReleaseAcquire())
                {
                    var task = Task.Run(() =>
                    {
                        using (ml.Acquire())
                        {
                            otherThreadAcquired.Set();
                        }
                    });
                    Assert.That(otherThreadAcquired.Wait(TimeSpan.FromSeconds(5)), Is.True,
                        "Other thread should acquire the lock during ReleaseAcquire.");
                    task.Wait(TimeSpan.FromSeconds(5));
                }
                Assert.That(ml.IsHeldByCurrentThread, Is.True,
                    "Lock should be re-acquired after ReleaseAcquire disposable is disposed.");
            }
        }

        [Test]
        public void MonitorSpinLock_Reentrant_SameThread_DoesNotDeadlock()
        {
            var ml = new MonitorSpinLock();
            using (ml.Acquire())
            {
                Assert.That(ml.LockDepth, Is.EqualTo(1));
                using (ml.Acquire())
                {
                    Assert.That(ml.LockDepth, Is.EqualTo(2));
                }
                Assert.That(ml.LockDepth, Is.EqualTo(1));
            }
            Assert.That(ml.LockDepth, Is.EqualTo(0));
        }

        [Test]
        public void MonitorSpinLock_TimeoutUnderContention_ThrowsTimeoutException()
        {
            var ml = new MonitorSpinLock(SmallTimeoutMs);
            var holderReady = new ManualResetEventSlim(false);
            var releaseHolder = new ManualResetEventSlim(false);

            var holder = Task.Run(() =>
            {
                using (ml.Acquire())
                {
                    holderReady.Set();
                    releaseHolder.Wait(TimeSpan.FromSeconds(10));
                }
            });

            Assert.That(holderReady.Wait(TimeSpan.FromSeconds(5)), Is.True);
            try
            {
                Assert.Throws<TimeoutException>(() => ml.Acquire());
            }
            finally
            {
                releaseHolder.Set();
                holder.Wait(TimeSpan.FromSeconds(5));
            }
        }
    }
}
