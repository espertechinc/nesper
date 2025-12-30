using System;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

namespace com.espertech.esper.compat.threading
{
    [TestFixture]
    public class CountDownLatchTests
    {
        [Test]
        public void Count_ReflectsCountDown()
        {
            var latch = new CountDownLatch(2);
            Assert.That(latch.Count, Is.EqualTo(2));

            latch.CountDown();
            Assert.That(latch.Count, Is.EqualTo(1));

            latch.CountDown();
            Assert.That(latch.Count, Is.EqualTo(0));
        }

        [Test]
        public void Await_BlocksUntilCountReachesZero()
        {
            var latch = new CountDownLatch(1);
            var started = new ManualResetEventSlim(false);

            var t = Task.Run(() => {
                started.Set();
                return latch.Await(TimeSpan.FromSeconds(2));
            });

            Assert.That(started.Wait(1000), Is.True);

            // Ensure it doesn't complete immediately.
            Thread.Sleep(25);
            Assert.That(t.IsCompleted, Is.False);

            latch.CountDown();

            Assert.That(t.Wait(1000), Is.True);
            Assert.That(t.Result, Is.True);
        }

        [Test]
        public void Await_WithTimeout_ReturnsFalseWhenNotReleased()
        {
            var latch = new CountDownLatch(1);

            var result = latch.Await(TimeSpan.FromMilliseconds(50));

            Assert.That(result, Is.False);
            Assert.That(latch.Count, Is.EqualTo(1));
        }

        [Test]
        public void Await_ReturnsImmediatelyWhenAlreadyReleased()
        {
            var latch = new CountDownLatch(0);
            Assert.That(latch.Await(TimeSpan.FromMilliseconds(1)), Is.True);
            Assert.That(latch.Await(), Is.True);
        }

        [Test]
        public void CountDown_CanBeCalledMoreTimesThanInitialCount()
        {
            var latch = new CountDownLatch(1);
            latch.CountDown();
            latch.CountDown();

            // Implementation decrements past 0; Await should still return immediately.
            Assert.That(latch.Count, Is.LessThanOrEqualTo(0));
            Assert.That(latch.Await(TimeSpan.FromMilliseconds(1)), Is.True);
        }
    }
}
