///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.compat.logging;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.multithread
{
	/// <summary>
	/// Test for multithread-safety and deterministic behavior when using insert-into.
	/// </summary>
    [TestFixture]
	public class TestMTStmtNamedWindowUnique 
	{
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

	    private EPServiceProvider _engine;

        [TearDown]
	    public void TearDown()
	    {
	        _engine.Initialize();
	    }

        [Test]
	    public void TestOrderedDeliverySpin()
	    {
	        Configuration config = SupportConfigFactory.GetConfiguration();
	        _engine = EPServiceProviderManager.GetDefaultProvider(config);
	        _engine.Initialize();
	    }
	}
} // end of namespace
