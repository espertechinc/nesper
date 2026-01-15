using System;

using com.espertech.esper.compat.datetime;
using NUnit.Framework;

namespace com.espertech.esper.compat
{
    [TestFixture]
    public class DateTimeOffsetHelperTests
    {
        /// <summary>
        /// Verifies that converting a DateTimeOffset to millis via UtcMillis and back via TimeFromMillis (UTC zone) preserves the instant.
        /// </summary>
        [Test]
        public void UtcMillis_And_TimeFromMillis_RoundTrip_Utc()
        {
            var original = new DateTimeOffset(2024, 1, 2, 3, 4, 5, 123, TimeSpan.Zero);

            var millis = original.UtcMillis();
            var roundTripped = millis.TimeFromMillis(TimeZoneInfo.Utc);

            Assert.That(roundTripped.UtcMillis(), Is.EqualTo(millis));
            Assert.That(roundTripped.ToUniversalTime(), Is.EqualTo(original.ToUniversalTime()));
        }

        /// <summary>
        /// Verifies that converting a DateTimeOffset to ticks via UtcTicks and back via TimeFromTicks (with zero offset) preserves the instant.
        /// </summary>
        [Test]
        public void UtcTicks_And_TimeFromTicks_RoundTrip_Utc()
        {
            var original = new DateTimeOffset(2024, 1, 2, 3, 4, 5, 678, TimeSpan.Zero).AddTicks(9);

            var ticks = original.UtcTicks();
            var roundTripped = ticks.TimeFromTicks(TimeSpan.Zero);

            Assert.That(roundTripped.ToUniversalTime(), Is.EqualTo(original.ToUniversalTime()));
        }

        /// <summary>
        /// Verifies that CreateDateTime constructs a DateTimeOffset whose offset matches the timezone-provided UTC offset.
        /// </summary>
        [Test]
        public void CreateDateTime_WithTimeZone_MatchesOffset()
        {
            var timeZone = TimeZoneInfo.Utc;

            var dto = DateTimeOffsetHelper.CreateDateTime(2024, 1, 2, 3, 4, 5, 123, timeZone);

            Assert.That(dto.Year, Is.EqualTo(2024));
            Assert.That(dto.Month, Is.EqualTo(1));
            Assert.That(dto.Offset, Is.EqualTo(timeZone.GetUtcOffset(dto.DateTime)));
        }

        /// <summary>
        /// Verifies that Normalize adjusts a DateTimeOffset to use the target timezone's UTC offset while representing the same instant.
        /// </summary>
        [Test]
        public void Normalize_AdjustsToTimeZoneOffset()
        {
            var utc = new DateTimeOffset(2024, 1, 2, 3, 4, 5, 0, TimeSpan.Zero);
            var timeZone = TimeZoneInfo.Utc;

            var normalized = utc.Normalize(timeZone);

            Assert.That(normalized.Offset, Is.EqualTo(timeZone.GetUtcOffset(normalized.DateTime)));
        }

        /// <summary>
        /// Verifies Truncate, Round, and Ceiling operations for the MINUTE field on DateTimeOffset.
        /// </summary>
        [Test]
        public void Truncate_Round_Ceiling_WorkForMinute()
        {
            var offset = TimeSpan.Zero;
            var dto = new DateTimeOffset(2024, 1, 2, 3, 4, 30, 500, offset);

            var truncated = dto.Truncate(DateTimeFieldEnum.MINUTE);
            var rounded = dto.Round(DateTimeFieldEnum.MINUTE);
            var ceiling = dto.Ceiling(DateTimeFieldEnum.MINUTE);

            Assert.That(truncated.Second, Is.EqualTo(0));
            Assert.That(truncated.Millisecond, Is.EqualTo(0));

            Assert.That(rounded.Minute, Is.EqualTo(5));
            Assert.That(rounded.Second, Is.EqualTo(0));

            Assert.That(ceiling.Minute, Is.EqualTo(5));
            Assert.That(ceiling.Second, Is.EqualTo(0));
        }
    }
}
