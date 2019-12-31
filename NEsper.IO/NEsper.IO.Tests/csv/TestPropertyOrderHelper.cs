///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;

using NUnit.Framework;

namespace com.espertech.esperio.csv
{
    [TestFixture]
    public class TestPropertyOrderHelper 
    {
    	private IDictionary<String, Object> _propertyTypes;

        [SetUp]
        public void SetUp()
    	{
    		_propertyTypes = new LinkedHashMap<String, Object>();
    		_propertyTypes.Put("myInt", typeof(int?));
    		_propertyTypes.Put("myDouble", typeof(double?));
    		_propertyTypes.Put("myString", typeof(String));
    	}
    
        [Test]
    	public void TestResolveTitleRow()
    	{
    		// Use first row
    		var firstRow = new String[] { "myDouble", "myInt", "myString" };
    		Assert.AreEqual(firstRow, CSVPropertyOrderHelper.ResolvePropertyOrder(firstRow, _propertyTypes));
    	}
    }
}
