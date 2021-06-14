///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.schedule
{
    [TestFixture]
    public class TestScheduleSpec : AbstractCommonTest
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private void AssertInvalid(IDictionary<ScheduleUnit, ICollection<int>> unitValues)
        {
            try
            {
                new ScheduleSpec(unitValues, null, null, null);
                Assert.IsFalse(true);
            }
            catch (ArgumentException ex)
            {
                Log.Debug(".assertInvalid Expected exception, msg=" + ex.Message);
                // Expected exception
            }
        }

        [Test]
        public void TestCompress()
        {
            IDictionary<ScheduleUnit, ICollection<int>> unitValues = new Dictionary<ScheduleUnit, ICollection<int>>();
            unitValues = new ScheduleSpec().UnitValues;

            // Populate Month with all valid values
            ICollection<int> monthValues = new SortedSet<int>();
            for (var i = ScheduleUnit.MONTHS.Min(); i <= ScheduleUnit.MONTHS.Max(); i++)
            {
                monthValues.Add(i);
            }

            unitValues.Put(ScheduleUnit.MONTHS, monthValues);

            // Construct spec, test that month was replaced with wildcards
            var spec = new ScheduleSpec(unitValues, null, null, null);
            Assert.IsTrue(spec.UnitValues.Get(ScheduleUnit.MONTHS) == null);
        }

        [Test]
        public void TestValidate()
        {
            // Test all units missing
            IDictionary<ScheduleUnit, ICollection<int>> unitValues = new Dictionary<ScheduleUnit, ICollection<int>>();
            AssertInvalid(unitValues);

            // Test one unit missing
            unitValues = new ScheduleSpec().UnitValues;
            unitValues.Remove(ScheduleUnit.HOURS);
            AssertInvalid(unitValues);

            // Test all units are wildcards
            unitValues = new ScheduleSpec().UnitValues;
            new ScheduleSpec(unitValues, null, null, null);

            // Test invalid value in month
            ICollection<int> values = new SortedSet<int>();
            values.Add(0);
            unitValues.Put(ScheduleUnit.MONTHS, values);
            AssertInvalid(unitValues);

            // Test valid value in month
            values = new SortedSet<int>();
            values.Add(1);
            values.Add(5);
            unitValues.Put(ScheduleUnit.MONTHS, values);
            new ScheduleSpec(unitValues, null, null, null);
        }
    }
} // end of namespace
