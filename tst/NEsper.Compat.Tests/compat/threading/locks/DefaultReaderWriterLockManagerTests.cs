// Copyright (C) 2006-2024 Esper Team. All rights reserved.
// Subject to the terms of the GPL license (see license.txt).

using System;

using NUnit.Framework;

using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.compat.threading.locks
{
    /// <summary>
    /// Tests for Task 1.3a: DefaultReaderWriterLockManager.CreateDefaultLock throws
    /// ApplicationException instead of InvalidOperationException when the factory is null.
    ///
    /// A missing factory is a programming error (misconfiguration), not a runtime fault.
    /// The correct exception type is InvalidOperationException.
    ///
    /// STATUS: Tests currently FAIL because the method still throws ApplicationException.
    ///         They will PASS once CreateDefaultLock(string) is changed to throw
    ///         InvalidOperationException.
    ///
    /// For comparison, DefaultLockManager already throws InvalidOperationException (Task 1.3,
    /// already fixed). Those tests are included here to document the expectation and prevent
    /// regression.
    /// </summary>
    [TestFixture]
    public class DefaultReaderWriterLockManagerTests
    {
        // ── DefaultReaderWriterLockManager (Task 1.3a — CURRENTLY FAILING) ──────────────

        [Test]
        public void CreateDefaultLock_WithNullFactory_ThrowsInvalidOperationException()
        {
            // FAILS NOW: throws ApplicationException
            // PASSES AFTER FIX: change throw to InvalidOperationException
            var manager = new DefaultReaderWriterLockManager(null);

            Assert.Throws<InvalidOperationException>(
                () => manager.CreateDefaultLock(),
                "Missing factory is a configuration error; ApplicationException is the wrong type.");
        }

        [Test]
        public void CreateDefaultLock_WithCategory_WithNullFactory_ThrowsInvalidOperationException()
        {
            // CreateDefaultLock(string) is the internal path; CreateDefaultLock() delegates to it.
            // FAILS NOW: same ApplicationException throw site.
            var manager = new DefaultReaderWriterLockManager(null);

            // Any category that has no registered factory falls back to CreateDefaultLock(category).
            Assert.Throws<InvalidOperationException>(
                () => manager.CreateLock("some.unregistered.Category"),
                "Category miss-fallback must raise InvalidOperationException, not ApplicationException.");
        }

        [Test]
        public void CreateLock_WithNullCategory_WithNullFactory_ThrowsInvalidOperationException()
        {
            // Null category skips the lookup and goes straight to CreateDefaultLock.
            var manager = new DefaultReaderWriterLockManager(null);

            Assert.Throws<InvalidOperationException>(
                () => manager.CreateLock((string)null),
                "Null category must fall back to CreateDefaultLock and raise InvalidOperationException.");
        }

        // ── Positive cases: factory is set, no exception expected ────────────────────────

        [Test]
        public void CreateDefaultLock_WithFactory_Succeeds()
        {
            var manager = new DefaultReaderWriterLockManager(timeout => new SlimReaderWriterLock(timeout));
            var rwLock = manager.CreateDefaultLock();
            Assert.That(rwLock, Is.Not.Null);
        }

        [Test]
        public void CreateLock_WithCategory_WithFactory_Succeeds()
        {
            var manager = new DefaultReaderWriterLockManager(timeout => new SlimReaderWriterLock(timeout));
            var rwLock = manager.CreateLock("some.Category");
            Assert.That(rwLock, Is.Not.Null);
        }

        [Test]
        public void CreateLock_WithRegisteredCategoryFactory_UsesRegisteredFactory()
        {
            int factoryCalls = 0;
            var manager = new DefaultReaderWriterLockManager(timeout => new SlimReaderWriterLock(timeout));
            manager.RegisterCategoryLock("my.category", timeout =>
            {
                factoryCalls++;
                return new SlimReaderWriterLock(timeout);
            });

            manager.CreateLock("my.category");
            Assert.That(factoryCalls, Is.EqualTo(1), "Registered factory should have been called.");
        }

        // ── DefaultLockManager baseline (Task 1.3 — already fixed, regression guard) ───

        [Test]
        public void DefaultLockManager_CreateDefaultLock_WithNullFactory_ThrowsInvalidOperationException()
        {
            // Already throws InvalidOperationException — verify this doesn't regress.
            var manager = new DefaultLockManager(null);
            Assert.Throws<InvalidOperationException>(() => manager.CreateDefaultLock());
        }

        [Test]
        public void DefaultLockManager_CreateLock_WithNullFactory_ThrowsInvalidOperationException()
        {
            var manager = new DefaultLockManager(null);
            Assert.Throws<InvalidOperationException>(() => manager.CreateLock("some.Category"));
        }

        // ── Additional DefaultReaderWriterLockManager coverage (Phase 7b) ────────────────

        [Test]
        public void RegisterCategoryLock_GenericOverload_UsesTypeFullName()
        {
            int factoryCalls = 0;
            var manager = new DefaultReaderWriterLockManager(timeout => new SlimReaderWriterLock(timeout));
            manager.RegisterCategoryLock<SlimReaderWriterLock>(timeout =>
            {
                factoryCalls++;
                return new FairReaderWriterLock(timeout);
            });

            manager.CreateLock(typeof(SlimReaderWriterLock));
            Assert.That(factoryCalls, Is.EqualTo(1), "Generic RegisterCategoryLock<T> should register by typeof(T).FullName.");
        }

        [Test]
        public void RegisterCategoryLock_TypeOverload_UsesTypeFullName()
        {
            int factoryCalls = 0;
            var manager = new DefaultReaderWriterLockManager(timeout => new SlimReaderWriterLock(timeout));
            manager.RegisterCategoryLock(typeof(FairReaderWriterLock), timeout =>
            {
                factoryCalls++;
                return new SlimReaderWriterLock(timeout);
            });

            manager.CreateLock(typeof(FairReaderWriterLock));
            Assert.That(factoryCalls, Is.EqualTo(1), "RegisterCategoryLock(Type, ...) should register by type.FullName.");
        }

        [Test]
        public void CreateLock_ByType_NonGeneric_UsesDefaultFactory()
        {
            var manager = new DefaultReaderWriterLockManager(timeout => new SlimReaderWriterLock(timeout));
            var lck = manager.CreateLock(typeof(SlimReaderWriterLock));
            Assert.That(lck, Is.Not.Null);
            Assert.That(lck, Is.InstanceOf<SlimReaderWriterLock>());
        }

        [Test]
        public void CreateLock_ByType_GenericType_BacktickSuffixIsStripped()
        {
            var manager = new DefaultReaderWriterLockManager(timeout => new SlimReaderWriterLock(timeout));
            int factoryCalls = 0;
            // Register the stripped name (without `1 suffix)
            string strippedName = typeof(System.Collections.Generic.List<int>).FullName;
            int backtickIdx = strippedName.IndexOf('`');
            if (backtickIdx != -1)
            {
                strippedName = strippedName.Substring(0, backtickIdx);
            }
            manager.RegisterCategoryLock(strippedName, timeout =>
            {
                factoryCalls++;
                return new FairReaderWriterLock(timeout);
            });

            manager.CreateLock(typeof(System.Collections.Generic.List<int>));
            Assert.That(factoryCalls, Is.EqualTo(1), "Generic type name backtick suffix should be stripped before category lookup.");
        }

        [Test]
        public void CreateLock_WithFactory_UsesSuppliedFactory()
        {
            var manager = new DefaultReaderWriterLockManager(timeout => new SlimReaderWriterLock(timeout));
            int factoryCalls = 0;
            var lck = manager.CreateLock(timeout =>
            {
                factoryCalls++;
                return new FairReaderWriterLock(timeout);
            });

            Assert.That(factoryCalls, Is.EqualTo(1));
            Assert.That(lck, Is.InstanceOf<FairReaderWriterLock>());
        }

        [Test]
        public void CreateLock_CategoryPrefixFallback_MatchesRegisteredPrefix()
        {
            var manager = new DefaultReaderWriterLockManager(timeout => new SlimReaderWriterLock(timeout));
            manager.RegisterCategoryLock("com.example", timeout => new FairReaderWriterLock(timeout));
            var lck = manager.CreateLock("com.example.Service");
            Assert.That(lck, Is.InstanceOf<FairReaderWriterLock>(),
                "Should fall back to the 'com.example' factory for a sub-category.");
        }

        [Test]
        public void CreateLock_CategoryPrefixFallback_NoMatch_UsesDefaultFactory()
        {
            var manager = new DefaultReaderWriterLockManager(timeout => new SlimReaderWriterLock(timeout));
            manager.RegisterCategoryLock("other.ns", timeout => new FairReaderWriterLock(timeout));
            var lck = manager.CreateLock("com.example.Service");
            Assert.That(lck, Is.InstanceOf<SlimReaderWriterLock>(),
                "Should use the default factory when no prefix matches.");
        }

        [Test]
        public void CreateDefaultLock_WithTelemetryEnabled_ReturnsTelemetryReaderWriterLock()
        {
            var manager = new DefaultReaderWriterLockManager(timeout => new SlimReaderWriterLock(timeout));
            manager.IsTelemetryEnabled = true;
            var lck = manager.CreateDefaultLock();
            Assert.That(lck, Is.InstanceOf<TelemetryReaderWriterLock>(),
                "Should wrap the lock in a TelemetryReaderWriterLock when telemetry is enabled.");
        }

        [Test]
        public void CreateLock_WithFactory_TelemetryEnabled_ReturnsTelemetryReaderWriterLock()
        {
            var manager = new DefaultReaderWriterLockManager(timeout => new SlimReaderWriterLock(timeout));
            manager.IsTelemetryEnabled = true;
            var lck = manager.CreateLock(timeout => new FairReaderWriterLock(timeout));
            Assert.That(lck, Is.InstanceOf<TelemetryReaderWriterLock>());
        }

        [Test]
        public void CreateDefaultLock_WithTelemetryEnabled_EventsFire()
        {
            var manager = new DefaultReaderWriterLockManager(timeout => new SlimReaderWriterLock(timeout));
            manager.IsTelemetryEnabled = true;
            var lck = (TelemetryReaderWriterLock)manager.CreateDefaultLock();

            bool readFired = false;
            bool writeFired = false;
            lck.ReadLockReleased += (s, e) => readFired = true;
            lck.WriteLockReleased += (s, e) => writeFired = true;

            using (lck.AcquireReadLock()) { }
            using (lck.AcquireWriteLock()) { }

            Assert.That(readFired, Is.True);
            Assert.That(writeFired, Is.True);
        }
    }
}
