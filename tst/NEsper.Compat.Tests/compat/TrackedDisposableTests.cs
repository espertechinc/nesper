// Copyright (C) 2006-2024 Esper Team. All rights reserved.
// Subject to the terms of the GPL license (see license.txt).

using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

namespace com.espertech.esper.compat
{
    /// <summary>
    /// Tests for Task 1.1: TrackedDisposable double-dispose guard.
    ///
    /// The IDisposable contract requires that calling Dispose() more than once is safe.
    /// Previously, TrackedDisposable invoked the action on every call. For locks, a second
    /// Dispose() would attempt a second release — either throwing SynchronizationLockException
    /// or silently corrupting state.
    ///
    /// FIX: Dispose() uses Interlocked.Exchange(ref _actionOnDispose, null)?.Invoke(), which
    /// atomically swaps the field to null and invokes only if it was non-null. This ensures
    /// the action runs exactly once and releases the closure reference after first dispose,
    /// allowing the GC to collect whatever the action captured.
    /// </summary>
    [TestFixture]
    public class TrackedDisposableTests
    {
        [Test]
        public void Dispose_InvokesAction_OnFirstCall()
        {
            int callCount = 0;
            var td = new TrackedDisposable(() => callCount++);

            td.Dispose();

            Assert.That(callCount, Is.EqualTo(1),
                "Action must be invoked exactly once on first Dispose().");
        }

        [Test]
        public void Dispose_DoesNotInvokeAction_OnSubsequentCalls()
        {
            int callCount = 0;
            var td = new TrackedDisposable(() => callCount++);

            td.Dispose();
            td.Dispose();
            td.Dispose();

            Assert.That(callCount, Is.EqualTo(1),
                "Action must not be invoked on subsequent Dispose() calls (double-dispose guard).");
        }

        [Test]
        public void Dispose_NeverCalled_ActionIsNeverInvoked()
        {
            int callCount = 0;
            // ReSharper disable once UnusedVariable
            var td = new TrackedDisposable(() => callCount++);

            // td goes out of scope without Dispose() — action must remain un-invoked.
            Assert.That(callCount, Is.EqualTo(0),
                "Action must not be invoked if Dispose() is never called.");
        }

        [Test]
        public void Dispose_UsingBlock_InvokesActionExactlyOnce()
        {
            int callCount = 0;

            using (new TrackedDisposable(() => callCount++))
            {
                // no-op inside the using block
            }

            Assert.That(callCount, Is.EqualTo(1),
                "Action must be invoked exactly once when used with a 'using' statement.");
        }

        [Test]
        public void Dispose_ConcurrentCalls_InvokesActionExactlyOnce()
        {
            // Verifies that Interlocked.Exchange makes the guard thread-safe:
            // when many threads race to dispose the same instance, the action fires once.
            const int threadCount = 32;
            int callCount = 0;
            var td = new TrackedDisposable(() => Interlocked.Increment(ref callCount));

            var barrier = new Barrier(threadCount);
            var tasks = new Task[threadCount];
            for (int i = 0; i < threadCount; i++)
            {
                tasks[i] = Task.Run(() =>
                {
                    barrier.SignalAndWait(); // all threads start simultaneously
                    td.Dispose();
                });
            }

            Task.WaitAll(tasks);

            Assert.That(callCount, Is.EqualTo(1),
                "Action must be invoked exactly once even when Dispose() is called concurrently from many threads.");
        }

        [Test]
        public void Dispose_NullAction_DoesNotThrow()
        {
            // Edge case: consumer passes null. Dispose() should not throw NRE.
            var td = new TrackedDisposable(null);

            Assert.DoesNotThrow(() => td.Dispose(),
                "Dispose() with a null action must not throw.");
            Assert.DoesNotThrow(() => td.Dispose(),
                "Second Dispose() with a null action must also not throw.");
        }
    }
}
