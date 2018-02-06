///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using NUnit.Framework;

namespace com.espertech.esper.filter
{
    [TestFixture]
    public class TestFilterSpecParamIn 
    {
        private FilterSpecParamIn _values;
    
        [Test]
        public void TestEquals()
        {
            _values = new FilterSpecParamIn(MakeLookupable("a"), FilterOperator.IN_LIST_OF_VALUES, GetList(new Object[] {"A", "B"}));
            var values2 = new FilterSpecParamIn(MakeLookupable("a"), FilterOperator.IN_LIST_OF_VALUES, GetList(new Object[] {"A"}));
            var values3 = new FilterSpecParamIn(MakeLookupable("a"), FilterOperator.IN_LIST_OF_VALUES, GetList(new Object[] {"A", "B"}));
            var values4 = new FilterSpecParamIn(MakeLookupable("a"), FilterOperator.IN_LIST_OF_VALUES, GetList(new Object[] {"A", "C"}));
    
            Assert.IsFalse(_values.Equals(new FilterSpecParamConstant(MakeLookupable("a"), FilterOperator.EQUAL, "a")));
            Assert.IsFalse(_values.Equals(values2));
            Assert.IsTrue(_values.Equals(values3));
            Assert.IsFalse(_values.Equals(values4));
        }
    
        private List<FilterSpecParamInValue> GetList(Object[] keys)
        {
            var list = new List<FilterSpecParamInValue>();
            for (var i = 0; i < keys.Length; i++)
            {
                list.Add(new FilterForEvalConstantAnyType(keys[i]));
            }
            return list;
        }
    
        private FilterSpecLookupable MakeLookupable(String fieldName) {
            return new FilterSpecLookupable(fieldName, null, null, false);
        }
    }
}
