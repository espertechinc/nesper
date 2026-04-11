// Copyright (C) 2006-2024 Esper Team. All rights reserved.
// Subject to the terms of the GPL license (see license.txt).

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.compat.threading.locks
{
    [TestFixture]
    public class TelemetryTests
    {
        // ── TelemetryLockCategory ─────────────────────────────────────────────────────────

        [Test]
        public void TelemetryLockCategory_Name_PreservesValue()
        {
            var cat = new TelemetryLockCategory("mycat");
            Assert.That(cat.Name, Is.EqualTo("mycat"));
        }

        [Test]
        public void TelemetryLockCategory_OnLockReleased_AddsToEvents()
        {
            var cat = new TelemetryLockCategory("test");
            var e = new TelemetryEventArgs { Id = "1", RequestTime = 100, AcquireTime = 110, ReleaseTime = 120, StackTrace = new StackTrace() };
            cat.OnLockReleased(this, e);
            var events = cat.Events;
            Assert.That(events, Has.Count.EqualTo(1));
            Assert.That(events, Does.Contain(e));
        }

        [Test]
        public void TelemetryLockCategory_Events_ThreadSafety_BothEventsRecorded()
        {
            var cat = new TelemetryLockCategory("concurrent");
            var e1 = new TelemetryEventArgs { Id = "a", StackTrace = new StackTrace() };
            var e2 = new TelemetryEventArgs { Id = "b", StackTrace = new StackTrace() };

            var barrier = new Barrier(2);
            var t1 = Task.Run(() => { barrier.SignalAndWait(); cat.OnLockReleased(this, e1); });
            var t2 = Task.Run(() => { barrier.SignalAndWait(); cat.OnLockReleased(this, e2); });
            Task.WaitAll(new[] { t1, t2 }, TimeSpan.FromSeconds(5));

            var events = cat.Events;
            Assert.That(events, Has.Count.EqualTo(2));
        }

        // ── TelemetryEngine ───────────────────────────────────────────────────────────────

        [Test]
        public void TelemetryEngine_GetCategory_New_ReturnsNonNullWithCorrectName()
        {
            var engine = new TelemetryEngine();
            var cat = engine.GetCategory("alpha");
            Assert.That(cat, Is.Not.Null);
            Assert.That(cat.Name, Is.EqualTo("alpha"));
        }

        [Test]
        public void TelemetryEngine_GetCategory_ExistingName_ReturnsSameInstance()
        {
            var engine = new TelemetryEngine();
            var cat1 = engine.GetCategory("beta");
            var cat2 = engine.GetCategory("beta");
            Assert.That(cat1, Is.SameAs(cat2));
        }

        [Test]
        public void TelemetryEngine_CategoryDictionary_ContainsAddedCategories()
        {
            var engine = new TelemetryEngine();
            engine.GetCategory("x");
            engine.GetCategory("y");
            Assert.That(engine.CategoryDictionary, Does.ContainKey("x"));
            Assert.That(engine.CategoryDictionary, Does.ContainKey("y"));
        }

        [Test]
        public void TelemetryEngine_Categories_EnumeratesAllAdded()
        {
            var engine = new TelemetryEngine();
            engine.GetCategory("p");
            engine.GetCategory("q");
            int count = 0;
            foreach (var _ in engine.Categories)
            {
                count++;
            }
            Assert.That(count, Is.EqualTo(2));
        }

        [Test]
        public void TelemetryEngine_DumpTo_WritesExpectedXmlElements()
        {
            var engine = new TelemetryEngine();
            var cat = engine.GetCategory("myCategory");
            var e = new TelemetryEventArgs
            {
                Id = "lock1",
                RequestTime = 1000,
                AcquireTime = 1010,
                ReleaseTime = 2000,
                StackTrace = new StackTrace()
            };
            cat.OnLockReleased(this, e);

            using var sw = new StringWriter();
            engine.DumpTo(sw);
            string xml = sw.ToString();

            Assert.That(xml, Does.Contain("telemetry"));
            Assert.That(xml, Does.Contain("category"));
            Assert.That(xml, Does.Contain("myCategory"));
            Assert.That(xml, Does.Contain("event"));
        }

        // ── TelemetryLock ─────────────────────────────────────────────────────────────────

        [Test]
        public void TelemetryLock_Acquire_NoListener_DoesNotThrow()
        {
            var inner = new MonitorLock();
            var tl = new TelemetryLock(inner);
            Assert.DoesNotThrow(() =>
            {
                using var d = tl.Acquire();
            });
        }

        [Test]
        public void TelemetryLock_Acquire_WithListener_FiresLockReleasedEventOnDispose()
        {
            var inner = new MonitorLock();
            var tl = new TelemetryLock("mylock", inner);
            TelemetryEventArgs captured = null;
            tl.LockReleased += (s, e) => captured = e;

            using (tl.Acquire())
            {
            }

            Assert.That(captured, Is.Not.Null, "LockReleased should have fired.");
            Assert.That(captured.Id, Is.EqualTo("mylock"));
            Assert.That(captured.RequestTime, Is.GreaterThan(0));
            Assert.That(captured.AcquireTime, Is.GreaterThanOrEqualTo(captured.RequestTime));
            Assert.That(captured.ReleaseTime, Is.GreaterThanOrEqualTo(captured.AcquireTime));
        }

        [Test]
        public void TelemetryLock_AcquireWithTimeout_WithListener_FiresEvent()
        {
            var inner = new MonitorLock();
            var tl = new TelemetryLock(inner);
            bool fired = false;
            tl.LockReleased += (s, e) => fired = true;

            using (tl.Acquire(1000L))
            {
            }

            Assert.That(fired, Is.True);
        }

        [Test]
        public void TelemetryLock_ReleaseAcquire_FiresEvent()
        {
            var inner = new MonitorLock();
            var tl = new TelemetryLock(inner);
            bool fired = false;
            tl.LockReleased += (s, e) => fired = true;

            using (tl.Acquire())
            {
                using (tl.ReleaseAcquire())
                {
                }
            }

            Assert.That(fired, Is.True);
        }

        [Test]
        public void TelemetryLock_Release_DoesNotThrow()
        {
            var inner = new MonitorLock();
            var tl = new TelemetryLock(inner);
            using (inner.Acquire())
            {
                Assert.DoesNotThrow(() => tl.Release());
            }
        }

        // ── TelemetryReaderWriterLock ─────────────────────────────────────────────────────

        [Test]
        public void TelemetryReaderWriterLock_AcquireReadLock_FiresReadLockReleasedEvent()
        {
            var inner = new SlimReaderWriterLock(5000);
            var trl = new TelemetryReaderWriterLock(inner);
            bool fired = false;
            trl.ReadLockReleased += (s, e) => fired = true;

            using (trl.AcquireReadLock())
            {
            }

            Assert.That(fired, Is.True, "ReadLockReleased should have fired.");
        }

        [Test]
        public void TelemetryReaderWriterLock_AcquireWriteLock_FiresWriteLockReleasedEvent()
        {
            var inner = new SlimReaderWriterLock(5000);
            var trl = new TelemetryReaderWriterLock(inner);
            bool fired = false;
            trl.WriteLockReleased += (s, e) => fired = true;

            using (trl.AcquireWriteLock())
            {
            }

            Assert.That(fired, Is.True, "WriteLockReleased should have fired.");
        }

        [Test]
        public void TelemetryReaderWriterLock_AcquireWriteLockWithTimeSpan_FiresWriteLockReleasedEvent()
        {
            var inner = new SlimReaderWriterLock(5000);
            var trl = new TelemetryReaderWriterLock(inner);
            bool fired = false;
            trl.WriteLockReleased += (s, e) => fired = true;

            using (trl.AcquireWriteLock(TimeSpan.FromSeconds(5)))
            {
            }

            Assert.That(fired, Is.True);
        }

        [Test]
        public void TelemetryReaderWriterLock_IsWriterLockHeld_DelegatesToSubLock()
        {
            var inner = new SlimReaderWriterLock(5000);
            var trl = new TelemetryReaderWriterLock(inner);

            Assert.That(trl.IsWriterLockHeld, Is.False);
            using (inner.AcquireWriteLock())
            {
                Assert.That(inner.IsWriterLockHeld, Is.True);
            }
        }

        [Test]
        public void TelemetryReaderWriterLock_ReleaseWriteLock_AllowsSubsequentReader()
        {
            var inner = new SlimReaderWriterLock(5000);
            var trl = new TelemetryReaderWriterLock(inner);
            trl.WriteLock.Acquire();
            Assert.DoesNotThrow(() => trl.ReleaseWriteLock());
            Assert.DoesNotThrow(() =>
            {
                using var r = trl.AcquireReadLock();
            });
        }
    }
}
