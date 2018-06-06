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
    public class TestFilterSpecParamConstant 
    {
        [Test]
        public void TestConstruct()
        {
            new FilterSpecParamConstant(Make("a"), FilterOperator.GREATER, 5);
    
            try
            {
                new FilterSpecParamConstant(Make("a"), FilterOperator.RANGE_CLOSED, 5);
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
            FilterSpecParam c1 = new FilterSpecParamConstant(Make("a"),FilterOperator.GREATER, 5);
            FilterSpecParam c2 = new FilterSpecParamConstant(Make("a"),FilterOperator.GREATER, 6);
            FilterSpecParam c3 = new FilterSpecParamConstant(Make("b"), FilterOperator.GREATER, 5);
            FilterSpecParam c4 = new FilterSpecParamConstant(Make("a"),FilterOperator.EQUAL, 5);
            FilterSpecParam c5 = new FilterSpecParamConstant(Make("a"),FilterOperator.GREATER, 5);
    
            Assert.IsFalse(c1.Equals(c2));
            Assert.IsFalse(c1.Equals(c3));
            Assert.IsFalse(c1.Equals(c4));
            Assert.IsTrue(c1.Equals(c5));
        }
    
        private FilterSpecLookupable Make(String name) {
            return new FilterSpecLookupable(name, null, null, false);
        }
    }
}
