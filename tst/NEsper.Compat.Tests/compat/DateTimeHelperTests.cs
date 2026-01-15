using System;

using com.espertech.esper.compat.datetime;
using NUnit.Framework;

namespace com.espertech.esper.compat
{
    [TestFixture]
    public class DateTimeHelperTests
    {
        /// <summary>
        /// Verifies that converting a UTC DateTime to millis via UtcMillis and back via UtcFromMillis preserves the instant and Kind.
        /// </summary>
        [Test]
        public void UtcMillis_And_UtcFromMillis_RoundTrip_Utc()
        {
            var original = new DateTime(2024, 1, 2, 3, 4, 5, 123, DateTimeKind.Utc);

            var millis = original.UtcMillis();
            var roundTripped = millis.UtcFromMillis();

            Assert.That(roundTripped, Is.EqualTo(original));
            Assert.That(roundTripped.Kind, Is.EqualTo(DateTimeKind.Utc));
        }

        /// <summary>
        /// Verifies that converting a UTC DateTime to ticks via UtcTicks and back via UtcFromTicks preserves the instant and Kind.
        /// </summary>
        [Test]
        public void UtcTicks_And_UtcFromTicks_RoundTrip_Utc()
        {
            var original = new DateTime(2024, 1, 2, 3, 4, 5, 678, DateTimeKind.Utc).AddTicks(9); // extra ticks

            var ticks = DateTimeHelper.UtcTicks(original);
            var roundTripped = DateTimeHelper.UtcFromTicks(ticks);

            Assert.That(roundTripped, Is.EqualTo(original));
            Assert.That(roundTripped.Kind, Is.EqualTo(DateTimeKind.Utc));
        }

        /// <summary>
        /// Verifies that converting millis to a local DateTime via TimeFromMillis and back to UTC preserves the original UTC instant.
        /// </summary>
        [Test]
        public void TimeFromMillis_Local_RoundTripViaUtc()
        {
            var utc = new DateTime(2024, 1, 2, 3, 4, 5, 123, DateTimeKind.Utc);
            var millis = utc.UtcMillis();

            var localTime = millis.TimeFromMillis();

            Assert.That(localTime.ToUniversalTime(), Is.EqualTo(utc));
        }

        /// <summary>
        /// Verifies that UtcMicros is consistent with UtcTicks by checking it equals ticks divided by TICKS_PER_MICRO.
        /// </summary>
        [Test]
        public void UtcMicros_Computed_From_UtcTicks()
        {
            var utc = new DateTime(2024, 1, 2, 3, 4, 5, 678, DateTimeKind.Utc).AddTicks(9); // extra ticks

            var ticks = DateTimeHelper.UtcTicks(utc);
            var micros = DateTimeHelper.UtcMicros(utc);

            Assert.That(micros, Is.EqualTo(ticks / DateTimeHelper.TICKS_PER_MICRO));
        }

        /// <summary>
        /// Verifies that at exact millisecond boundaries, TimeFromMicros produces the same instant as TimeFromMillis.
        /// </summary>
        [Test]
        public void TimeFromMicros_Local_Matches_TimeFromMillis_At_Millisecond_Boundary()
        {
            var utc = new DateTime(2024, 1, 2, 3, 4, 5, 123, DateTimeKind.Utc);

            var millis = utc.UtcMillis();
            var micros = millis * 1000; // on a millisecond boundary

            var fromMillis = millis.TimeFromMillis();
            var fromMicros = micros.TimeFromMicros();

            Assert.That(fromMicros.ToUniversalTime(), Is.EqualTo(fromMillis.ToUniversalTime()));
        }

        /// <summary>
        /// Verifies that converting ticks obtained from UtcTicks through TimeFromTicks and back to UTC preserves the original instant.
        /// </summary>
        [Test]
        public void TimeFromTicks_Local_RoundTripViaUtc()
        {
            var utc = new DateTime(2024, 1, 2, 3, 4, 5, 678, DateTimeKind.Utc).AddTicks(9); // extra ticks
            var ticks = DateTimeHelper.UtcTicks(utc);

            var localTime = ticks.TimeFromTicks();

            Assert.That(localTime.ToUniversalTime(), Is.EqualTo(utc));
        }

        [Test]
        public void Print_FormatsDateTimeAndOffset()
        {
            var utc = new DateTime(2024, 1, 2, 3, 4, 5, 123, DateTimeKind.Utc);
            var dto = new DateTimeOffset(utc);

            Assert.That(utc.Print(), Is.EqualTo("2024-01-02 03:04:05.123"));
            Assert.That(dto.Print(), Is.EqualTo("2024-01-02 03:04:05.123"));
        }

        /// <summary>
        /// Verifies that WithYear, WithMonth, and WithDay change only the targeted component of a DateTime.
        /// </summary>
        [Test]
        public void WithYearMonthDay_ModifyIndividualComponents()
        {
            var dt = new DateTime(2020, 5, 15, 10, 20, 30, 40, DateTimeKind.Utc);

            var changedYear = dt.WithYear(2024);
            var changedMonth = dt.WithMonth(12);
            var changedDay = dt.WithDay(1);

            Assert.That(changedYear.Year, Is.EqualTo(2024));
            Assert.That(changedMonth.Month, Is.EqualTo(12));
            Assert.That(changedDay.Day, Is.EqualTo(1));
        }

        /// <summary>
        /// Verifies Truncate, Round, and Ceiling for the SECOND field on DateTime.
        /// </summary>
        [Test]
        public void Truncate_Round_Ceiling_WorkForSecond()
        {
            var dt = new DateTime(2024, 1, 2, 3, 4, 5, 600, DateTimeKind.Utc);

            var truncated = dt.Truncate(DateTimeFieldEnum.SECOND);
            var rounded = dt.Round(DateTimeFieldEnum.SECOND);
            var ceiling = dt.Ceiling(DateTimeFieldEnum.SECOND);

            Assert.That(truncated.Millisecond, Is.EqualTo(0));
            Assert.That(rounded.Second, Is.EqualTo(6));
            Assert.That(rounded.Millisecond, Is.EqualTo(0));
            Assert.That(ceiling.Second, Is.EqualTo(6));
            Assert.That(ceiling.Millisecond, Is.EqualTo(0));
        }
    }
}
