using System;
using System.Text;

using NUnit.Framework;

namespace com.espertech.esper.compat
{
    [TestFixture]
    public class ByteExtensionsTest
    {
        [Test]
        public void TestToHexStringSequentialNibbles()
        {
            var bytes = new byte[]
            {
                0x00, 0x01, 0x02, 0x03,
                0x04, 0x05, 0x06, 0x07,
                0x08, 0x09, 0x0A, 0x0B,
                0x0C, 0x0D, 0x0E, 0x0F
            };

            Assert.That(bytes.ToHexString(), Is.EqualTo("000102030405060708090a0b0c0d0e0f"));
        }

        [Test]
        public void TestToHexStringEmpty()
        {
            var bytes = Array.Empty<byte>();
            Assert.That(bytes.ToHexString(), Is.EqualTo(string.Empty));
        }

        [Test]
        public void TestToHexStringSingleByte()
        {
            var bytes = new byte[] { 0xAB };
            Assert.That(bytes.ToHexString(), Is.EqualTo("ab"));
        }

        [Test]
        public void TestToHexStringMixedValues()
        {
            var bytes = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF };
            Assert.That(bytes.ToHexString(), Is.EqualTo("deadbeef"));
        }

        [Test]
        public void TestToHexString()
        {
            var bytes = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F };
            Assert.That(bytes.ToHexString(), Is.EqualTo("000102030405060708090a0b0c0d0e0f"));
        }

        [Test]
        public void TestGetCrc32KnownVector()
        {
            var bytes = Encoding.ASCII.GetBytes("123456789");

            // Standard CRC32 (IEEE) of "123456789" is 0xCBF43926.
            const uint expected = 0xCBF43926u;

            var actual = (uint)bytes.GetCrc32();
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void TestGetCrc32Empty()
        {
            var bytes = Array.Empty<byte>();

            var crc = (uint)bytes.GetCrc32();

            // Non-null operation: just assert that it returns the well-defined CRC32 of empty input
            // which is 0x00000000 for the standard IEEE CRC32 with the chosen init/xor values.
            Assert.That(crc, Is.EqualTo(0x00000000u));
        }

        [Test]
        public void TestGetCrc32SingleByte()
        {
            var bytes = new byte[] { 0x42 };

            var crc = (uint)bytes.GetCrc32();
            
            // Just ensure we get a non-zero, deterministic value.
            Assert.That(crc, Is.Not.EqualTo(0u));
        }

        [Test]
        public void TestGetCrc32PatternedArray()
        {
            var bytes = new byte[] { 0x00, 0xFF, 0x55, 0xAA, 0x10, 0x20, 0x30, 0x40 };

            var crc = (uint)bytes.GetCrc32();
            
            // Sanity checks: not zero and stable over repeated calls.
            Assert.That(crc, Is.Not.EqualTo(0u));
            Assert.That((uint)bytes.GetCrc32(), Is.EqualTo(crc));
        }
    }
}