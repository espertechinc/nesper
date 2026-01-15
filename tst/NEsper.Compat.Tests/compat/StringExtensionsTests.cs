using System;
using System.Linq;
using System.Text;

using NUnit.Framework;

namespace com.espertech.esper.compat
{
    [TestFixture]
    public class StringExtensionsTest
    {
        [Test]
        public void TestRepeatBasic()
        {
            var result = "ab".Repeat(3);
            Assert.That(result, Is.EqualTo(new[] { "ab", "ab", "ab" }));
        }

        [Test]
        public void TestRepeatZero()
        {
            var result = "x".Repeat(0);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void TestBetween()
        {
            const string s = "abcdef";
            Assert.That(s.Between(1, 4), Is.EqualTo("bcd"));
        }

        [Test]
        public void TestCapitalizeNullAndEdge()
        {
            string s = null;
            Assert.That(s.Capitalize(), Is.Null);

            Assert.That("".Capitalize(), Is.EqualTo(""));
            Assert.That("a".Capitalize(), Is.EqualTo("A"));
        }

        [Test]
        public void TestCapitalizeLonger()
        {
            Assert.That("hello".Capitalize(), Is.EqualTo("Hello"));
            Assert.That("Hello".Capitalize(), Is.EqualTo("Hello"));
        }

        [Test]
        public void TestSplitCsv()
        {
            var parts = "a,b,,c".SplitCsv();
            Assert.That(parts, Is.EqualTo(new[] { "a", "b", "", "c" }));
        }

        [Test]
        public void TestMatchesAddsAnchors()
        {
            // Pattern without anchors should be anchored at both ends: "bar" should only match "bar" exactly.
            Assert.That("bar".Matches("bar"), Is.True);
            Assert.That("foobar".Matches("bar"), Is.False);
            Assert.That("barfoo".Matches("bar"), Is.False);
        }

        [Test]
        public void TestMatchesWithExplicitAnchorsLeftUnchanged()
        {
            Assert.That("foobar".Matches("^foo.*bar$"), Is.True);
        }

        [Test]
        public void TestRegexSplit()
        {
            var parts = "a1b22c".RegexSplit(@"\d+");
            Assert.That(parts, Is.EqualTo(new[] { "a", "b", "c" }));
        }

        [Test]
        public void TestRegexReplaceAll()
        {
            var result = "a1b2c3".RegexReplaceAll(@"\d", "X");
            Assert.That(result, Is.EqualTo("aXbXcX"));
        }

        [Test]
        public void TestGetCrc32UsesUtf8ByDefault()
        {
            const string s = "123456789";
            var bytes = Encoding.UTF8.GetBytes(s);

            const uint expected = 0xCBF43926u;
            var fromBytes = (uint)bytes.GetCrc32();
            var fromString = (uint)s.GetCrc32();

            Assert.That(fromString, Is.EqualTo(expected));
            Assert.That(fromString, Is.EqualTo(fromBytes));
        }

        [Test]
        public void TestGetCrc32WithCustomEncoding()
        {
            const string s = "hello";
            var utf8 = (uint)s.GetCrc32(Encoding.UTF8);
            var unicode = (uint)s.GetCrc32(Encoding.Unicode);

            Assert.That(utf8, Is.Not.EqualTo(unicode));
        }

        [Test]
        public void TestGetUTF8Bytes()
        {
            const string s = "abc";
            var bytes = s.GetUTF8Bytes();

            Assert.That(bytes, Is.EqualTo(Encoding.UTF8.GetBytes(s)));
        }

        [Test]
        public void TestGetUnicodeBytes()
        {
            const string s = "abc";
            var bytes = s.GetUnicodeBytes();

            Assert.That(bytes, Is.EqualTo(Encoding.Unicode.GetBytes(s)));
        }

        [Test]
        public void TestRemoveWhitespace()
        {
            const string s = " a\t b\n c \r";
            Assert.That(s.RemoveWhitespace(), Is.EqualTo("abc"));
        }
    }
}
