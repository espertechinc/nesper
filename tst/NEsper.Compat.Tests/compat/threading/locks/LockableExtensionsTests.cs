// Copyright (C) 2006-2024 Esper Team. All rights reserved.
// Subject to the terms of the GPL license (see license.txt).

using System;

using NUnit.Framework;

using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.compat.threading.locks
{
    [TestFixture]
    public class LockableExtensionsTests
    {
        // ── LockableExtensions ────────────────────────────────────────────────────────────

        [Test]
        public void Call_WithVoidLock_ExecutesActionExactlyOnce()
        {
            var vl = new VoidLock();
            int callCount = 0;
            vl.Call(() => callCount++);
            Assert.That(callCount, Is.EqualTo(1));
        }

        [Test]
        public void Call_WithMonitorLock_ExecutesActionExactlyOnce()
        {
            var ml = new MonitorLock();
            int callCount = 0;
            ml.Call(() => callCount++);
            Assert.That(callCount, Is.EqualTo(1));
        }

        [Test]
        public void CallFunc_WithVoidLock_ReturnsExpectedValue()
        {
            var vl = new VoidLock();
            int result = vl.Call(() => 42);
            Assert.That(result, Is.EqualTo(42));
        }

        [Test]
        public void CallFunc_WithMonitorLock_ReturnsExpectedValue()
        {
            var ml = new MonitorLock();
            string result = ml.Call(() => "hello");
            Assert.That(result, Is.EqualTo("hello"));
        }

        [Test]
        public void Call_LockIsReleasedAfterAction()
        {
            var ml = new MonitorLock(500);
            ml.Call(() => { });
            Assert.DoesNotThrow(() => ml.Call(() => { }));
        }

        [Test]
        public void CallFunc_LockIsReleasedAfterFunction()
        {
            var ml = new MonitorLock(500);
            ml.Call(() => 1);
            Assert.DoesNotThrow(() => ml.Call(() => 2));
        }

        // ── LockException ─────────────────────────────────────────────────────────────────

        [Test]
        public void LockException_DefaultConstructor_CreatesInstance()
        {
            var ex = new LockException();
            Assert.That(ex, Is.Not.Null);
        }

        [Test]
        public void LockException_MessageConstructor_PreservesMessage()
        {
            var ex = new LockException("test message");
            Assert.That(ex.Message, Is.EqualTo("test message"));
        }

        [Test]
        public void LockException_InnerExceptionConstructor_PreservesBoth()
        {
            var inner = new Exception("inner");
            var ex = new LockException("outer", inner);
            Assert.That(ex.Message, Is.EqualTo("outer"));
            Assert.That(ex.InnerException, Is.SameAs(inner));
        }

        [Test]
        public void LockException_CanBeThrownAndCaught()
        {
            Assert.Throws<LockException>(() => throw new LockException("thrown"));
        }
    }
}
