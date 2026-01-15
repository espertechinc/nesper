using System;
using System.Diagnostics;
using System.Threading;

using NUnit.Framework;

namespace com.espertech.esper.compat.collections
{
    [TestFixture]
    public class BlockingQueueTests
    {
        [Test]
        public void LinkedBlockingQueue_PushThenPop()
        {
            var q = new LinkedBlockingQueue<int>();
            q.Push(10);
            Assert.That(q.Pop(), Is.EqualTo(10));
        }

        [Test]
        public void LinkedBlockingQueue_PopWithTimeoutTimesOut()
        {
            var q = new LinkedBlockingQueue<int>();

            var sw = Stopwatch.StartNew();
            var result = q.Pop(50, out var item);
            sw.Stop();

            Assert.That(result, Is.False);
            Assert.That(item, Is.EqualTo(default(int)));
            Assert.That(sw.ElapsedMilliseconds, Is.GreaterThanOrEqualTo(40));
        }

        [Test]
        public void BoundBlockingQueue_RespectsCapacity()
        {
            var q = new BoundBlockingQueue<int>(maxCapacity: 1, lockTimeout: 60000);
            q.Push(1);

            var pushedSecond = false;
            var t = new Thread(() => {
                q.Push(2);
                pushedSecond = true;
            });

            t.Start();

            // Give the producer time to block
            Thread.Sleep(50);
            Assert.That(pushedSecond, Is.False);

            // Pop should unblock producer
            Assert.That(q.Pop(), Is.EqualTo(1));

            t.Join(1000);
            Assert.That(pushedSecond, Is.True);
            Assert.That(q.Pop(), Is.EqualTo(2));
        }

        [Test]
        public void ImperfectBlockingQueue_PushThenPop()
        {
            var q = new ImperfectBlockingQueue<int>();
            q.Push(10);
            Assert.That(q.Pop(), Is.EqualTo(10));
        }

        [Test]
        public void ImperfectBlockingQueue_PopWithTimeoutTimesOut()
        {
            var q = new ImperfectBlockingQueue<int>();
            var result = q.Pop(50, out var item);

            Assert.That(result, Is.False);
            Assert.That(item, Is.EqualTo(default(int)));
        }
    }
}
