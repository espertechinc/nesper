using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

namespace com.espertech.esper.compat
{
    [TestFixture]
    public class AtomicTests
    {
        [Test]
        public void AtomicBoolean_GetSetRoundTrip()
        {
            var ab = new AtomicBoolean();
            Assert.That(ab.Get(), Is.False);

            ab.Set(true);
            Assert.That(ab.Get(), Is.True);

            ab.Set(false);
            Assert.That(ab.Get(), Is.False);
        }

        [Test]
        public void AtomicLong_GetSetAndIncrementDecrement()
        {
            var al = new AtomicLong(10);

            Assert.That(al.Get(), Is.EqualTo(10));
            al.Set(11);
            Assert.That(al.Get(), Is.EqualTo(11));

            Assert.That(al.GetAndIncrement(), Is.EqualTo(11));
            Assert.That(al.Get(), Is.EqualTo(12));

            Assert.That(al.IncrementAndGet(), Is.EqualTo(13));
            Assert.That(al.DecrementAndGet(), Is.EqualTo(12));
            Assert.That(al.IncrementAndGet(5), Is.EqualTo(17));
            Assert.That(al.DecrementAndGet(7), Is.EqualTo(10));

            Assert.That(al.GetAndSet(123), Is.EqualTo(10));
            Assert.That(al.Get(), Is.EqualTo(123));
        }

        [Test]
        public void AtomicLong_CompareAndSet_SucceedsAndFails()
        {
            var al = new AtomicLong(1);

            Assert.That(al.CompareAndSet(1, 2), Is.True);
            Assert.That(al.Get(), Is.EqualTo(2));

            // wrong expected value should not change
            Assert.That(al.CompareAndSet(1, 3), Is.False);
            Assert.That(al.Get(), Is.EqualTo(2));
        }

        [Test]
        public void AtomicLong_IncrementAndGet_IsThreadSafe()
        {
            var al = new AtomicLong(0);
            var threads = 8;
            var perThread = 50_000;

            Parallel.For(0, threads, _ => {
                for (var ii = 0; ii < perThread; ii++) {
                    al.IncrementAndGet();
                }
            });

            Assert.That(al.Get(), Is.EqualTo((long)threads * perThread));
        }

        [Test]
        public void AtomicReference_SetIsVisible()
        {
            var atomic = new Atomic<string>("a");
            Assert.That(atomic.Get(), Is.EqualTo("a"));

            atomic.Set("b");
            Assert.That(atomic.Value, Is.EqualTo("b"));
        }

        [Test]
        public void AtomicReference_SetIsAtomicAcrossThreads()
        {
            var atomic = new Atomic<string>("start");

            var ready = new ManualResetEventSlim(false);
            var done = new ManualResetEventSlim(false);

            var t = new Thread(() => {
                ready.Set();
                atomic.Set("changed");
                done.Set();
            });
            t.Start();

            Assert.That(ready.Wait(1000), Is.True);
            Assert.That(done.Wait(1000), Is.True);

            Assert.That(atomic.Get(), Is.EqualTo("changed"));
        }
    }
}
