///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using NUnit.Framework;

namespace com.espertech.esper.example.autoid
{
	[TestFixture]
	public class TestAutoIdSimMain
	{
		[Test]
	    public void testRun() 
	    {
            AutoIdSimMain main = new AutoIdSimMain(10, "AutoIdSample");
	        main.Run();
	    }
	}
}
