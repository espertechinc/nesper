using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;

using NUnit.Framework;

namespace com.espertech.esper.compat
{
    [TestFixture]
    public class TypeExtensionsTest
    {
        private class Sample
        {
            public int Value { get; set; }
        }

        [Test]
        public void FindProperty_FindsCaseInsensitiveProperty()
        {
            var type = typeof(Sample);

            PropertyInfo prop1 = type.FindProperty("Value");
            PropertyInfo prop2 = type.FindProperty("value");

            Assert.That(prop1, Is.Not.Null);
            Assert.That(prop2, Is.Not.Null);
            Assert.That(prop1, Is.EqualTo(prop2));
        }

        [Test]
        public void AsSingleton_WrapsValueInArray()
        {
            var result = 42.AsSingleton();
            Assert.That(result, Is.EqualTo(new[] { 42 }));
        }

        [Test]
        public void AsSet_CreatesSingletonSet()
        {
            var set = 7.AsSet();
            Assert.That(set.Count, Is.EqualTo(1));
            Assert.That(set.Contains(7), Is.True);
        }

        [Test]
        public void AsInt32_HandlesVariousNumericTypes()
        {
            Assert.That(((object)1).AsInt32(), Is.EqualTo(1));
            Assert.That(((object)(short)2).AsInt32(), Is.EqualTo(2));
            Assert.That(((object)(byte)3).AsInt32(), Is.EqualTo(3));
            Assert.That(((object)4L).AsInt32(), Is.EqualTo(4));
        }

        [Test]
        public void AsInt64_HandlesVariousNumericTypes()
        {
            Assert.That(((object)1L).AsInt64(), Is.EqualTo(1L));
            Assert.That(((object)2).AsInt64(), Is.EqualTo(2L));
            Assert.That(((object)(short)3).AsInt64(), Is.EqualTo(3L));
        }

        [Test]
        public void AsFloat_HandlesIntegralAndBigInteger()
        {
            Assert.That(((object)1).AsFloat(), Is.EqualTo(1f));
            Assert.That(((object)2L).AsFloat(), Is.EqualTo(2f));
            Assert.That(((object)new BigInteger(3)).AsFloat(), Is.EqualTo(3f));
        }

        [Test]
        public void IsInt32_UpcastAndNoUpcast()
        {
            object shortValue = (short)1;
            object intValue = 2;
            object longValue = 3L;

            Assert.That(shortValue.IsInt32(withUpcast: true), Is.True);
            Assert.That(shortValue.IsInt32(withUpcast: false), Is.False);
            Assert.That(intValue.IsInt32(), Is.True);
            Assert.That(longValue.IsInt32(), Is.False);
        }

        [Test]
        public void IsDateTime_DetectsRelevantTypes()
        {
            Assert.That(typeof(DateTime).IsDateTime(), Is.True);
            Assert.That(typeof(DateTime?).IsDateTime(), Is.True);
            Assert.That(typeof(DateTimeOffset?).IsDateTime(), Is.True);
            Assert.That(typeof(long?).IsDateTime(), Is.True);
            Assert.That(typeof(string).IsDateTime(), Is.False);
        }

        [Test]
        public void CleanName_FormatsGenericsAndArrays()
        {
            Assert.That(typeof(int).CleanName(), Is.EqualTo(typeof(int).FullName));
            Assert.That(typeof(List<string>).CleanName(useFullName: false), Is.EqualTo("List<String>"));
            Assert.That(typeof(int[]).CleanName(useFullName: false), Is.EqualTo("System.Int32[]"));
        }

        [Test]
        public void GetDefaultValue_WorksForValueAndReferenceTypes()
        {
            Assert.That(TypeExtensions.GetDefaultValue(typeof(int)), Is.EqualTo(0));
            Assert.That(TypeExtensions.GetDefaultValue(typeof(int?)), Is.Null);
            Assert.That(TypeExtensions.GetDefaultValue(typeof(string)), Is.Null);
        }
    }
}
