using System;

using NUnit.Framework;

namespace com.espertech.esper.compat
{
    [TestFixture]
    public class DateTimeExTests
    {
        [Test]
        public void Constructor_FromDateTimeOffsetAndTimeZone_AlignsToTimeZone()
        {
            var tz = TimeZoneInfo.Utc;
            var dto = new DateTimeOffset(2024, 1, 2, 3, 4, 5, 123, TimeSpan.Zero);

            var ex = new DateTimeEx(dto, tz);

            Assert.That(ex.TimeZone, Is.EqualTo(tz));
            Assert.That(ex.UtcDateTime.ToUniversalTime(), Is.EqualTo(dto.ToUniversalTime()));
        }

        [Test]
        public void SetUtcMillis_RoundTripsMillis()
        {
            var tz = TimeZoneInfo.Utc;
            var ex = DateTimeEx.GetInstance(tz);

            var originalMillis = ex.UtcMillis;

            ex.SetUtcMillis(originalMillis);

            Assert.That(ex.UtcMillis, Is.EqualTo(originalMillis));
        }

        [Test]
        public void Set_IndividualFields_UpdateDateComponents()
        {
            var tz = TimeZoneInfo.Utc;
            var ex = new DateTimeEx(DateTimeOffsetHelper.Now(tz), tz);

            ex.Set(2024, 1, 2, 3, 4, 5, 123);

            Assert.That(ex.Year, Is.EqualTo(2024));
            Assert.That(ex.Month, Is.EqualTo(1));
            Assert.That(ex.Day, Is.EqualTo(2));
            Assert.That(ex.Hour, Is.EqualTo(3));
            Assert.That(ex.Minute, Is.EqualTo(4));
            Assert.That(ex.Second, Is.EqualTo(5));
            Assert.That(ex.Millisecond, Is.EqualTo(123));
        }

        [Test]
        public void CompareTo_ComparesUnderlyingDateTime()
        {
            var tz = TimeZoneInfo.Utc;
            var baseDto = new DateTimeOffset(2024, 1, 2, 3, 4, 5, 0, TimeSpan.Zero);

            var ex1 = new DateTimeEx(baseDto, tz);
            var ex2 = new DateTimeEx(baseDto.AddSeconds(10), tz);

            Assert.That(ex1.CompareTo(ex2), Is.LessThan(0));
            Assert.That(ex2.CompareTo(ex1), Is.GreaterThan(0));
        }

        [Test]
        public void AddDays_And_AddHours_ModifyDateTime()
        {
            var tz = TimeZoneInfo.Utc;
            var dto = new DateTimeOffset(2024, 1, 2, 3, 4, 5, 0, TimeSpan.Zero);
            var ex = new DateTimeEx(dto, tz);

            ex.AddDays(1).AddHours(2);

            Assert.That(ex.Year, Is.EqualTo(2024));
            Assert.That(ex.Month, Is.EqualTo(1));
            Assert.That(ex.Day, Is.EqualTo(3));
            Assert.That(ex.Hour, Is.EqualTo(5));
        }
    }
}
