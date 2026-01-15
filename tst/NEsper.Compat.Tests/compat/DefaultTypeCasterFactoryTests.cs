using System;

using NUnit.Framework;

namespace com.espertech.esper.compat
{
    [TestFixture]
    public class DefaultTypeCasterFactoryTests
    {
        private class Base
        {
            public string Value { get; set; }
        }

        private sealed class Derived : Base
        {
        }

        [Test]
        public void GetTrueType_UnwrapsNullable()
        {
            Assert.That(DefaultTypeCasterFactory.GetTrueType(typeof(int?)), Is.EqualTo(typeof(int)));
            Assert.That(DefaultTypeCasterFactory.GetTrueType(typeof(string)), Is.EqualTo(typeof(string)));
        }

        [Test]
        public void GetTypeCaster_CachesByTypePair()
        {
            var factory = new DefaultTypeCasterFactory();

            var c1 = factory.GetTypeCaster(typeof(int), typeof(long));
            var c2 = factory.GetTypeCaster(typeof(int), typeof(long));

            Assert.That(c1, Is.SameAs(c2));
        }

        [Test]
        public void GetTypeCaster_IdentityForSameType()
        {
            var factory = new DefaultTypeCasterFactory();
            var caster = factory.GetTypeCaster(typeof(int), typeof(int));

            var boxed = (object) 10;
            Assert.That(caster(boxed), Is.SameAs(boxed));
        }

        [Test]
        public void GetTypeCaster_IdentityForAssignableTypes()
        {
            var factory = new DefaultTypeCasterFactory();
            var caster = factory.GetTypeCaster(typeof(Derived), typeof(Base));

            var d = new Derived { Value = "x" };
            Assert.That(caster(d), Is.SameAs(d));
        }

        [Test]
        public void GetTypeCaster_StringConversion_UsesToString_AndNullPassthrough()
        {
            var factory = new DefaultTypeCasterFactory();
            var caster = factory.GetTypeCaster(typeof(int), typeof(string));

            Assert.That(caster(10), Is.EqualTo("10"));
            Assert.That(caster(null), Is.Null);
        }

        [Test]
        public void GetTypeCaster_NullInputReturnsNull()
        {
            var factory = new DefaultTypeCasterFactory();
            var caster = factory.GetTypeCaster(typeof(int), typeof(long));

            Assert.That(caster(null), Is.Null);
        }

        [Test]
        public void GetTypeCaster_CheckedNumericCast_ThrowsOnOverflow()
        {
            var factory = new DefaultTypeCasterFactory();
            var caster = factory.GetTypeCaster(typeof(long), typeof(int));

            Assert.Throws<OverflowException>(() => caster((long)int.MaxValue + 1));
        }

        [Test]
        public void GetTypeCaster_UsesUnderlyingTypesForNullablePairs()
        {
            var factory = new DefaultTypeCasterFactory();

            var casterNullable = factory.GetTypeCaster(typeof(int?), typeof(long?));
            var casterNonNullable = factory.GetTypeCaster(typeof(int), typeof(long));

            Assert.That(casterNullable, Is.SameAs(casterNonNullable));
            Assert.That(casterNullable(10), Is.EqualTo(10L));
        }
    }
}
