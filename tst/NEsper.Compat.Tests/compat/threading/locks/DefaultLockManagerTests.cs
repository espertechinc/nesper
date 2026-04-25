// Copyright (C) 2006-2024 Esper Team. All rights reserved.
// Subject to the terms of the GPL license (see license.txt).

using System;

using NUnit.Framework;

using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.compat.threading.locks
{
    [TestFixture]
    public class DefaultLockManagerTests
    {
        // ── CreateLock(Type) ──────────────────────────────────────────────────────────────

        [Test]
        public void CreateLock_ByType_UsesDefaultFactory()
        {
            var manager = new DefaultLockManager(timeout => new MonitorLock(timeout));
            var lck = manager.CreateLock(typeof(string));
            Assert.That(lck, Is.Not.Null);
            Assert.That(lck, Is.InstanceOf<MonitorLock>());
        }

        [Test]
        public void CreateLock_ByType_WithRegisteredCategory_UsesRegisteredFactory()
        {
            var manager = new DefaultLockManager(timeout => new MonitorLock(timeout));
            int factoryCalls = 0;
            manager.RegisterCategoryLock(typeof(string), timeout =>
            {
                factoryCalls++;
                return new MonitorSlimLock(timeout);
            });

            var lck = manager.CreateLock(typeof(string));
            Assert.That(factoryCalls, Is.EqualTo(1));
            Assert.That(lck, Is.InstanceOf<MonitorSlimLock>());
        }

        // ── CreateLock(Func<int, ILockable>) ─────────────────────────────────────────────

        [Test]
        public void CreateLock_WithFactory_UsesSuppliedFactory()
        {
            var manager = new DefaultLockManager(timeout => new MonitorLock(timeout));
            int factoryCalls = 0;

            var lck = manager.CreateLock(timeout =>
            {
                factoryCalls++;
                return new MonitorSlimLock(timeout);
            });

            Assert.That(factoryCalls, Is.EqualTo(1));
            Assert.That(lck, Is.InstanceOf<MonitorSlimLock>());
        }

        // ── RegisterCategoryLock(Type, ...) ───────────────────────────────────────────────

        [Test]
        public void RegisterCategoryLock_ByType_ResolvesOnCreateLockByType()
        {
            var manager = new DefaultLockManager(timeout => new MonitorLock(timeout));
            manager.RegisterCategoryLock(typeof(int), timeout => new MonitorSlimLock(timeout));
            var lck = manager.CreateLock(typeof(int));
            Assert.That(lck, Is.InstanceOf<MonitorSlimLock>());
        }

        // ── Category prefix fallback algorithm ────────────────────────────────────────────

        [Test]
        public void CreateLock_CategoryPrefixFallback_MatchesRegisteredPrefix()
        {
            var manager = new DefaultLockManager(timeout => new MonitorLock(timeout));
            manager.RegisterCategoryLock("com.example", timeout => new MonitorSlimLock(timeout));
            var lck = manager.CreateLock("com.example.Service");
            Assert.That(lck, Is.InstanceOf<MonitorSlimLock>(),
                "Should fall back to the 'com.example' factory for a sub-category.");
        }

        [Test]
        public void CreateLock_CategoryPrefixFallback_NoMatch_UsesDefaultFactory()
        {
            var manager = new DefaultLockManager(timeout => new MonitorLock(timeout));
            manager.RegisterCategoryLock("other.ns", timeout => new MonitorSlimLock(timeout));
            var lck = manager.CreateLock("com.example.Service");
            Assert.That(lck, Is.InstanceOf<MonitorLock>(),
                "Should use the default factory when no prefix matches.");
        }

        // ── IsTelemetryEnabled wrapping ───────────────────────────────────────────────────

        [Test]
        public void CreateDefaultLock_WithTelemetryEnabled_ReturnsTelemetryLock()
        {
            var manager = new DefaultLockManager(timeout => new MonitorLock(timeout));
            manager.IsTelemetryEnabled = true;
            var lck = manager.CreateDefaultLock();
            Assert.That(lck, Is.InstanceOf<TelemetryLock>(),
                "Should wrap the lock in a TelemetryLock when telemetry is enabled.");
        }

        [Test]
        public void CreateLock_WithFactory_TelemetryEnabled_ReturnsTelemetryLock()
        {
            var manager = new DefaultLockManager(timeout => new MonitorLock(timeout));
            manager.IsTelemetryEnabled = true;
            var lck = manager.CreateLock(timeout => new MonitorSlimLock(timeout));
            Assert.That(lck, Is.InstanceOf<TelemetryLock>());
        }

        [Test]
        public void CreateLock_ByType_TelemetryEnabled_ReturnsTelemetryLock()
        {
            var manager = new DefaultLockManager(timeout => new MonitorLock(timeout));
            manager.IsTelemetryEnabled = true;
            var lck = manager.CreateLock(typeof(string));
            Assert.That(lck, Is.InstanceOf<TelemetryLock>());
        }
    }
}
