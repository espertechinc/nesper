///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using NUnit.Framework;

namespace com.espertech.esper.filter
{
    [TestFixture]
    public class TestFilterSpecParamComparator 
    {
        private FilterSpecParamComparator _comparator;
    
        [SetUp]
        public void SetUp()
        {
            _comparator = new FilterSpecParamComparator();
        }
    
        [Test]
        public void TestCompareOneByOne()
        {
            FilterOperator param1 = FilterOperator.EQUAL;
            FilterOperator param4 = FilterOperator.RANGE_CLOSED;
            FilterOperator param7 = FilterOperator.GREATER;
            FilterOperator param8 = FilterOperator.NOT_EQUAL;
            FilterOperator param9 = FilterOperator.IN_LIST_OF_VALUES;
            FilterOperator param10 = FilterOperator.NOT_RANGE_CLOSED;
            FilterOperator param11 = FilterOperator.NOT_IN_LIST_OF_VALUES;
    
            // Compare same comparison types
            Assert.IsTrue(_comparator.Compare(param8, param1) == 1);
            Assert.IsTrue(_comparator.Compare(param1, param8) == -1);
    
            Assert.IsTrue(_comparator.Compare(param4, param4) == 0);
    
            // Compare across comparison types
            Assert.IsTrue(_comparator.Compare(param7, param1) == 1);
            Assert.IsTrue(_comparator.Compare(param1, param7) == -1);
    
            Assert.IsTrue(_comparator.Compare(param4, param1) == 1);
            Assert.IsTrue(_comparator.Compare(param1, param4) == -1);
    
            // 'in' is before all but after equals
            Assert.IsTrue(_comparator.Compare(param9, param4) == -1);
            Assert.IsTrue(_comparator.Compare(param9, param9) == 0);
            Assert.IsTrue(_comparator.Compare(param9, param1) == 1);
    
            // inverted range is lower rank
            Assert.IsTrue(_comparator.Compare(param10, param1) == 1);
            Assert.IsTrue(_comparator.Compare(param10, param8) == -1);
    
            // not-in is lower rank
            Assert.IsTrue(_comparator.Compare(param11, param1) == 1);
            Assert.IsTrue(_comparator.Compare(param11, param8) == -1);
        }
    
        [Test]
        public void TestCompareAll()
        {
            var sorted =  new SortedSet<FilterOperator>(_comparator);
            var filterOperatorValues = EnumHelper.GetValues<FilterOperator>();
            foreach (var op in filterOperatorValues)
            {
                sorted.Add(op);
            }
    
            Assert.AreEqual(FilterOperator.EQUAL, sorted.First());
            Assert.AreEqual(FilterOperator.BOOLEAN_EXPRESSION, sorted.Last());
            Assert.AreEqual("[EQUAL, IS, IN_LIST_OF_VALUES, ADVANCED_INDEX, RANGE_OPEN, RANGE_HALF_OPEN, RANGE_HALF_CLOSED, RANGE_CLOSED, LESS, LESS_OR_EQUAL, GREATER_OR_EQUAL, GREATER, NOT_RANGE_CLOSED, NOT_RANGE_HALF_CLOSED, NOT_RANGE_HALF_OPEN, NOT_RANGE_OPEN, NOT_IN_LIST_OF_VALUES, NOT_EQUAL, IS_NOT, BOOLEAN_EXPRESSION]", sorted.Render());

            log.Debug(".testCompareAll " + sorted.Render());
        }
    
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
