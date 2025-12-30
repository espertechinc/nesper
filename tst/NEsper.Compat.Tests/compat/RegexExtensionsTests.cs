using System;
using System.Text.RegularExpressions;

using NUnit.Framework;

namespace com.espertech.esper.compat
{
    [TestFixture]
    public class RegexExtensionsTest
    {
        [Test]
        public void Compile_AddsAnchorsAndMatchesSameAsOriginalInside()
        {
            const string pattern = "foo.*bar";

            var regex = RegexExtensions.Compile(pattern, out var compiledText);

            // pattern text should be wrapped with ^ and $ but otherwise unchanged
            Assert.That(compiledText, Is.EqualTo("^" + pattern + "$"));

            Assert.That(regex.IsMatch("fooxbar"), Is.True);
            Assert.That(regex.IsMatch("xfooxbar"), Is.False);
            Assert.That(regex.IsMatch("fooxbarx"), Is.False);
        }

        [Test]
        public void Compile_DoesNotDoubleAnchor()
        {
            const string pattern = "^foo$";

            var regex = RegexExtensions.Compile(pattern, out var compiledText);

            // Should not add extra anchors if they are already present
            Assert.That(compiledText, Is.EqualTo(pattern));
            Assert.That(regex.IsMatch("foo"), Is.True);
            Assert.That(regex.IsMatch("xfoo"), Is.False);
        }

        [Test]
        public void Compile_InvalidPatternThrowsArgumentException()
        {
            const string invalidPattern = "["; // invalid regex

            Assert.That(
                () => RegexExtensions.Compile(invalidPattern, out _),
                Throws.InstanceOf<ArgumentException>());
        }

        [Test]
        public void IsMatchDebug_DelegatesToRegexIsMatch()
        {
            var regex = new Regex("^foo$");

            Assert.That(regex.IsMatchDebug("foo"), Is.True);
            Assert.That(regex.IsMatchDebug("xfoo"), Is.False);
        }
    }
}
