using System;
using System.Numerics;

using NUnit.Framework;

namespace com.espertech.esper.compat.util
{
    [TestFixture]
    public class CastHelperTests
    {
        private enum TestEnum
        {
            Zero = 0,
            One = 1,
            A = 'A'
        }

        [Test]
        public void CastInt32_FromNumericTypes_TruncatesLikeCSharpCast()
        {
            Assert.That(CastHelper.CastInt32(1L), Is.EqualTo(1));
            Assert.That(CastHelper.CastInt32(1.9d), Is.EqualTo(1));
            Assert.That(CastHelper.CastInt32(1.9f), Is.EqualTo(1));
            Assert.That(CastHelper.CastInt32(1.9m), Is.EqualTo(1));
        }

        [Test]
        public void CastNullableInt32_NullAndInvalidString_ReturnsNull()
        {
            Assert.That(CastHelper.CastNullableInt32(null), Is.Null);
            Assert.That(CastHelper.CastNullableInt32("not-a-number"), Is.Null);
        }

        [Test]
        public void CastInt32_InvalidString_ReturnsDefaultZero()
        {
            Assert.That(CastHelper.CastInt32("not-a-number"), Is.EqualTo(0));
        }

        [Test]
        public void CastByte_NullThrows()
        {
            Assert.Throws<ArgumentException>(() => CastHelper.CastByte(null));
        }

        [Test]
        public void CastEnum_FromIntAndString_FirstChar()
        {
            Assert.That(CastHelper.CastEnum<TestEnum>(1), Is.EqualTo(TestEnum.One));

            Assert.That(CastHelper.CastEnum<TestEnum>("A"), Is.EqualTo(TestEnum.A));
        }

        [Test]
        public void CastEnum_NullReturnsNull_WhenUsingNonGenericOverload()
        {
            Assert.That(CastHelper.CastEnum(typeof(TestEnum), null), Is.Null);
        }

        [Test]
        public void CastEnum_FromOtherNumericTypes()
        {
            Assert.That((TestEnum)CastHelper.CastEnum(typeof(TestEnum), (byte)1), Is.EqualTo(TestEnum.One));
            Assert.That((TestEnum)CastHelper.CastEnum(typeof(TestEnum), (short)1), Is.EqualTo(TestEnum.One));
            Assert.That((TestEnum)CastHelper.CastEnum(typeof(TestEnum), 1L), Is.EqualTo(TestEnum.One));
        }

        [Test]
        public void CastByte_FromHexString()
        {
            Assert.That(CastHelper.CastByte("0x10"), Is.EqualTo((byte)16));
            Assert.That(CastHelper.CastNullableByte("0x10"), Is.EqualTo((byte?)16));
        }

        [Test]
        public void CastInt64_FromStringWithSuffix()
        {
            Assert.That(CastHelper.CastInt64("12L"), Is.EqualTo(12L));
            Assert.That(CastHelper.CastUInt64("12L"), Is.EqualTo(12UL));
        }

        [Test]
        public void CastSingleAndDouble_FromStringWithSuffix()
        {
            Assert.That(CastHelper.CastSingle("1.25f"), Is.EqualTo(1.25f));
            Assert.That(CastHelper.CastDouble("1.25d"), Is.EqualTo(1.25d));
        }

        [Test]
        public void CastDecimal_FromStringWithSuffix()
        {
            Assert.That(CastHelper.CastDecimal("1.25m"), Is.EqualTo(1.25m));
        }

        [Test]
        public void NullableNumericCasts_InvalidStringReturnNull()
        {
            Assert.That(CastHelper.CastNullableInt16("x"), Is.Null);
            Assert.That(CastHelper.CastNullableInt64("x"), Is.Null);
            Assert.That(CastHelper.CastNullableUInt16("x"), Is.Null);
            Assert.That(CastHelper.CastNullableUInt32("x"), Is.Null);
            Assert.That(CastHelper.CastNullableUInt64("x"), Is.Null);
            Assert.That(CastHelper.CastNullableSingle("x"), Is.Null);
            Assert.That(CastHelper.CastNullableDouble("x"), Is.Null);
            Assert.That(CastHelper.CastNullableDecimal("x"), Is.Null);
            Assert.That(CastHelper.CastNullableBigInteger("x"), Is.Null);
        }

        [Test]
        public void NonNullableNumericCasts_InvalidStringReturnDefault()
        {
            Assert.That(CastHelper.CastInt16("x"), Is.EqualTo((short)0));
            Assert.That(CastHelper.CastInt64("x"), Is.EqualTo(0L));
            Assert.That(CastHelper.CastUInt16("x"), Is.EqualTo((ushort)0));
            Assert.That(CastHelper.CastUInt32("x"), Is.EqualTo(0U));
            Assert.That(CastHelper.CastUInt64("x"), Is.EqualTo(0UL));
            Assert.That(CastHelper.CastSingle("x"), Is.EqualTo(0f));
            Assert.That(CastHelper.CastDouble("x"), Is.EqualTo(0d));
            Assert.That(CastHelper.CastDecimal("x"), Is.EqualTo(0m));
            Assert.That(CastHelper.CastBigInteger("x"), Is.EqualTo(new BigInteger(0)));
        }

        [Test]
        public void CastBigInteger_FromVariousNumericTypes()
        {
            Assert.That(CastHelper.CastBigInteger(10), Is.EqualTo(new BigInteger(10)));
            Assert.That(CastHelper.CastBigInteger(10L), Is.EqualTo(new BigInteger(10)));
            Assert.That(CastHelper.CastBigInteger(10.9m), Is.EqualTo(new BigInteger(10)));
        }

        [Test]
        public void CastChar_FromStringTakesFirstCharacter()
        {
            Assert.That(CastHelper.CastChar("Hello"), Is.EqualTo('H'));
            Assert.That(CastHelper.CastNullableChar("Hello"), Is.EqualTo((char?)'H'));
        }

        [Test]
        public void GetCastConverter_NonAssignableReturnsNull()
        {
            var dateTimeCaster = CastHelper.GetCastConverter(typeof(DateTime));
            Assert.That(dateTimeCaster("x"), Is.Null);
        }

        [Test]
        public void CastMethods_UnsupportedTypesThrow()
        {
            Assert.Throws<ArgumentException>(() => CastHelper.CastSByte(DateTime.UtcNow));
            Assert.Throws<ArgumentException>(() => CastHelper.CastNullableSByte(DateTime.UtcNow));
            Assert.Throws<ArgumentException>(() => CastHelper.CastBigInteger(DateTime.UtcNow));
        }

        [Test]
        public void GetCastConverter_ForKnownAndUnknownTypes()
        {
            var intCaster = CastHelper.GetCastConverter(typeof(int));
            Assert.That(intCaster(10L), Is.EqualTo(10));

            var bigIntCaster = CastHelper.GetCastConverter(typeof(BigInteger));
            Assert.That((BigInteger)bigIntCaster(123), Is.EqualTo(new BigInteger(123)));

            // unknown: returns identity when assignable, else null
            var objCaster = CastHelper.GetCastConverter(typeof(object));
            var o = new object();
            Assert.That(objCaster(o), Is.SameAs(o));

            var stringCaster = CastHelper.GetCastConverter(typeof(string));
            Assert.That(stringCaster("s"), Is.EqualTo("s"));
        }

        [Test]
        public void GetCastConverter_Generic_WrapsTypeCaster()
        {
            var caster = CastHelper.GetCastConverter<int>();
            Assert.That(caster(5L), Is.EqualTo(5));
        }
    }
}
