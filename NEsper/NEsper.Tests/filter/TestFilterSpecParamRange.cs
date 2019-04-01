///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using NUnit.Framework;


namespace com.espertech.esper.filter
{
    [TestFixture]
    public class TestFilterSpecParamRange 
    {
        [Test]
        public void TestConstruct()
        {
            DoubleRange range = new DoubleRange(3d,3d);
    
            MakeParam("a", FilterOperator.RANGE_HALF_OPEN, range);
    
            try
            {
                MakeParam("a", FilterOperator.EQUAL, range);
                Assert.IsTrue(false);
            }
            catch (ArgumentException)
            {
                // Expected exception
            }
        }
    
        [Test]
        public void TestEquals()
        {
            FilterSpecParam c1 = MakeParam("a", FilterOperator.RANGE_CLOSED, new DoubleRange(5d, 6d));
            FilterSpecParam c2 = MakeParam("b", FilterOperator.RANGE_CLOSED, new DoubleRange(5d, 6d));
            FilterSpecParam c3 = MakeParam("a", FilterOperator.RANGE_HALF_CLOSED, new DoubleRange(5d, 6d));
            FilterSpecParam c4 = MakeParam("a", FilterOperator.RANGE_CLOSED, new DoubleRange(7d, 6d));
            FilterSpecParam c5 = MakeParam("a", FilterOperator.RANGE_CLOSED, new DoubleRange(5d, 6d));
    
            Assert.IsFalse(c1.Equals(c2));
            Assert.IsFalse(c1.Equals(c3));
            Assert.IsFalse(c1.Equals(c4));
            Assert.IsTrue(c1.Equals(c5));
        }
    
        private FilterSpecParamRange MakeParam(String propertyName, FilterOperator filterOp, DoubleRange doubleRange)
        {
            return new FilterSpecParamRange(MakeLookupable(propertyName), filterOp,
                new FilterForEvalConstantDouble(doubleRange.Min.Value),
                new FilterForEvalConstantDouble(doubleRange.Max.Value));
        }
    
        private FilterSpecLookupable MakeLookupable(String fieldName) {
            return new FilterSpecLookupable(fieldName, null, typeof(double?), false);
        }
    }
}
