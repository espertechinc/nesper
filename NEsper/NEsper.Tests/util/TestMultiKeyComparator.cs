///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;

using com.espertech.esper.collection;

using NUnit.Framework;


namespace com.espertech.esper.util
{
    [TestFixture]
    public class TestMultiKeyComparator  
    {
    	IComparer<MultiKeyUntyped> comparator;
    	MultiKeyUntyped firstValues;
    	MultiKeyUntyped secondValues;
    	
        [Test]
    	public void TestCompareSingleProperty()
    	{
    		comparator = new MultiKeyComparator(new bool[] {false});
    
    		firstValues = new MultiKeyUntyped(new Object[] {3d});
    		secondValues = new MultiKeyUntyped(new Object[] {4d});
    		Assert.IsTrue(comparator.Compare(firstValues, secondValues) < 0);
    		
    		comparator = new MultiKeyComparator(new bool[] {true});
    
    		Assert.IsTrue(comparator.Compare(firstValues, secondValues) > 0);
    		Assert.IsTrue(comparator.Compare(firstValues, firstValues) == 0);
    	}
    	
        [Test]
    	public void TestCompareTwoProperties()
    	{
    		comparator = new MultiKeyComparator(new bool[] {false, false});
    
    		firstValues = new MultiKeyUntyped(new Object[] {3d, 3L});
    		secondValues = new MultiKeyUntyped(new Object[] {3d, 4L});
    		Assert.IsTrue(comparator.Compare(firstValues, secondValues) < 0);
    		
    		comparator = new MultiKeyComparator(new bool[] {false, true});
    		
    		Assert.IsTrue(comparator.Compare(firstValues, secondValues) > 0);
    		Assert.IsTrue(comparator.Compare(firstValues, firstValues) == 0);
    	}
    	
        [Test]
    	public void TestInvalid()
    	{
    		comparator = new MultiKeyComparator(new bool[] {false, false});
    	
    		firstValues = new MultiKeyUntyped(new Object[] {3d});
    		secondValues = new MultiKeyUntyped(new Object[] {3d, 4L});
    		try
    		{
    			comparator.Compare(firstValues, secondValues);
    			Assert.Fail();
    		}
    		catch(ArgumentException)
    		{
    			// Expected
    		}
    		
    		firstValues = new MultiKeyUntyped(new Object[] {3d});
    		secondValues = new MultiKeyUntyped(new Object[] {3d});
    		try
    		{
    			comparator.Compare(firstValues, secondValues);
    			Assert.Fail();
    		}
    		catch(ArgumentException)
    		{
    			// Expected
    		}
    
    	}
    }
}
