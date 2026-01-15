using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

namespace com.espertech.esper.compat.function
{
    [TestFixture]
    public class MemoizedSupplierTests
    {
        [Test]
        public void Memoize_CallsSupplierOnceAndCachesValue()
        {
            var callCount = 0;
            Supplier<string> supplier = () => {
                Interlocked.Increment(ref callCount);
                return Guid.NewGuid().ToString("N");
            };

            var memoized = Suppliers.Memoize(supplier);

            var v1 = memoized();
            var v2 = memoized();
            var v3 = memoized();

            Assert.That(v1, Is.EqualTo(v2));
            Assert.That(v2, Is.EqualTo(v3));
            Assert.That(callCount, Is.EqualTo(1));
        }

        [Test]
        public void Memoize_IsThreadSafeAndComputesAtMostOnce()
        {
            var callCount = 0;
            var gate = new ManualResetEventSlim(false);

            Supplier<string> supplier = () => {
                Interlocked.Increment(ref callCount);
                gate.Wait(1000);
                return "value";
            };

            var memoized = Suppliers.Memoize(supplier);

            var tasks = Enumerable.Range(0, 16)
                .Select(_ => Task.Run(() => memoized()))
                .ToArray();

            gate.Set();

            Task.WaitAll(tasks, 2000);

            Assert.That(tasks.All(t => t.IsCompleted), Is.True);
            Assert.That(tasks.Select(t => t.Result).Distinct().Single(), Is.EqualTo("value"));

            Assert.That(callCount, Is.EqualTo(1));
        }

        [Test]
        public void Memoize_CachesNullValue()
        {
            var callCount = 0;
            Supplier<string> supplier = () => {
                Interlocked.Increment(ref callCount);
                return null;
            };

            var memoized = Suppliers.Memoize(supplier);

            Assert.That(memoized(), Is.Null);
            Assert.That(memoized(), Is.Null);
            Assert.That(callCount, Is.EqualTo(1));
        }
    }
}
