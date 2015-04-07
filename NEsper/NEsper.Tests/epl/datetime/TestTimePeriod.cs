///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using NUnit.Framework;

namespace com.espertech.esper.epl.datetime
{
    [TestFixture]
	public class TestTimePeriod
    {
        [Test]
	    public void TestLargestAbsoluteValue()
        {
	        Assert.AreEqual(1, (int) new TimePeriod().SetYears(1).LargestAbsoluteValue());
	        Assert.AreEqual(2, (int) new TimePeriod().SetMonths(2).LargestAbsoluteValue());
	        Assert.AreEqual(3, (int) new TimePeriod().SetDays(3).LargestAbsoluteValue());
	        Assert.AreEqual(4, (int) new TimePeriod().SetWeeks(4).LargestAbsoluteValue());
	        Assert.AreEqual(5, (int) new TimePeriod().SetHours(5).LargestAbsoluteValue());
	        Assert.AreEqual(6, (int) new TimePeriod().SetMinutes(6).LargestAbsoluteValue());
	        Assert.AreEqual(7, (int) new TimePeriod().SetSeconds(7).LargestAbsoluteValue());
	        Assert.AreEqual(8, (int) new TimePeriod().SetMillis(8).LargestAbsoluteValue());
	        Assert.AreEqual(10, (int) new TimePeriod().SetMillis(9).SetSeconds(10).SetHours(3).LargestAbsoluteValue());
	        Assert.AreEqual(1, (int) new TimePeriod()
                .SetYears(1)
                .SetMonths(1)
                .SetWeeks(1)
                .SetDays(1)
                .SetHours(1)
                .SetMinutes(1)
                .SetSeconds(1)
                .SetMillis(1)
                .LargestAbsoluteValue());
	    }

        [Test]
	    public void TestToStringISO8601()
        {
	        Assert.AreEqual("T10M", new TimePeriod().SetMinutes(10).ToStringISO8601());
	        Assert.AreEqual("9DT10M", new TimePeriod().SetMinutes(10).SetDays(9).ToStringISO8601());
	        Assert.AreEqual("4Y", new TimePeriod().SetYears(4).ToStringISO8601());
	        Assert.AreEqual("1Y1M1W1DT1H1M1S", new TimePeriod()
                .SetYears(1)
                .SetMonths(1)
                .SetWeeks(1)
                .SetDays(1)
                .SetHours(1)
                .SetMinutes(1)
                .SetSeconds(1)
                .SetMillis(1)
                .ToStringISO8601());
	    }
	}
} // end of namespace
